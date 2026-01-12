using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SMSServices.Services;

public interface IGeminiService
{
    Task<GeminiResponse> AnalyzeErrorAsync(string category, string message, string? stackTrace, Dictionary<string, object>? metadata);
    Task<bool> TestConnectionAsync();
}

public class GeminiResponse
{
    public bool Success { get; set; }
    public string? Suggestion { get; set; }
    public string? Error { get; set; }
    public bool Cached { get; set; }
    public int TokensUsed { get; set; }
}

public class GeminiService : IGeminiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _endpoint;

    public GeminiService(
        IConfiguration configuration,
        ILogger<GeminiService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        
        _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        _endpoint = _configuration["Gemini:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/models";
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var url = $"{_endpoint}/{_model}:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = "Hello! Respond with just 'OK' if you can read this." }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gemini API test successful: {Response}", responseText);
                return true;
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API test failed: {StatusCode} - {Error}", response.StatusCode, errorText);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Gemini API test");
            return false;
        }
    }

    public async Task<GeminiResponse> AnalyzeErrorAsync(
        string category, 
        string message, 
        string? stackTrace, 
        Dictionary<string, object>? metadata)
    {
        try
        {
            var prompt = BuildPrompt(category, message, stackTrace, metadata);
            var url = $"{_endpoint}/{_model}:generateContent?key={_apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 500
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var suggestion = ExtractSuggestionFromResponse(responseText);
                
                return new GeminiResponse
                {
                    Success = true,
                    Suggestion = suggestion,
                    Cached = false,
                    TokensUsed = 0 // We'll calculate this later if needed
                };
            }
            else
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorText);
                
                return new GeminiResponse
                {
                    Success = false,
                    Error = $"API Error: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling Gemini API");
            return new GeminiResponse
            {
                Success = false,
                Error = "Failed to analyze error. Please try again."
            };
        }
    }

    private string BuildPrompt(string category, string message, string? stackTrace, Dictionary<string, object>? metadata)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"You are an expert debugging assistant analyzing a {category} error in a .NET/React application.");
        sb.AppendLine();
        sb.AppendLine("ERROR DETAILS:");
        sb.AppendLine($"Category: {category}");
        sb.AppendLine($"Message: {message}");
        
        if (!string.IsNullOrEmpty(stackTrace))
        {
            var truncatedStack = stackTrace.Length > 500 ? stackTrace.Substring(0, 500) + "..." : stackTrace;
            sb.AppendLine($"Stack Trace: {truncatedStack}");
        }

        if (metadata != null && metadata.Count > 0)
        {
            sb.AppendLine("Context:");
            foreach (var kvp in metadata)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("TASK:");
        sb.AppendLine("Provide a concise 4-5 line solution following this format:");
        sb.AppendLine();
        sb.AppendLine("Cause: [One sentence explaining what caused this error]");
        sb.AppendLine("Fix: [1-2 lines with specific code suggestion or action to take]");
        sb.AppendLine("Check: [1-2 lines where to verify or what to inspect]");
        sb.AppendLine();
        sb.AppendLine("Keep it practical and actionable. Focus on the most likely cause based on the error type.");

        return sb.ToString();
    }

    private string ExtractSuggestionFromResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content))
                {
                    if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString() ?? "No suggestion generated.";
                        }
                    }
                }
            }
            
            return "Unable to parse suggestion from response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            return "Error parsing AI response.";
        }
    }
}
