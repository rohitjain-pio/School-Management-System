using System.Collections.Concurrent;
using System.Security.Claims;

namespace SMSPrototype1.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, UserRateLimitInfo> _rateLimits = new();
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private static DateTime _lastCleanup = DateTime.UtcNow;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Cleanup old entries periodically
            if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
            {
                CleanupOldEntries();
                _lastCleanup = DateTime.UtcNow;
            }

            var endpoint = context.GetEndpoint();
            var path = context.Request.Path.Value ?? "";

            // Apply rate limiting to specific endpoints
            if (ShouldRateLimit(path, context.Request.Method))
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? context.Connection.RemoteIpAddress?.ToString() 
                    ?? "unknown";

                var key = $"{userId}:{path}";
                var rateLimitInfo = _rateLimits.GetOrAdd(key, _ => new UserRateLimitInfo());

                var (allowed, retryAfter) = rateLimitInfo.TryRequest(path);

                if (!allowed)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded",
                        message = $"Too many requests. Please try again in {retryAfter} seconds.",
                        retryAfter = retryAfter
                    });
                    return;
                }
            }

            await _next(context);
        }

        private bool ShouldRateLimit(string path, string method)
        {
            // Rate limit authentication endpoints
            if (path.Contains("/api/Auth/login", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.Contains("/api/Auth/register", StringComparison.OrdinalIgnoreCase)) return true;
            if (path.Contains("/api/Auth/refresh", StringComparison.OrdinalIgnoreCase)) return true;
            
            // Rate limit POST requests to chat endpoints
            if (method == "POST")
            {
                if (path.Contains("/api/ChatRooms", StringComparison.OrdinalIgnoreCase)) return true;
                if (path.Contains("/api/ChatRooms/join", StringComparison.OrdinalIgnoreCase)) return true;
                if (path.Contains("/api/ChatRooms/messages", StringComparison.OrdinalIgnoreCase)) return true;
            }

            // Rate limit file uploads
            if (path.Contains("/upload", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private void CleanupOldEntries()
        {
            var keysToRemove = _rateLimits
                .Where(kvp => kvp.Value.IsExpired())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimits.TryRemove(key, out _);
            }
        }
    }

    public class UserRateLimitInfo
    {
        private readonly object _lock = new();
        private Queue<DateTime> _requestTimes = new();
        private DateTime _lastRequest = DateTime.UtcNow;

        // Rate limits per endpoint
        private static readonly Dictionary<string, (int limit, TimeSpan window)> _limits = new()
        {
            { "/api/Auth/login", (5, TimeSpan.FromMinutes(1)) }, // 5 login attempts per minute
            { "/api/Auth/register", (3, TimeSpan.FromMinutes(1)) }, // 3 registrations per minute
            { "/api/Auth/refresh", (10, TimeSpan.FromMinutes(1)) }, // 10 refresh attempts per minute
            { "/api/ChatRooms", (5, TimeSpan.FromMinutes(1)) }, // 5 room creations per minute
            { "/api/ChatRooms/join", (10, TimeSpan.FromMinutes(1)) }, // 10 join attempts per minute
            { "/api/ChatRooms/messages", (30, TimeSpan.FromMinutes(1)) }, // 30 messages per minute
            { "/upload", (10, TimeSpan.FromHours(1)) } // 10 file uploads per hour
        };

        public (bool allowed, int retryAfter) TryRequest(string endpoint)
        {
            lock (_lock)
            {
                _lastRequest = DateTime.UtcNow;

                // Get limits for this endpoint
                var (limit, window) = GetLimitsForEndpoint(endpoint);

                // Remove old requests outside the window
                while (_requestTimes.Count > 0 && DateTime.UtcNow - _requestTimes.Peek() > window)
                {
                    _requestTimes.Dequeue();
                }

                // Check if limit exceeded
                if (_requestTimes.Count >= limit)
                {
                    var oldestRequest = _requestTimes.Peek();
                    var retryAfter = (int)(window - (DateTime.UtcNow - oldestRequest)).TotalSeconds + 1;
                    return (false, retryAfter);
                }

                // Allow request
                _requestTimes.Enqueue(DateTime.UtcNow);
                return (true, 0);
            }
        }

        private (int limit, TimeSpan window) GetLimitsForEndpoint(string endpoint)
        {
            foreach (var kvp in _limits)
            {
                if (endpoint.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            // Default limit
            return (100, TimeSpan.FromMinutes(1));
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow - _lastRequest > TimeSpan.FromHours(1);
        }
    }
}
