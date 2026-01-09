using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class Parents
    {
        public Guid Id { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        // Student Id
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
    }
}
