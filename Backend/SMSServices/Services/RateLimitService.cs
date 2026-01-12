using System.Collections.Concurrent;

namespace SMSServices.Services;

public interface IRateLimitService
{
    bool IsAllowed(string clientId, string operation);
    RateLimitStatus GetStatus(string clientId, string operation);
}

public class RateLimitStatus
{
    public bool Allowed { get; set; }
    public int RemainingRequests { get; set; }
    public TimeSpan? RetryAfter { get; set; }
    public string? Message { get; set; }
}

public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _requestHistory = new();
    private readonly int _maxRequestsPerMinute;
    private readonly int _maxRequestsPerDay;
    private readonly TimeSpan _minuteWindow = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _dayWindow = TimeSpan.FromDays(1);

    public RateLimitService(int maxPerMinute = 10, int maxPerDay = 100)
    {
        _maxRequestsPerMinute = maxPerMinute;
        _maxRequestsPerDay = maxPerDay;
    }

    public bool IsAllowed(string clientId, string operation)
    {
        var key = $"{clientId}:{operation}";
        var now = DateTime.UtcNow;

        var queue = _requestHistory.GetOrAdd(key, _ => new Queue<DateTime>());

        lock (queue)
        {
            // Remove old entries
            while (queue.Count > 0 && queue.Peek() < now - _dayWindow)
            {
                queue.Dequeue();
            }

            // Count requests in the last minute and day
            var requestsInLastMinute = queue.Count(t => t >= now - _minuteWindow);
            var requestsInLastDay = queue.Count;

            // Check limits
            if (requestsInLastMinute >= _maxRequestsPerMinute)
            {
                return false;
            }

            if (requestsInLastDay >= _maxRequestsPerDay)
            {
                return false;
            }

            // Add current request
            queue.Enqueue(now);
            return true;
        }
    }

    public RateLimitStatus GetStatus(string clientId, string operation)
    {
        var key = $"{clientId}:{operation}";
        var now = DateTime.UtcNow;

        if (!_requestHistory.TryGetValue(key, out var queue))
        {
            return new RateLimitStatus
            {
                Allowed = true,
                RemainingRequests = _maxRequestsPerMinute,
                Message = "No requests made yet"
            };
        }

        lock (queue)
        {
            // Clean up old entries
            while (queue.Count > 0 && queue.Peek() < now - _dayWindow)
            {
                queue.Dequeue();
            }

            var requestsInLastMinute = queue.Count(t => t >= now - _minuteWindow);
            var requestsInLastDay = queue.Count;

            if (requestsInLastMinute >= _maxRequestsPerMinute)
            {
                var oldestInMinute = queue.Where(t => t >= now - _minuteWindow).Min();
                var retryAfter = oldestInMinute.Add(_minuteWindow) - now;

                return new RateLimitStatus
                {
                    Allowed = false,
                    RemainingRequests = 0,
                    RetryAfter = retryAfter,
                    Message = $"Rate limit exceeded: {_maxRequestsPerMinute} requests per minute. Retry after {retryAfter.TotalSeconds:F0} seconds."
                };
            }

            if (requestsInLastDay >= _maxRequestsPerDay)
            {
                var oldestInDay = queue.Min();
                var retryAfter = oldestInDay.Add(_dayWindow) - now;

                return new RateLimitStatus
                {
                    Allowed = false,
                    RemainingRequests = 0,
                    RetryAfter = retryAfter,
                    Message = $"Daily limit exceeded: {_maxRequestsPerDay} requests per day. Retry after {retryAfter.TotalHours:F0} hours."
                };
            }

            return new RateLimitStatus
            {
                Allowed = true,
                RemainingRequests = Math.Min(
                    _maxRequestsPerMinute - requestsInLastMinute,
                    _maxRequestsPerDay - requestsInLastDay
                ),
                Message = "Request allowed"
            };
        }
    }
}
