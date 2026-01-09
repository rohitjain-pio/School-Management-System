using SMSDataModel.Model.enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateOnly JoiningDate { get; set; }
        public string Phone { get; set; }
        public Gender Gender { get; set; }
        public string Address { get; set; }
        // School
        public Guid SchoolId { get; set; }
        public School School { get; set; }
        // UserId
        public Guid? UserId { get; set; }
        public ApplicationUser User { get; set; }

    }
}
