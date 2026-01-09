using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateAttendanceRqstDto
    {
        public Guid StudentId { get; set; }
        public Guid SchoolId { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; }


    }
}
