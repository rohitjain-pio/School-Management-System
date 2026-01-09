using System.ComponentModel.DataAnnotations;

namespace SMSDataModel.Model.RequestDtos
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
