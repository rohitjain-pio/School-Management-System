using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMSDataModel.Model.Models;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateClassRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(5)]
        public string Section { get; set; }
        public Guid? ClassTeacherId { get; set; }
        [Required]
        public Guid SchoolId { get; set; }
    }
}