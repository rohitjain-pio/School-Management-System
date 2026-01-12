using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SMSServices.Services
{
    public class ChatService : IChatService
    {
        private readonly DataContext _context;
        private readonly IDistributedCache _cache;
        private readonly IMessageEncryptionService _encryptionService;

        // Track users in each room: RoomId -> (ConnectionId -> (Username, UserId))
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId)>> _roomUsers
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId)>>();

        // Track user message counts for flood protection: UserId -> (RoomId -> Queue<DateTime>)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<DateTime>>> _userMessageCounts
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<DateTime>>>();

        private const int MaxMessagesPerMinute = 30;
        private static readonly TimeSpan FloodProtectionWindow = TimeSpan.FromMinutes(1);

        public ChatService(DataContext context, IDistributedCache cache, IMessageEncryptionService encryptionService)
        {
            _context = context;
            _cache = cache;
            _encryptionService = encryptionService;
        }

        public async Task<ChatRoom?> GetRoomAsync(string roomId)
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
                return null;

            return await _context.ChatRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == roomGuid);
        }

        public async Task<bool> CreateRoomAsync(ChatRoom room)
        {
            _context.ChatRooms.Add(room);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ValidateRoomPasswordAsync(string roomId, string password)
        {
            var room = await GetRoomAsync(roomId);
            if (room == null || string.IsNullOrEmpty(room.PasswordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, room.PasswordHash);
        }

        public async Task<bool> IsUserParticipantAsync(string roomId, string userId)
        {
            if (!Guid.TryParse(roomId, out var roomGuid) || !Guid.TryParse(userId, out var userGuid))
                return false;

            return await _context.ChatRoomUsers
                .AnyAsync(ru => ru.RoomId == roomGuid && ru.UserId == userGuid);
        }

        public async Task<bool> ValidateRoomAccessAsync(string roomId, string userId)
        {
            var room = await GetRoomAsync(roomId);
            if (room == null || !room.IsActive)
                return false;

            // Check participant limit
            var currentCount = room.Participants?.Count ?? 0;
            if (currentCount >= room.MaxParticipants)
                return false;

            // Check if user is participant
            return await IsUserParticipantAsync(roomId, userId);
        }

        public void AddUserToRoom(string roomId, string connectionId, string username, string userId)
        {
            var users = _roomUsers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, (string, string)>());
            users.TryAdd(connectionId, (username, userId));
        }

        public (string Username, string UserId)? RemoveUserFromRoom(string roomId, string connectionId)
        {
            if (_roomUsers.TryGetValue(roomId, out var users))
            {
                if (users.TryRemove(connectionId, out var user))
                {
                    return user;
                }
            }
            return null;
        }

        public List<string> GetRoomUsernames(string roomId)
        {
            if (_roomUsers.TryGetValue(roomId, out var users))
            {
                return users.Values.Select(u => u.Username).Distinct().ToList();
            }
            return new List<string>();
        }

        public bool IsUserInRoom(string roomId, string connectionId)
        {
            return _roomUsers.TryGetValue(roomId, out var users) && users.ContainsKey(connectionId);
        }

        public bool CheckFloodProtection(string userId, string roomId)
        {
            var userRooms = _userMessageCounts.GetOrAdd(userId, _ => new ConcurrentDictionary<string, Queue<DateTime>>());
            var messageQueue = userRooms.GetOrAdd(roomId, _ => new Queue<DateTime>());

            lock (messageQueue)
            {
                var now = DateTime.UtcNow;

                // Remove old messages outside the window
                while (messageQueue.Count > 0 && now - messageQueue.Peek() > FloodProtectionWindow)
                {
                    messageQueue.Dequeue();
                }

                // Check if limit exceeded
                if (messageQueue.Count >= MaxMessagesPerMinute)
                {
                    return false;
                }

                messageQueue.Enqueue(now);
                return true;
            }
        }

        public void CleanupUserTracking(string userId)
        {
            _userMessageCounts.TryRemove(userId, out _);
        }

        public List<string> RemoveUserFromAllRooms(string connectionId)
        {
            var affectedRooms = new List<string>();
            foreach (var room in _roomUsers)
            {
                if (room.Value.TryRemove(connectionId, out _))
                {
                    affectedRooms.Add(room.Key);
                }
            }
            return affectedRooms;
        }

        public async Task UpdateRoomActivityAsync(string roomId)
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
                return;

            var room = await _context.ChatRooms.FindAsync(roomGuid);
            if (room != null)
            {
                room.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ChatMessage> SaveMessageAsync(string roomId, string userId, string content, bool isEncrypted)
        {
            if (!Guid.TryParse(roomId, out var roomGuid) || !Guid.TryParse(userId, out var userGuid))
                throw new InvalidOperationException("Invalid room or user ID");

            var chatMessage = new ChatMessage
            {
                RoomId = roomGuid,
                UserId = userGuid,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsDeleted = false,
                IsEdited = false
            };

            _context.ChatMessages.Add(chatMessage);
            await UpdateRoomActivityAsync(roomId);
            await _context.SaveChangesAsync();

            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetMessageHistoryAsync(string roomId, int count = 50)
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
                return new List<ChatMessage>();

            return await _context.ChatMessages
                .Where(m => m.RoomId == roomGuid && !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .Include(m => m.User)
                .ToListAsync();
        }

        public string EncryptMessage(string message, string roomId)
        {
            return _encryptionService.Encrypt(message, roomId);
        }

        public string DecryptMessage(string encryptedMessage, string roomId)
        {
            try
            {
                return _encryptionService.Decrypt(encryptedMessage, roomId);
            }
            catch
            {
                return "[Decryption failed]";
            }
        }

        public string SanitizeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            // Basic XSS prevention - encode HTML entities
            return System.Net.WebUtility.HtmlEncode(message);
        }
    }
}
