using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.Models
{
    public class SchoolClass
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        // Teacher Id
        public Guid? ClassTeacherId {  get; set; }
        public Teacher ClassTeacher { get; set; }
        //School Id
        public Guid SchoolId { get; set; }
        public School School { get; set; }
    }
}


