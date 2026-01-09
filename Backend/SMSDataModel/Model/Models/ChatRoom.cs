using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{  
    public class ChatRoom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Hashed password
        
        [Required]
        public string CreatedBy { get; set; } = string.Empty; // User ID
        
        public string? CreatedByUsername { get; set; } // Display name
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastActivityAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Privacy settings
        public RoomPrivacyLevel PrivacyLevel { get; set; } = RoomPrivacyLevel.Private;
        
        public int MaxParticipants { get; set; } = 50;
        
        public bool AllowRecording { get; set; } = true;
        
        public bool IsEncrypted { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<ChatMessage>? Messages { get; set; }
        public virtual ICollection<ChatRoomUser>? Participants { get; set; }
    }

    public enum RoomPrivacyLevel
    {
        Public = 0,      // Anyone can see and join
        Private = 1,     // Need password to join
        InviteOnly = 2   // Need invitation to join
    }
}
