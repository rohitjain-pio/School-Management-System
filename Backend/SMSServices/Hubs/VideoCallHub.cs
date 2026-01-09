using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSServices.Services;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SMSServices.Hubs
{
    [Authorize]
    public class VideoCallHub : Hub
    {
        private readonly DataContext _context;
        private readonly IRoomAccessTokenService _roomTokenService;
        private readonly IVideoRecordingService _recordingService;

        // Track users in each room: RoomId -> List of (ConnectionId, Username, UserId)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId, bool AudioEnabled, bool VideoEnabled)>> _roomParticipants 
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, (string Username, string UserId, bool AudioEnabled, bool VideoEnabled)>>();

        public VideoCallHub(
            DataContext context,
            IRoomAccessTokenService roomTokenService,
            IVideoRecordingService recordingService)
        {
            _context = context;
            _roomTokenService = roomTokenService;
            _recordingService = recordingService;
        }

        public async Task JoinVideoRoom(string roomId, string roomAccessToken)
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

            // Add to SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Track participant
            var participants = _roomParticipants.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, (string, string, bool, bool)>());
            participants.TryAdd(Context.ConnectionId, (username, userId, true, true)); // Default: audio and video enabled

            // Get list of existing participants (excluding the new joiner)
            var existingParticipants = participants
                .Where(p => p.Key != Context.ConnectionId)
                .Select(p => new 
                { 
                    connectionId = p.Key, 
                    username = p.Value.Username, 
                    userId = p.Value.UserId,
                    audioEnabled = p.Value.AudioEnabled,
                    videoEnabled = p.Value.VideoEnabled
                })
                .ToList();

            // Notify the new user about existing participants
            await Clients.Caller.SendAsync("ExistingParticipants", existingParticipants);

            // Notify other users about the new participant
            await Clients.OthersInGroup(roomId).SendAsync("UserJoinedCall", new 
            { 
                connectionId = Context.ConnectionId, 
                username = username, 
                userId = userId,
                audioEnabled = true,
                videoEnabled = true
            });

            // Check if recording is in progress
            var isRecording = _recordingService.IsRecording(Guid.Parse(roomId));
            if (isRecording)
            {
                await Clients.Caller.SendAsync("RecordingInProgress", true);
            }

            // Update room activity
            room.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send updated participant list to all
            await NotifyParticipantsUpdated(roomId);
        }

        public async Task LeaveVideoRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                if (participants.TryRemove(Context.ConnectionId, out var user))
                {
                    await Clients.OthersInGroup(roomId).SendAsync("UserLeftCall", Context.ConnectionId);
                    await NotifyParticipantsUpdated(roomId);
                }
            }
        }

        // WebRTC Signaling methods
        public async Task SendOffer(string targetConnectionId, object offer)
        {
            // Verify both users are in the same room
            var senderRoomId = GetUserRoomId(Context.ConnectionId);
            var targetRoomId = GetUserRoomId(targetConnectionId);

            if (senderRoomId == null || senderRoomId != targetRoomId)
            {
                throw new HubException("Unauthorized: Users not in same room");
            }

            await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        public async Task SendAnswer(string targetConnectionId, object answer)
        {
            // Verify both users are in the same room
            var senderRoomId = GetUserRoomId(Context.ConnectionId);
            var targetRoomId = GetUserRoomId(targetConnectionId);

            if (senderRoomId == null || senderRoomId != targetRoomId)
            {
                throw new HubException("Unauthorized: Users not in same room");
            }

            await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string targetConnectionId, object candidate)
        {
            // Verify both users are in the same room
            var senderRoomId = GetUserRoomId(Context.ConnectionId);
            var targetRoomId = GetUserRoomId(targetConnectionId);

            if (senderRoomId == null || senderRoomId != targetRoomId)
            {
                return; // Silently ignore invalid ICE candidates
            }

            await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        // Notify about media state changes (mute/unmute, video on/off)
        public async Task UpdateMediaState(string roomId, bool audioEnabled, bool videoEnabled)
        {
            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                if (participants.TryGetValue(Context.ConnectionId, out var participant))
                {
                    // Update participant state
                    participants[Context.ConnectionId] = (participant.Username, participant.UserId, audioEnabled, videoEnabled);

                    await Clients.OthersInGroup(roomId).SendAsync("ParticipantMediaStateChanged", 
                        Context.ConnectionId, audioEnabled, videoEnabled);
                }
            }
        }

        // Kick a participant (moderator only)
        public async Task KickParticipant(string roomId, string connectionIdToKick)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Unauthorized");
            }

            // Check if user is moderator
            var roomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(ru => ru.RoomId == Guid.Parse(roomId) && ru.UserId == Guid.Parse(userId));

            if (roomUser?.Role != "Moderator")
            {
                throw new HubException("Unauthorized: Only moderators can kick participants");
            }

            // Notify the kicked user
            await Clients.Client(connectionIdToKick).SendAsync("KickedFromRoom", "You have been removed from the call by a moderator");

            // Remove from room
            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                participants.TryRemove(connectionIdToKick, out _);
            }

            await Groups.RemoveFromGroupAsync(connectionIdToKick, roomId);
            await Clients.OthersInGroup(roomId).SendAsync("UserLeftCall", connectionIdToKick);
            await NotifyParticipantsUpdated(roomId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove from all rooms
            foreach (var room in _roomParticipants)
            {
                if (room.Value.TryRemove(Context.ConnectionId, out var user))
                {
                    await Clients.Group(room.Key).SendAsync("UserLeftCall", Context.ConnectionId);
                    await NotifyParticipantsUpdated(room.Key);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyParticipantsUpdated(string roomId)
        {
            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                var participantList = participants.Select(p => new 
                { 
                    connectionId = p.Key, 
                    username = p.Value.Username, 
                    userId = p.Value.UserId,
                    audioEnabled = p.Value.AudioEnabled,
                    videoEnabled = p.Value.VideoEnabled
                }).ToList();

                await Clients.Group(roomId).SendAsync("ParticipantsUpdated", participantList);
            }
        }

        private string? GetUserRoomId(string connectionId)
        {
            foreach (var room in _roomParticipants)
            {
                if (room.Value.ContainsKey(connectionId))
                {
                    return room.Key;
                }
            }
            return null;
        }
    }
}
