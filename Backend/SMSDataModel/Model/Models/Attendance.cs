using SMSDataModel.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class Attendance
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public Guid? UserId { get; set; }
        public ApplicationUser User { get; set; }
    } 
}
