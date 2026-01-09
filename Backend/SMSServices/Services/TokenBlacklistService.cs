using Microsoft.Extensions.Caching.Memory;
using SMSServices.ServicesInterfaces;

namespace SMSServices.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        private const string BlacklistPrefix = "blacklist_";

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task AddToBlacklistAsync(string tokenId, TimeSpan expiration)
        {
            var key = $"{BlacklistPrefix}{tokenId}";
            _cache.Set(key, true, expiration);
            return Task.CompletedTask;
        }

        public Task<bool> IsBlacklistedAsync(string tokenId)
        {
            var key = $"{BlacklistPrefix}{tokenId}";
            var isBlacklisted = _cache.TryGetValue(key, out _);
            return Task.FromResult(isBlacklisted);
        }

        public Task CleanupExpiredTokensAsync()
        {
            // Memory cache automatically removes expired entries
            return Task.CompletedTask;
        }
    }
}
