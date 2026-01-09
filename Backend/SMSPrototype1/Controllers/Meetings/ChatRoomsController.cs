using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSServices.Services;
using SMSPrototype1.Attributes;
using System.Security.Claims;
using BCrypt.Net;

namespace SMSPrototype1.Controllers.Meetings
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatRoomsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IRoomAccessTokenService _roomTokenService;
        private readonly IVideoRecordingService _recordingService;

        public ChatRoomsController(
            DataContext context,
            IRoomAccessTokenService roomTokenService,
            IVideoRecordingService recordingService)
        {
            _context = context;
            _roomTokenService = roomTokenService;
            _recordingService = recordingService;
        }

        /// <summary>
        /// Get all accessible rooms (excludes passwords and sensitive data)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDetailsDto>>> GetRooms()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isAdmin = userRoles.Any(r => r == "Admin" || r == "SuperAdmin" || r == "Principal" || r == "SchoolIncharge");

            var rooms = await _context.ChatRooms
                .Where(r => r.IsActive)
                .Select(r => new RoomDetailsDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedBy = r.CreatedBy,
                    CreatedByUsername = r.CreatedByUsername,
                    CreatedAt = r.CreatedAt,
                    PrivacyLevel = r.PrivacyLevel,
                    MaxParticipants = r.MaxParticipants,
                    CurrentParticipants = r.Participants != null ? r.Participants.Count : 0,
                    AllowRecording = r.AllowRecording,
                    IsEncrypted = r.IsEncrypted,
                    IsActive = r.IsActive,
                    IsUserModerator = r.CreatedBy == userId || isAdmin
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(rooms);
        }

        /// <summary>
        /// Create a new room with hashed password
        /// </summary>
        [HttpPost]
        [ValidateModel]
        public async Task<ActionResult<RoomDetailsDto>> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { ok = false, message = "User not authenticated" });

            // Check if user already has too many active rooms
            var userRoomCount = await _context.ChatRooms
                .CountAsync(r => r.CreatedBy == userId && r.IsActive);

            if (userRoomCount >= 10) // Limit per user
            {
                return BadRequest(new { ok = false, message = "You have reached the maximum number of active rooms (10)" });
            }

            // Sanitize input
            var sanitizedName = SanitizeInput(request.Name);
            var sanitizedDescription = request.Description != null ? SanitizeInput(request.Description) : null;

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var room = new ChatRoom
            {
                Name = sanitizedName,
                Description = sanitizedDescription,
                PasswordHash = passwordHash,
                CreatedBy = userId,
                CreatedByUsername = username,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                PrivacyLevel = request.PrivacyLevel,
                MaxParticipants = request.MaxParticipants,
                AllowRecording = request.AllowRecording,
                IsEncrypted = true,
                IsActive = true
            };

            _context.ChatRooms.Add(room);

            // Add creator as moderator
            var roomUser = new ChatRoomUser
            {
                UserId = Guid.Parse(userId),
                RoomId = room.Id,
                JoinedAt = DateTime.UtcNow,
                Role = "Moderator"
            };

            _context.ChatRoomUsers.Add(roomUser);
            await _context.SaveChangesAsync();

            var roomDetails = new RoomDetailsDto
            {
                Id = room.Id,
                Name = room.Name,
                Description = room.Description,
                CreatedBy = room.CreatedBy,
                CreatedByUsername = room.CreatedByUsername,
                CreatedAt = room.CreatedAt,
                PrivacyLevel = room.PrivacyLevel,
                MaxParticipants = room.MaxParticipants,
                CurrentParticipants = 1,
                AllowRecording = room.AllowRecording,
                IsEncrypted = room.IsEncrypted,
                IsActive = room.IsActive,
                IsUserModerator = true
            };

            return Ok(roomDetails);
        }

        /// <summary>
        /// Join a room with password verification and return access token
        /// </summary>
        [HttpPost("join")]
        [ValidateModel]
        public async Task<ActionResult<JoinRoomResponse>> JoinRoom([FromBody] JoinRoomRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { ok = false, message = "User not authenticated" });

            var room = await _context.ChatRooms
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId);

            if (room == null)
                return NotFound(new JoinRoomResponse { Ok = false, Message = "Room not found" });

            if (!room.IsActive)
                return BadRequest(new JoinRoomResponse { Ok = false, Message = "Room is not active" });

            // Check participant limit
            var currentParticipantCount = room.Participants?.Count ?? 0;
            if (currentParticipantCount >= room.MaxParticipants)
            {
                return BadRequest(new JoinRoomResponse { Ok = false, Message = "Room is full" });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, room.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new JoinRoomResponse { Ok = false, Message = "Incorrect password" });
            }

            // Check if user is already in room
            var existingParticipant = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(ru => ru.RoomId == request.RoomId && ru.UserId == Guid.Parse(userId));

            if (existingParticipant == null)
            {
                // Add user to room
                var roomUser = new ChatRoomUser
                {
                    UserId = Guid.Parse(userId),
                    RoomId = request.RoomId,
                    JoinedAt = DateTime.UtcNow,
                    Role = room.CreatedBy == userId ? "Moderator" : "Participant"
                };

                _context.ChatRoomUsers.Add(roomUser);
            }

            // Update room activity
            room.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate room access token
            var role = room.CreatedBy == userId ? "Moderator" : "Participant";
            var roomAccessToken = _roomTokenService.GenerateRoomAccessToken(
                Guid.Parse(userId),
                request.RoomId,
                username,
                role
            );

            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isAdmin = userRoles.Any(r => r == "Admin" || r == "SuperAdmin" || r == "Principal" || r == "SchoolIncharge");

            var response = new JoinRoomResponse
            {
                Ok = true,
                Message = "Successfully joined room",
                RoomAccessToken = roomAccessToken,
                RoomDetails = new RoomDetailsDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Description = room.Description,
                    CreatedBy = room.CreatedBy,
                    CreatedByUsername = room.CreatedByUsername,
                    CreatedAt = room.CreatedAt,
                    PrivacyLevel = room.PrivacyLevel,
                    MaxParticipants = room.MaxParticipants,
                    CurrentParticipants = room.Participants?.Count ?? 0,
                    AllowRecording = room.AllowRecording,
                    IsEncrypted = room.IsEncrypted,
                    IsActive = room.IsActive,
                    IsUserModerator = role == "Moderator" || isAdmin
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Delete a room (only creator or admin)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var room = await _context.ChatRooms.FindAsync(id);
            if (room == null)
                return NotFound(new { ok = false, message = "Room not found" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            bool isAdmin = userRoles.Any(r => r == "Admin" || r == "SuperAdmin" || r == "Principal" || r == "SchoolIncharge");
            bool isCreator = room.CreatedBy == userId;
            
            if (!isAdmin && !isCreator)
            {
                return Forbid();
            }

            // Delete related records with proper cascade
            var messages = await _context.ChatMessages.Where(m => m.RoomId == id).ToListAsync();
            _context.ChatMessages.RemoveRange(messages);

            var roomUsers = await _context.ChatRoomUsers.Where(ru => ru.RoomId == id).ToListAsync();
            _context.ChatRoomUsers.RemoveRange(roomUsers);

            _context.ChatRooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Start recording a video call
        /// </summary>
        [HttpPost("recording/start")]
        [ValidateModel]
        public async Task<ActionResult> StartRecording([FromBody] StartRecordingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { ok = false, message = "User not authenticated" });

            // Validate room access token
            var roomIdFromToken = _roomTokenService.GetRoomIdFromToken(request.RoomAccessToken);
            if (roomIdFromToken != request.RoomId)
            {
                return Unauthorized(new { ok = false, message = "Invalid room access token" });
            }

            var room = await _context.ChatRooms.FindAsync(request.RoomId);
            if (room == null)
                return NotFound(new { ok = false, message = "Room not found" });

            if (!room.AllowRecording)
                return BadRequest(new { ok = false, message = "Recording is not allowed in this room" });

            // Check if user is moderator
            var roomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(ru => ru.RoomId == request.RoomId && ru.UserId == Guid.Parse(userId));

            if (roomUser?.Role != "Moderator")
            {
                return Forbid();
            }

            try
            {
                var recordingId = await _recordingService.StartRecording(
                    request.RoomId,
                    Guid.Parse(userId),
                    username
                );

                return Ok(new { ok = true, recordingId = recordingId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Stop recording a video call
        /// </summary>
        [HttpPost("recording/stop")]
        [ValidateModel]
        public async Task<ActionResult> StopRecording([FromBody] StopRecordingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { ok = false, message = "User not authenticated" });

            var recordingInfo = await _recordingService.GetRecordingInfo(request.RecordingId);
            if (recordingInfo == null)
                return NotFound(new { ok = false, message = "Recording not found" });

            // Check if user is the one who started recording or moderator
            if (recordingInfo.StartedBy.ToString() != userId)
            {
                var roomUser = await _context.ChatRoomUsers
                    .FirstOrDefaultAsync(ru => ru.RoomId == recordingInfo.RoomId && ru.UserId == Guid.Parse(userId));

                if (roomUser?.Role != "Moderator")
                {
                    return Forbid();
                }
            }

            try
            {
                await _recordingService.StopRecording(request.RecordingId);
                return Ok(new { ok = true, message = "Recording stopped successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get recordings for a room
        /// </summary>
        [HttpGet("{roomId}/recordings")]
        public async Task<ActionResult> GetRoomRecordings(Guid roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { ok = false, message = "User not authenticated" });

            // Check if user has access to this room
            var roomUser = await _context.ChatRoomUsers
                .FirstOrDefaultAsync(ru => ru.RoomId == roomId && ru.UserId == Guid.Parse(userId));

            if (roomUser == null)
            {
                return Forbid();
            }

            var recordings = await _recordingService.GetRoomRecordings(roomId);
            return Ok(recordings);
        }

        // Helper method to sanitize input (prevent XSS)
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove potentially dangerous characters
            return System.Text.RegularExpressions.Regex.Replace(
                input,
                @"[<>""'%;()&+]",
                string.Empty
            );
        }
    }
}
