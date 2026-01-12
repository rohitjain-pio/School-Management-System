using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using SMSServices.Services;
using System.Security.Claims;

namespace SMSServices.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IRoomAccessTokenService _roomTokenService;

        public ChatHub(IChatService chatService, IRoomAccessTokenService roomTokenService)
        {
            _chatService = chatService;
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

            // Validate room access using service
            if (!await _chatService.ValidateRoomAccessAsync(roomId, userId))
            {
                throw new HubException("Room access denied");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            // Track user in room using service
            _chatService.AddUserToRoom(roomId, Context.ConnectionId, username, userId);
            
            // Update room activity
            await _chatService.UpdateRoomActivityAsync(roomId);

            // Notify all users in room about updated user list
            await NotifyUserListUpdated(roomId);
            
            // Notify room that user joined
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", username);
        }

        // Leave a room
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            
            // Remove user from tracking using service
            var user = _chatService.RemoveUserFromRoom(roomId, Context.ConnectionId);
            if (user.HasValue)
            {
                // Notify remaining users
                await NotifyUserListUpdated(roomId);
                await Clients.OthersInGroup(roomId).SendAsync("UserLeft", user.Value.Username);
            }
        }

        public async Task SendTyping(string roomId, string user)
        {
            // Verify user is in room using service
            if (!_chatService.IsUserInRoom(roomId, Context.ConnectionId))
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

            // Verify user is in room using service
            if (!_chatService.IsUserInRoom(roomId, Context.ConnectionId))
            {
                throw new HubException("Unauthorized: Not in room");
            }

            // Flood protection using service
            if (!_chatService.CheckFloodProtection(userIdStr, roomId))
            {
                throw new HubException("Rate limit exceeded. Please slow down.");
            }

            // Get room to check if encryption is enabled
            var room = await _chatService.GetRoomAsync(roomId);
            if (room == null)
            {
                throw new HubException("Room not found");
            }

            // Sanitize message using service
            var sanitizedMessage = _chatService.SanitizeMessage(message);

            // Encrypt message if room has encryption enabled
            string contentToStore = sanitizedMessage;
            if (room.IsEncrypted)
            {
                try
                {
                    contentToStore = _chatService.EncryptMessage(sanitizedMessage, roomId);
                }
                catch (Exception ex)
                {
                    throw new HubException("Failed to encrypt message: " + ex.Message);
                }
            }

            // Save message using service
            var chatMessage = await _chatService.SaveMessageAsync(roomId, userIdStr, contentToStore, room.IsEncrypted);

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
            // Verify user is authenticated
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Unauthorized");
            }

            // Verify user is a participant using service
            if (!await _chatService.IsUserParticipantAsync(roomId, userId))
            {
                throw new HubException("Unauthorized: Not a room participant");
            }

            var room = await _chatService.GetRoomAsync(roomId);
            if (room == null)
            {
                throw new HubException("Room not found");
            }

            // Get message history from service
            var messages = await _chatService.GetMessageHistoryAsync(roomId, count);

            // Decrypt messages if needed
            var decryptedMessages = messages.Select(m =>
            {
                var content = m.Content;
                if (room.IsEncrypted)
                {
                    content = _chatService.DecryptMessage(m.Content, roomId);
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
            // Remove user from all rooms using service
            var affectedRooms = _chatService.RemoveUserFromAllRooms(Context.ConnectionId);
            
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            foreach (var roomId in affectedRooms)
            {
                await NotifyUserListUpdated(roomId);
                await Clients.Group(roomId).SendAsync("UserLeft", username);
            }

            // Clean up message count tracking
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _chatService.CleanupUserTracking(userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyUserListUpdated(string roomId)
        {
            var userList = _chatService.GetRoomUsernames(roomId);
            await Clients.Group(roomId).SendAsync("UserListUpdated", userList);
        }
    }
}
