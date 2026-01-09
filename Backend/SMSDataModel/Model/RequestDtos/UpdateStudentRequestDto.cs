using SMSDataModel.Model.enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class UpdateStudentRequestDto
    {
      
        public string SRNumber { get; set; }
       
        public int RollNumber { get; set; }

      
        [EmailAddress]
        public string Email { get; set; }
        
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        public string LastName { get; set; }
        
        public DateOnly DOB { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
        
        public Guid ClassId { get; set; }
    }
}
