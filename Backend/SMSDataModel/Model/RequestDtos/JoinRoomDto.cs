using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class JoinRoomDto
    {
        public Guid RoomId { get; set; }
        public string Password { get; set; }
    }
}
