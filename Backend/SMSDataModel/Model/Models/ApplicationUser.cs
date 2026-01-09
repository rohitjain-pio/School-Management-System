using Microsoft.AspNetCore.Identity;

namespace SMSDataModel.Model.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public Guid SchoolId { get; set; }
        public School School { get; set; } = null!;

        // Security-related properties
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIp { get; set; }

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Helper methods
        public bool IsLockedOut => LockoutEndDate.HasValue && LockoutEndDate.Value > DateTime.UtcNow;
        
        public void IncrementFailedLoginAttempts()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                LockoutEndDate = DateTime.UtcNow.AddMinutes(30);
            }
        }

        public void ResetFailedLoginAttempts()
        {
            FailedLoginAttempts = 0;
            LockoutEndDate = null;
        }
    }
}
