using Microsoft.AspNetCore.Mvc;
using SMSServices.Services;
using Microsoft.AspNetCore.Authorization;

namespace SMSPrototype1.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow access in development without authentication
public class DebugController : ControllerBase
{
    private readonly IErrorLogService _errorLogService;
    private readonly IGeminiService _geminiService;
    private readonly IErrorSanitizationService _sanitizationService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IGeminiCacheService _cacheService;

    public DebugController(
        IErrorLogService errorLogService, 
        IGeminiService geminiService,
        IErrorSanitizationService sanitizationService,
        IRateLimitService rateLimitService,
        IGeminiCacheService cacheService)
    {
        _errorLogService = errorLogService;
        _geminiService = geminiService;
        _sanitizationService = sanitizationService;
        _rateLimitService = rateLimitService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get all logged errors (Development only)
    /// </summary>
    [HttpGet("errors")]
    public ActionResult<List<ErrorLogEntry>> GetErrors([FromQuery] string? category = null)
    {
        #if !DEBUG
        return NotFound();
        #endif

        if (string.IsNullOrEmpty(category))
        {
            return Ok(_errorLogService.GetAllErrors());
        }

        return Ok(_errorLogService.GetErrorsByCategory(category));
    }

    /// <summary>
    /// Get error count (Development only)
    /// </summary>
    [HttpGet("errors/count")]
    public ActionResult<int> GetErrorCount()
    {
        #if !DEBUG
        return NotFound();
        #endif

        return Ok(_errorLogService.GetErrorCount());
    }

    /// <summary>
    /// Clear all errors (Development only)
    /// </summary>
    [HttpDelete("errors")]
    public IActionResult ClearErrors()
    {
        #if !DEBUG
        return NotFound();
        #endif

        _errorLogService.ClearErrors();
        return NoContent();
    }

    /// <summary>
    /// Test endpoint to generate sample errors (Development only)
    /// </summary>
    [HttpPost("errors/test")]
    public IActionResult TestError([FromQuery] string type = "backend")
    {
        #if !DEBUG
        return NotFound();
        #endif

        switch (type.ToLower())
        {
            case "database":
                _errorLogService.LogError("Database", "Test database connection error", "at TestMethod() line 42", "DebugController");
                break;
            case "validation":
                _errorLogService.LogError("Validation", "Test validation error: Email is required", null, "DebugController");
                break;
            case "auth":
                _errorLogService.LogError("Auth", "Test authentication error: Token expired", null, "DebugController");
                break;
            default:
                _errorLogService.LogError("Backend", "Test backend error", "at TestMethod() line 42", "DebugController");
                break;
        }

        return Ok(new { message = $"Test {type} error logged" });
    }

    /// <summary>
    /// Test Gemini API connection (Development only)
    /// </summary>
    [HttpGet("gemini/test")]
    public async Task<IActionResult> TestGeminiConnection()
    {
        #if !DEBUG
        return NotFound();
        #endif

        var isConnected = await _geminiService.TestConnectionAsync();
        
        if (isConnected)
        {
            return Ok(new 
            { 
                success = true, 
                message = "✅ Gemini API connection successful! Your API key is working correctly.",
                model = "gemini-2.5-flash"
            });
        }
        else
        {
            return BadRequest(new 
            { 
                success = false, 
                message = "❌ Gemini API connection failed. Check your API key in appsettings.Development.json"
            });
        }
    }

    /// <summary>
    /// Analyze an error using Gemini AI (Development only)
    /// </summary>
    [HttpPost("analyze-error")]
    public async Task<IActionResult> AnalyzeError([FromBody] AnalyzeErrorRequest request)
    {
        #if !DEBUG
        return NotFound();
        #endif

        if (string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Category and Message are required" });
        }

        // Get client identifier (use IP or a session ID in production)
        var clientId = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Check rate limit
        if (!_rateLimitService.IsAllowed(clientId, "gemini-analyze"))
        {
            var status = _rateLimitService.GetStatus(clientId, "gemini-analyze");
            return StatusCode(429, new 
            { 
                error = "Rate limit exceeded", 
                message = status.Message,
                retryAfter = status.RetryAfter?.TotalSeconds
            });
        }

        // Sanitize input data
        var sanitizedMessage = _sanitizationService.SanitizeErrorMessage(request.Message);
        var sanitizedStackTrace = _sanitizationService.SanitizeStackTrace(request.StackTrace);
        var sanitizedMetadata = _sanitizationService.SanitizeMetadata(request.Metadata);

        // Check cache first
        var cacheKey = _cacheService.GenerateCacheKey(request.Category, sanitizedMessage, sanitizedStackTrace);
        var cachedSuggestion = await _cacheService.GetCachedSuggestionAsync(cacheKey);

        if (cachedSuggestion != null)
        {
            return Ok(new
            {
                success = true,
                suggestion = cachedSuggestion,
                cached = true,
                tokensUsed = 0
            });
        }

        // Call Gemini API
        var response = await _geminiService.AnalyzeErrorAsync(
            request.Category,
            sanitizedMessage,
            sanitizedStackTrace,
            sanitizedMetadata
        );

        if (!response.Success)
        {
            return BadRequest(new
            {
                success = false,
                error = response.Error,
                message = "Failed to analyze error with Gemini AI"
            });
        }

        // Cache the suggestion
        if (!string.IsNullOrEmpty(response.Suggestion))
        {
            await _cacheService.SetCachedSuggestionAsync(cacheKey, response.Suggestion);
        }

        return Ok(new
        {
            success = true,
            suggestion = response.Suggestion,
            cached = false,
            tokensUsed = response.TokensUsed
        });
    }
}

public class AnalyzeErrorRequest
{
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
