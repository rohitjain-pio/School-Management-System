using Microsoft.EntityFrameworkCore;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using SMSServices.ServicesInterfaces;
using System.Security.Cryptography;

namespace SMSServices.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DataContext _context;

        public RefreshTokenService(DataContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = GenerateSecureToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken?> RotateRefreshTokenAsync(string oldToken, string ipAddress)
        {
            var token = await GetRefreshTokenAsync(oldToken);
            
            if (token == null || !token.IsActive)
                return null;

            // Generate new token
            var newToken = await GenerateRefreshTokenAsync(token.UserId, ipAddress);

            // Revoke old token
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReplacedByToken = newToken.Token;

            await _context.SaveChangesAsync();

            return newToken;
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            
            if (refreshToken == null || !refreshToken.IsActive)
                return;

            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, string ipAddress)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt != null)
                .Where(rt => rt.CreatedAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days for audit
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
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
