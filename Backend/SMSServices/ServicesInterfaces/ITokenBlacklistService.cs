namespace SMSServices.ServicesInterfaces
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string tokenId, TimeSpan expiration);
        Task<bool> IsBlacklistedAsync(string tokenId);
        Task CleanupExpiredTokensAsync();
    }
}
