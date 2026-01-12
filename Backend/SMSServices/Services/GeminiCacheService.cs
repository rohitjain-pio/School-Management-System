using System.Collections.Concurrent;

namespace SMSServices.Services;

public interface IGeminiCacheService
{
    Task<string?> GetCachedSuggestionAsync(string cacheKey);
    Task SetCachedSuggestionAsync(string cacheKey, string suggestion, TimeSpan? expiration = null);
    string GenerateCacheKey(string category, string message, string? stackTrace);
}

public class GeminiCacheService : IGeminiCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);

    private class CacheEntry
    {
        public string Suggestion { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public Task<string?> GetCachedSuggestionAsync(string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<string?>(entry.Suggestion);
            }

            // Remove expired entry
            _cache.TryRemove(cacheKey, out _);
        }

        return Task.FromResult<string?>(null);
    }

    public Task SetCachedSuggestionAsync(string cacheKey, string suggestion, TimeSpan? expiration = null)
    {
        var expirationTime = expiration ?? _defaultExpiration;
        var entry = new CacheEntry
        {
            Suggestion = suggestion,
            ExpiresAt = DateTime.UtcNow.Add(expirationTime)
        };

        _cache[cacheKey] = entry;

        // Clean up expired entries periodically (simple approach)
        CleanupExpiredEntries();

        return Task.CompletedTask;
    }

    public string GenerateCacheKey(string category, string message, string? stackTrace)
    {
        // Create a simple hash-based key from the error details
        var stackTracePreview = stackTrace != null 
            ? stackTrace.Substring(0, Math.Min(100, stackTrace.Length)) 
            : "";
        var keySource = $"{category}:{message}:{stackTracePreview}";
        var hash = keySource.GetHashCode();
        return $"gemini:{category}:{hash}";
    }

    private void CleanupExpiredEntries()
    {
        // Only clean up if cache is getting large (> 1000 entries)
        if (_cache.Count < 1000)
            return;

        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }
}
