using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class TeacherSubject
    {
        public Guid Id { get; set; }
        // Assigned Teacher
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        // Assigned Subject
        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; }
        // Assigned Class
        public Guid SchoolClassId { get; set; }
        public SchoolClass SchoolClass { get; set; }
    }
}
