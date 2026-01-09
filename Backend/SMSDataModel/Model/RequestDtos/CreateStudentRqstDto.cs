using SMSDataModel.Model.enums;
using SMSDataModel.Model.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SMSDataModel.Model.RequestDtos
{
    public class CreateStudentRqstDto
    {
        [Required]
        public string SRNumber { get; set; }
        [Required]
        public int RollNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        public DateOnly DOB { get; set; }
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
        [Required]
        public Guid ClassId { get; set; }

    }
}

