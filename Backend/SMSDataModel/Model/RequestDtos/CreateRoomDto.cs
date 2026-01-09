using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateRoomDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
    }
}
