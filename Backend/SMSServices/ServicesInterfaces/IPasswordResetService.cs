using SMSDataModel.Model.Models;

namespace SMSServices.ServicesInterfaces
{
    public interface IPasswordResetService
    {
        Task<PasswordResetToken> GeneratePasswordResetTokenAsync(Guid userId, string ipAddress);
        Task<PasswordResetToken?> ValidateTokenAsync(string token);
        Task MarkTokenAsUsedAsync(Guid tokenId);
        Task CleanupExpiredTokensAsync();
    }
}
