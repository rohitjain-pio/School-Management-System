using System.Text.RegularExpressions;

namespace SMSServices.Services;

public interface IErrorSanitizationService
{
    string SanitizeErrorMessage(string message);
    string SanitizeStackTrace(string? stackTrace);
    Dictionary<string, object> SanitizeMetadata(Dictionary<string, object>? metadata);
}

public class ErrorSanitizationService : IErrorSanitizationService
{
    // Patterns for sensitive data detection
    private static readonly Regex EmailPattern = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex JwtPattern = new Regex(@"eyJ[a-zA-Z0-9_-]*\.eyJ[a-zA-Z0-9_-]*\.[a-zA-Z0-9_-]*", RegexOptions.Compiled);
    private static readonly Regex ApiKeyPattern = new Regex(@"(api[_-]?key|apikey|api[_-]?secret|bearer)[\s:=]+['""]?([a-zA-Z0-9_\-]{20,})['""]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PasswordPattern = new Regex(@"(password|pwd|pass|passwd)[\s:=]+['""]?([^'""\s]{6,})['""]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ConnectionStringPattern = new Regex(@"(server|data source|uid|user id|password|pwd)[\s]*=[\s]*[^;]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IpAddressPattern = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex FilePathPattern = new Regex(@"[A-Za-z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*", RegexOptions.Compiled);
    private static readonly Regex GuidPattern = new Regex(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", RegexOptions.Compiled);

    public string SanitizeErrorMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return message;

        var sanitized = message;

        // Replace sensitive patterns
        sanitized = EmailPattern.Replace(sanitized, "[EMAIL]");
        sanitized = JwtPattern.Replace(sanitized, "[JWT_TOKEN]");
        sanitized = ApiKeyPattern.Replace(sanitized, "$1=[API_KEY]");
        sanitized = PasswordPattern.Replace(sanitized, "$1=[PASSWORD]");
        sanitized = ConnectionStringPattern.Replace(sanitized, "[CONNECTION_STRING]");
        sanitized = IpAddressPattern.Replace(sanitized, "[IP_ADDRESS]");
        sanitized = FilePathPattern.Replace(sanitized, "[FILE_PATH]");
        sanitized = GuidPattern.Replace(sanitized, "[GUID]");

        return sanitized;
    }

    public string SanitizeStackTrace(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return stackTrace ?? string.Empty;

        var sanitized = stackTrace;

        // Sanitize file paths in stack traces (keep method names but remove full paths)
        sanitized = FilePathPattern.Replace(sanitized, "[PATH]");
        sanitized = GuidPattern.Replace(sanitized, "[GUID]");

        // Truncate very long stack traces (keep first 2000 chars for token efficiency)
        if (sanitized.Length > 2000)
        {
            sanitized = sanitized.Substring(0, 2000) + "\n... [truncated for analysis]";
        }

        return sanitized;
    }

    public Dictionary<string, object> SanitizeMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata == null || metadata.Count == 0)
            return new Dictionary<string, object>();

        var sanitized = new Dictionary<string, object>();

        foreach (var kvp in metadata)
        {
            var key = kvp.Key;
            var value = kvp.Value?.ToString() ?? string.Empty;

            // Skip sensitive keys entirely
            if (IsSensitiveKey(key))
            {
                sanitized[key] = "[REDACTED]";
                continue;
            }

            // Sanitize value
            var sanitizedValue = SanitizeErrorMessage(value);
            sanitized[key] = sanitizedValue;
        }

        return sanitized;
    }

    private bool IsSensitiveKey(string key)
    {
        var lowerKey = key.ToLowerInvariant();
        return lowerKey.Contains("password") ||
               lowerKey.Contains("secret") ||
               lowerKey.Contains("token") ||
               lowerKey.Contains("apikey") ||
               lowerKey.Contains("connectionstring") ||
               lowerKey.Contains("authorization");
    }
}
