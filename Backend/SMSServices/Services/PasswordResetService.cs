using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using System.Security.Cryptography;

namespace SMSServices.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly DataContext _context;

        public PasswordResetService(DataContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken> GeneratePasswordResetTokenAsync(Guid userId, string ipAddress)
        {
            // Invalidate any existing tokens
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed && !t.IsExpired)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
            }

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15-minute expiration
                CreatedByIp = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _context.PasswordResetTokens.AddAsync(resetToken);
            await _context.SaveChangesAsync();

            return resetToken;
        }

        public async Task<PasswordResetToken?> ValidateTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.IsValid);
        }

        public async Task MarkTokenAsUsedAsync(Guid tokenId)
        {
            var token = await _context.PasswordResetTokens.FindAsync(tokenId);
            
            if (token != null)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(t => t.CreatedAt < DateTime.UtcNow.AddDays(-7)) // Keep for 7 days for audit
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[32];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
