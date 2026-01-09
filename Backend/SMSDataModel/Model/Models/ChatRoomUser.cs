using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class ChatRoomUser
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public string Role { get; set; } = "Participant";

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("RoomId")]
        public virtual ChatRoom Room { get; set; }
    }

}
