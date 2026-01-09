using SMSDataModel.Model.Models;

namespace SMSServices.ServicesInterfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken?> RotateRefreshTokenAsync(string oldToken, string ipAddress);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
        Task RevokeAllUserTokensAsync(Guid userId, string ipAddress);
        Task CleanupExpiredTokensAsync();
    }
}
