using System.Collections.Concurrent;

namespace SMSServices.Services;

public class ErrorLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public interface IErrorLogService
{
    void LogError(string category, string message, string? stackTrace = null, string? source = null, Dictionary<string, object>? metadata = null);
    List<ErrorLogEntry> GetAllErrors();
    List<ErrorLogEntry> GetErrorsByCategory(string category);
    void ClearErrors();
    int GetErrorCount();
}

public class ErrorLogService : IErrorLogService
{
    private readonly ConcurrentQueue<ErrorLogEntry> _errors = new();
    private const int MaxErrors = 1000; // Limit to prevent memory issues

    public void LogError(string category, string message, string? stackTrace = null, string? source = null, Dictionary<string, object>? metadata = null)
    {
        var entry = new ErrorLogEntry
        {
            Category = category,
            Message = message,
            StackTrace = stackTrace,
            Source = source,
            Metadata = metadata ?? new Dictionary<string, object>()
        };

        _errors.Enqueue(entry);

        // Trim old errors if we exceed max
        while (_errors.Count > MaxErrors)
        {
            _errors.TryDequeue(out _);
        }
    }

    public List<ErrorLogEntry> GetAllErrors()
    {
        return _errors.ToList();
    }

    public List<ErrorLogEntry> GetErrorsByCategory(string category)
    {
        return _errors.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public void ClearErrors()
    {
        _errors.Clear();
    }

    public int GetErrorCount()
    {
        return _errors.Count;
    }
}
