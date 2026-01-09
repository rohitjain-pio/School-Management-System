using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class UpdateClassRequestDto
    {
       
        [MaxLength(50)]
        public string Name { get; set; }
       
        [MaxLength(5)]
        public string Section { get; set; }
        public Guid? ClassTeacherId { get; set; }
    }
}
