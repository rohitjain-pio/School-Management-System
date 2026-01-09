using System.ComponentModel.DataAnnotations;
using SMSDataModel.Model.Models;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateRoomRequest
    {
        [Required(ErrorMessage = "Room name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Room name must be between 3 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Room name can only contain letters, numbers, spaces, hyphens, and underscores")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public RoomPrivacyLevel PrivacyLevel { get; set; } = RoomPrivacyLevel.Private;

        [Range(2, 100, ErrorMessage = "Max participants must be between 2 and 100")]
        public int MaxParticipants { get; set; } = 50;

        public bool AllowRecording { get; set; } = true;
    }

    public class JoinRoomRequest
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class JoinRoomResponse
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RoomAccessToken { get; set; }
        public RoomDetailsDto? RoomDetails { get; set; }
    }

    public class RoomDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? CreatedByUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public RoomPrivacyLevel PrivacyLevel { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public bool AllowRecording { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsActive { get; set; }
        public bool IsUserModerator { get; set; }
    }

    public class StartRecordingRequest
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public string RoomAccessToken { get; set; } = string.Empty;
    }

    public class StopRecordingRequest
    {
        [Required]
        public string RecordingId { get; set; } = string.Empty;
    }
}
