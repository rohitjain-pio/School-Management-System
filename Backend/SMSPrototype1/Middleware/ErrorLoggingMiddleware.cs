using SMSServices.Services;
using System.Net;
using System.Text.Json;

namespace SMSPrototype1.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLogService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            // Categorize the error
            var category = CategorizeException(ex);

            // Log to error service
            errorLogService.LogError(
                category: category,
                message: ex.Message,
                stackTrace: ex.StackTrace,
                source: ex.Source,
                metadata: new Dictionary<string, object>
                {
                    { "Path", context.Request.Path },
                    { "Method", context.Request.Method },
                    { "QueryString", context.Request.QueryString.ToString() },
                    { "ExceptionType", ex.GetType().Name }
                }
            );

            // Return error response
            await HandleExceptionAsync(context, ex);
        }
    }

    private static string CategorizeException(Exception ex)
    {
        var exceptionType = ex.GetType().Name;
        var message = ex.Message.ToLower();

        // Database errors
        if (exceptionType.Contains("Sql") || exceptionType.Contains("Database") || 
            message.Contains("database") || message.Contains("connection"))
            return "Database";

        // Validation errors
        if (exceptionType.Contains("Validation") || exceptionType.Contains("ArgumentException") ||
            message.Contains("validation") || message.Contains("invalid"))
            return "Validation";

        // Auth errors
        if (exceptionType.Contains("Unauthorized") || exceptionType.Contains("Authentication") ||
            message.Contains("unauthorized") || message.Contains("token"))
            return "Auth";

        // Default to Backend
        return "Backend";
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = new
            {
                message = exception.Message,
                type = exception.GetType().Name,
                #if DEBUG
                stackTrace = exception.StackTrace
                #endif
            }
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
