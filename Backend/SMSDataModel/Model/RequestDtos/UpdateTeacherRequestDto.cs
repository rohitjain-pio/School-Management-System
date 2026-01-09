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
    public class UpdateTeacherRequestDto
    {
       
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }      
        [Phone]
        [MaxLength(12)]
        public string Phone { get; set; }
        public string Address { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
    }
}
