using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSServices.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SMSServices.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly DataContext _context;
        private readonly IMessageEncryptionService _encryptionService;
        private readonly IRoomAccessTokenService _roomTokenService;

        // Track users in each room: RoomId -> List of (ConnectionId, Username, UserId)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId)>> _roomUsers 
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId)>>();

        // Track user message counts for flood protection: UserId -> (RoomId -> Queue<DateTime>)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<DateTime>>> _userMessageCounts
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, Queue<DateTime>>>();

        private const int MaxMessagesPerMinute = 30;
        private static readonly TimeSpan FloodProtectionWindow = TimeSpan.FromMinutes(1);

        public ChatHub(
            DataContext context,
            IMessageEncryptionService encryptionService,
            IRoomAccessTokenService roomTokenService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _roomTokenService = roomTokenService;
        }

        // Join a room with access token validation
        public async Task JoinRoom(string roomId, string roomAccessToken)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Unauthorized: User not authenticated");
            }

            // Validate room access token
            var tokenRoomId = _roomTokenService.GetRoomIdFromToken(roomAccessToken);
            if (tokenRoomId == null || tokenRoomId.ToString() != roomId)
            {
                throw new HubException("Unauthorized: Invalid room access token");
            }

            // Verify room exists and user has access
            var room = await _context.ChatRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == Guid.Parse(roomId));

            if (room == null)
            {
                throw new HubException("Room not found");
            }

            if (!room.IsActive)
            {
                throw new HubException("Room is not active");
            }

            // Check participant limit
            var currentCount = room.Participants?.Count ?? 0;
            if (currentCount >= room.MaxParticipants)
            {
                throw new HubException("Room is full");
            }

            // Verify user is a participant
            var isParticipant = await _context.ChatRoomUsers
                .AnyAsync(ru => ru.RoomId == Guid.Parse(roomId) && ru.UserId == Guid.Parse(userId));

            if (!isParticipant)
            {
                throw new HubException("Unauthorized: Not a room participant");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            // Track user in room
            var users = _roomUsers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, (string, string)>());
            users.TryAdd(Context.ConnectionId, (username, userId));
            
            // Update room activity
            room.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify all users in room about updated user list
            await NotifyUserListUpdated(roomId);
            
            // Notify room that user joined
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", username);
        }

        // Leave a room
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            
            // Remove user from tracking
            if (_roomUsers.TryGetValue(roomId, out var users))
            {
                if (users.TryRemove(Context.ConnectionId, out var user))
                {
                    // Notify remaining users
                    await NotifyUserListUpdated(roomId);
                    await Clients.OthersInGroup(roomId).SendAsync("UserLeft", user.Username);
                }
            }
        }

        public async Task SendTyping(string roomId, string user)
        {
            // Verify user is in room
            if (!_roomUsers.TryGetValue(roomId, out var users) || !users.ContainsKey(Context.ConnectionId))
            {
                return; // Silently ignore if not in room
            }

            await Clients.OthersInGroup(roomId).SendAsync("ReceiveTyping", user);
        }

        // Send a message with encryption and flood protection
        public async Task SendMessage(string roomId, string message)
        {
            // Validate message
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new HubException("Message cannot be empty");
            }

            if (message.Length > 1000)
            {
                throw new HubException("Message too long (max 1000 characters)");
            }

            // Get authenticated user
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new HubException("Unauthorized: Invalid user");
            }

            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                throw new HubException("Invalid room ID");
            }

            // Verify user is in room
            if (!_roomUsers.TryGetValue(roomId, out var users) || !users.ContainsKey(Context.ConnectionId))
            {
                throw new HubException("Unauthorized: Not in room");
            }

            // Flood protection
            if (!CheckFloodProtection(userIdStr, roomId))
            {
                throw new HubException("Rate limit exceeded. Please slow down.");
            }

            // Get room to check if encryption is enabled
            var room = await _context.ChatRooms.FindAsync(roomGuid);
            if (room == null)
            {
                throw new HubException("Room not found");
            }

            // Sanitize message content (basic XSS prevention)
            var sanitizedMessage = SanitizeMessage(message);

            // Encrypt message if room has encryption enabled
            string contentToStore = sanitizedMessage;
            if (room.IsEncrypted)
            {
                try
                {
                    contentToStore = _encryptionService.Encrypt(sanitizedMessage, roomId);
                }
                catch (Exception ex)
                {
                    throw new HubException("Failed to encrypt message: " + ex.Message);
                }
            }

            // Save encrypted message to database
            var chatMessage = new ChatMessage
            {
                RoomId = roomGuid,
                UserId = userId,
                Content = contentToStore,
                Timestamp = DateTime.UtcNow,
                IsDeleted = false,
                IsEdited = false
            };

            _context.ChatMessages.Add(chatMessage);

            // Update room activity
            room.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Broadcast message to all users in room (send decrypted version)
            await Clients.Group(roomId).SendAsync("ReceiveMessage", new
            {
                id = chatMessage.Id,
                sender = username,
                content = sanitizedMessage, // Send decrypted message
                timestamp = chatMessage.Timestamp.ToString("o"), // ISO 8601 format
                isEncrypted = room.IsEncrypted
            });
        }

        // Load message history for a room with decryption
        public async Task<List<object>> LoadMessageHistory(string roomId, int count = 50)
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                throw new HubException("Invalid room ID");
            }

            // Verify user is in room
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Unauthorized");
            }

            var isParticipant = await _context.ChatRoomUsers
                .AnyAsync(ru => ru.RoomId == roomGuid && ru.UserId == Guid.Parse(userId));

            if (!isParticipant)
            {
                throw new HubException("Unauthorized: Not a room participant");
            }

            var room = await _context.ChatRooms.FindAsync(roomGuid);
            if (room == null)
            {
                throw new HubException("Room not found");
            }

            // Get last N messages from database
            var messages = await _context.ChatMessages
                .Where(m => m.RoomId == roomGuid && !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .Include(m => m.User)
                .ToListAsync();

            // Decrypt messages if needed
            var decryptedMessages = messages.Select(m =>
            {
                var content = m.Content;
                if (room.IsEncrypted)
                {
                    try
                    {
                        content = _encryptionService.Decrypt(m.Content, roomId);
                    }
                    catch
                    {
                        content = "[Decryption failed]";
                    }
                }

                return new
                {
                    id = m.Id,
                    sender = m.User?.UserName ?? "Unknown",
                    content = content,
                    timestamp = m.Timestamp.ToString("o"),
                    isEdited = m.IsEdited,
                    isEncrypted = room.IsEncrypted
                };
            })
            .Reverse()
            .Cast<object>()
            .ToList();

            return decryptedMessages;
        }

        // Handle disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove user from all rooms they were in
            foreach (var room in _roomUsers)
            {
                if (room.Value.TryRemove(Context.ConnectionId, out var user))
                {
                    await NotifyUserListUpdated(room.Key);
                    await Clients.Group(room.Key).SendAsync("UserLeft", user.Username);
                }
            }

            // Clean up message count tracking
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userMessageCounts.TryRemove(userId, out _);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyUserListUpdated(string roomId)
        {
            if (_roomUsers.TryGetValue(roomId, out var users))
            {
                var userList = users.Values.Select(u => u.Username).Distinct().ToList();
                await Clients.Group(roomId).SendAsync("UserListUpdated", userList);
            }
        }

        private bool CheckFloodProtection(string userId, string roomId)
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

        private string SanitizeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            // Basic XSS prevention - encode HTML entities
            message = System.Net.WebUtility.HtmlEncode(message);

            // Optional: Filter profanity (implement as needed)
            // message = ProfanityFilter(message);

            return message;
        }
    }
}
