using SMSDataModel.Model.enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        public string SRNumber { get; set; }
        public int RollNumber { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public DateOnly DOB { get; set; }
        public Gender Gender { get; set; }
        // Class Id
        public Guid ClassId { get; set; }
        public SchoolClass Class { get; set; }
        // User Id
        public Guid? UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
