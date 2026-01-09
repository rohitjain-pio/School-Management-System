# API Security Best Practices - School Management System

**Last Updated:** January 9, 2026  
**Version:** 1.0  
**Audience:** Development Team, Security Team

---

## Table of Contents

- [Authentication Best Practices](#authentication-best-practices)
- [Authorization Best Practices](#authorization-best-practices)
- [Input Validation](#input-validation)
- [Output Encoding](#output-encoding)
- [Session Management](#session-management)
- [Cryptography](#cryptography)
- [Error Handling](#error-handling)
- [Logging and Monitoring](#logging-and-monitoring)
- [API Design Security](#api-design-security)
- [Third-Party Dependencies](#third-party-dependencies)
- [Deployment Security](#deployment-security)
- [Code Review Checklist](#code-review-checklist)

---

## Authentication Best Practices

### 1. JWT Token Security

**DO ✓**

```csharp
// Use strong secret key (minimum 256 bits)
var securityKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])
);

// Recommended: 512-bit key
// Secret: At least 64 characters (hexadecimal: 128 chars)

// Set appropriate expiration
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddHours(3), // Short-lived access token
    SigningCredentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256Signature // HS256 minimum
    ),
    Issuer = "SMS-API",
    Audience = "SMS-Client"
};

// Validate issuer and audience
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["JWT:Issuer"],
            ValidAudience = Configuration["JWT:Audience"],
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero // No tolerance for expiration
        };
    });
```

**DON'T ✗**

```csharp
// Weak secret key
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret"));

// No expiration
Expires = DateTime.MaxValue,

// Algorithm confusion attack vulnerability
SecurityAlgorithms.None // Never use "none" algorithm

// Skip validation
ValidateIssuer = false,
ValidateAudience = false,
ValidateLifetime = false
```

### 2. Password Security

**DO ✓**

```csharp
// Use BCrypt with high work factor
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    // Work factor 12 = 2^12 iterations = 4096
}

// Verify without timing attacks
public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}

// Enforce strong password policy
public class PasswordValidator
{
    public static bool IsValid(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(ch => "!@#$%^&*".Contains(ch))) return false;
        return true;
    }
}

// Check against common passwords
private static readonly HashSet<string> CommonPasswords = new()
{
    "Password123!", "Admin123!", "Welcome123!", // ...
};

public bool IsCommonPassword(string password)
{
    return CommonPasswords.Contains(password);
}
```

**DON'T ✗**

```csharp
// Weak hashing (MD5, SHA1, SHA256 alone)
var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));

// Storing plain text passwords
user.Password = password; // NEVER DO THIS

// Weak password requirements
if (password.Length >= 4) // Too weak
```

### 3. Multi-Factor Authentication (MFA)

**DO ✓**

```csharp
// Generate secure TOTP secret
using (var rng = RandomNumberGenerator.Create())
{
    var secretBytes = new byte[20]; // 160 bits
    rng.GetBytes(secretBytes);
    var secret = Base32.Encode(secretBytes);
}

// Validate TOTP with time window
public bool ValidateTOTP(string userCode, string secret)
{
    var totp = new Totp(Base32.Decode(secret));
    
    // Check current time window and 1 window before/after (90 seconds total)
    return totp.VerifyTotp(userCode, out _, new VerificationWindow(1, 1));
}

// Require MFA for sensitive roles
[Authorize(Policy = "RequireMFA")]
public class AdminController : ControllerBase { }
```

**DON'T ✗**

```csharp
// Predictable OTP codes
var otp = new Random().Next(100000, 999999); // Weak randomness

// No time window validation (allows replay attacks)

// Optional MFA for all users (should be required for admins)
```

### 4. Refresh Token Security

**DO ✓**

```csharp
public class RefreshToken
{
    public string Token { get; set; } // Cryptographically random
    public DateTime ExpiresAt { get; set; } // 7 days
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; } // Token rotation
    public bool IsActive => RevokedAt == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

// Generate secure token
public string GenerateRefreshToken()
{
    using var rng = RandomNumberGenerator.Create();
    var randomBytes = new byte[32];
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
}

// Token rotation on use
public async Task<AuthResponse> RefreshAccessToken(string refreshToken)
{
    var token = await _context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken);
    
    if (token == null || !token.IsActive)
        throw new SecurityException("Invalid refresh token");
    
    // Revoke old token
    token.RevokedAt = DateTime.UtcNow;
    token.ReplacedByToken = GenerateRefreshToken();
    
    // Create new refresh token
    var newRefreshToken = new RefreshToken
    {
        Token = token.ReplacedByToken,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    };
    
    await _context.RefreshTokens.AddAsync(newRefreshToken);
    await _context.SaveChangesAsync();
    
    return new AuthResponse
    {
        AccessToken = GenerateAccessToken(token.UserId),
        RefreshToken = newRefreshToken.Token
    };
}
```

**DON'T ✗**

```csharp
// Reusing refresh tokens (no rotation)
return existingRefreshToken.Token;

// No expiration
ExpiresAt = DateTime.MaxValue,

// No revocation mechanism
```

---

## Authorization Best Practices

### 1. Role-Based Access Control (RBAC)

**DO ✓**

```csharp
// Define clear role hierarchy
public enum Role
{
    Student = 1,
    Teacher = 2,
    Principal = 3,
    SchoolIncharge = 4,
    Admin = 5,
    SuperAdmin = 6
}

// Use policy-based authorization
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin", "SuperAdmin"));
    
    options.AddPolicy("TeacherOrAbove", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Teacher") ||
            context.User.IsInRole("Principal") ||
            context.User.IsInRole("Admin") ||
            context.User.IsInRole("SuperAdmin")));
    
    options.AddPolicy("SameSchoolOnly", policy =>
        policy.Requirements.Add(new SameSchoolRequirement()));
});

// Controller-level authorization
[Authorize(Roles = "Admin,SuperAdmin")]
public class UserManagementController : ControllerBase { }

// Action-level authorization
[Authorize(Policy = "TeacherOrAbove")]
[HttpGet("grades")]
public async Task<IActionResult> GetGrades() { }
```

**DON'T ✗**

```csharp
// Hardcoded role checks in business logic
if (user.Role == "Admin") // Use policies instead

// Inconsistent authorization checks
[AllowAnonymous] // Be careful with this attribute

// Missing authorization on sensitive endpoints
public async Task<IActionResult> DeleteUser(Guid id) // Missing [Authorize]
```

### 2. Resource-Based Authorization

**DO ✓**

```csharp
// Authorization handler
public class StudentOwnershipHandler : 
    AuthorizationHandler<SameUserRequirement, Student>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserRequirement requirement,
        Student student)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == student.UserId.ToString() || 
            context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

// Controller usage
[HttpGet("{id}")]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _studentRepository.GetByIdAsync(id);
    if (student == null) return NotFound();
    
    // Check authorization
    var authResult = await _authorizationService
        .AuthorizeAsync(User, student, "SameUserPolicy");
    
    if (!authResult.Succeeded)
        return Forbid();
    
    return Ok(student);
}
```

**DON'T ✗**

```csharp
// No ownership check
[HttpGet("{id}")]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _studentRepository.GetByIdAsync(id);
    return Ok(student); // Any authenticated user can access
}
```

### 3. Multi-Tenant Isolation

**DO ✓**

```csharp
// Query filter for school isolation
modelBuilder.Entity<Student>().HasQueryFilter(s => 
    s.SchoolId == _currentSchoolService.GetSchoolId());

// Middleware to set school context
public class SchoolContextMiddleware
{
    public async Task InvokeAsync(HttpContext context, ICurrentSchoolService schoolService)
    {
        var schoolId = context.User.FindFirst("SchoolId")?.Value;
        
        if (!string.IsNullOrEmpty(schoolId))
        {
            schoolService.SetSchoolId(Guid.Parse(schoolId));
        }
        
        await _next(context);
    }
}

// Verify school ownership
public async Task<Student> GetStudentAsync(Guid id)
{
    var student = await _context.Students.FindAsync(id);
    
    if (student == null) return null;
    
    // Verify belongs to current school
    if (student.SchoolId != _currentSchoolService.GetSchoolId())
        throw new UnauthorizedAccessException("Cross-school access denied");
    
    return student;
}
```

**DON'T ✗**

```csharp
// No school filtering
var students = await _context.Students.ToListAsync(); // Returns ALL schools

// Trust client-provided school ID
var schoolId = request.SchoolId; // Client can manipulate this
```

---

## Input Validation

### 1. Request Validation

**DO ✓**

```csharp
// Use FluentValidation
public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name too long")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Invalid characters");
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today).WithMessage("Birth date must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Invalid birth date");
        
        RuleFor(x => x.GradeLevel)
            .InclusiveBetween(1, 12).WithMessage("Grade must be between 1-12");
    }
}

// Validate in controller
[HttpPost]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
{
    var validator = new CreateStudentRequestValidator();
    var validationResult = await validator.ValidateAsync(request);
    
    if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors);
    
    // Proceed with creation
}

// Or use automatic validation
services.AddFluentValidationAutoValidation();
```

**DON'T ✗**

```csharp
// No validation
[HttpPost]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
{
    // Directly use request without validation
    await _studentRepository.CreateAsync(request);
}

// Weak validation
if (string.IsNullOrEmpty(request.FirstName)) // Not enough
```

### 2. SQL Injection Prevention

**DO ✓**

```csharp
// Use parameterized queries (Entity Framework does this automatically)
var students = await _context.Students
    .Where(s => s.FirstName.Contains(searchTerm))
    .ToListAsync();

// Raw SQL with parameters
var students = await _context.Students
    .FromSqlRaw(
        "SELECT * FROM Students WHERE FirstName = {0}",
        searchTerm
    )
    .ToListAsync();

// Dapper with parameters
var students = await connection.QueryAsync<Student>(
    "SELECT * FROM Students WHERE FirstName = @FirstName",
    new { FirstName = searchTerm }
);
```

**DON'T ✗**

```csharp
// String concatenation (SQL INJECTION VULNERABILITY!)
var sql = $"SELECT * FROM Students WHERE FirstName = '{searchTerm}'";
var students = await _context.Students.FromSqlRaw(sql).ToListAsync();

// Dynamic SQL without sanitization
var query = "SELECT * FROM Students WHERE " + userProvidedCondition;
```

### 3. XSS Prevention

**DO ✓**

```csharp
// Encode output (ASP.NET Core does this automatically in Razor views)
@Html.DisplayFor(model => model.UserInput)

// Sanitize HTML input with HtmlSanitizer
public string SanitizeHtml(string input)
{
    var sanitizer = new HtmlSanitizer();
    
    // Allow only safe tags
    sanitizer.AllowedTags.Clear();
    sanitizer.AllowedTags.Add("p");
    sanitizer.AllowedTags.Add("br");
    sanitizer.AllowedTags.Add("strong");
    sanitizer.AllowedTags.Add("em");
    
    return sanitizer.Sanitize(input);
}

// Validate chat messages
public class ChatMessageValidator : AbstractValidator<ChatMessageRequest>
{
    public ChatMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(NotContainScriptTags)
            .WithMessage("Script tags not allowed");
    }
    
    private bool NotContainScriptTags(string content)
    {
        return !Regex.IsMatch(content, @"<script[^>]*>.*?</script>", 
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}
```

**DON'T ✗**

```csharp
// Storing unsanitized user input
chatMessage.Content = request.Content; // Could contain <script>

// Rendering raw HTML
@Html.Raw(Model.UserInput) // XSS VULNERABILITY!

// Disabling encoding
@Model.UnsafeContent // Without proper sanitization
```

### 4. File Upload Validation

**DO ✓**

```csharp
public async Task<IActionResult> UploadFile(IFormFile file)
{
    // Check file size (10 MB max)
    if (file.Length > 10 * 1024 * 1024)
        return BadRequest("File too large");
    
    // Validate file extension
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    
    if (!allowedExtensions.Contains(extension))
        return BadRequest("Invalid file type");
    
    // Validate MIME type
    var allowedMimeTypes = new[] 
    { 
        "image/jpeg", 
        "image/png", 
        "application/pdf" 
    };
    
    if (!allowedMimeTypes.Contains(file.ContentType))
        return BadRequest("Invalid MIME type");
    
    // Validate file signature (magic numbers)
    using var stream = file.OpenReadStream();
    var header = new byte[8];
    await stream.ReadAsync(header, 0, header.Length);
    
    if (!IsValidFileSignature(header, extension))
        return BadRequest("File signature mismatch");
    
    // Generate unique filename
    var fileName = $"{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine(_uploadPath, fileName);
    
    // Save file
    using var fileStream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(fileStream);
    
    return Ok(new { FileName = fileName });
}

private bool IsValidFileSignature(byte[] header, string extension)
{
    return extension switch
    {
        ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8,
        ".png" => header[0] == 0x89 && header[1] == 0x50 
                  && header[2] == 0x4E && header[3] == 0x47,
        ".pdf" => header[0] == 0x25 && header[1] == 0x50 
                  && header[2] == 0x44 && header[3] == 0x46,
        _ => false
    };
}
```

**DON'T ✗**

```csharp
// Trust file extension
var extension = Path.GetExtension(file.FileName); // Can be spoofed

// No size limit
// No MIME type validation
// Use original filename
var filePath = Path.Combine(_uploadPath, file.FileName); // Path traversal risk!

// No virus scanning (consider integrating ClamAV)
```

---

## Output Encoding

### 1. JSON Response Encoding

**DO ✓**

```csharp
// ASP.NET Core automatically encodes JSON responses
return Ok(new { Message = userInput }); // Safe

// Configure JSON serializer
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = 
            JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Encoder = 
            JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // Use carefully
    });
```

**DON'T ✗**

```csharp
// Manual JSON construction (prone to injection)
var json = $"{{\"message\": \"{userInput}\"}}"; // Unsafe!
return Content(json, "application/json");
```

### 2. HTML Encoding

**DO ✓**

```csharp
// In Razor views
@Html.Encode(Model.UserInput)
@Model.SafeProperty // Auto-encoded by default

// In C# code
var encoded = HtmlEncoder.Default.Encode(userInput);
```

**DON'T ✗**

```csharp
// Raw output
@Html.Raw(Model.UserInput) // Only if already sanitized
```

---

## Session Management

### 1. Cookie Security

**DO ✓**

```csharp
// Configure cookie options
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Prevent XSS access
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
    options.Cookie.Name = "SMS.AuthToken"; // Custom name
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;
});

// Set refresh token cookie
Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddDays(7)
});
```

**DON'T ✗**

```csharp
// Insecure cookies
options.Cookie.HttpOnly = false; // Accessible via JavaScript (XSS risk)
options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Works over HTTP
options.Cookie.SameSite = SameSiteMode.None; // No CSRF protection
```

### 2. Session Invalidation

**DO ✓**

```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Invalidate refresh token
    var refreshToken = Request.Cookies["refreshToken"];
    if (!string.IsNullOrEmpty(refreshToken))
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);
        
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _context.SaveChangesAsync();
        }
    }
    
    // Clear cookie
    Response.Cookies.Delete("refreshToken");
    
    // Add token to blacklist (optional, for JWT)
    await _tokenBlacklist.AddAsync(
        User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
        TimeSpan.FromHours(3) // Token expiration time
    );
    
    return Ok(new { Message = "Logged out successfully" });
}

// Check blacklist on each request
public class JwtBlacklistMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITokenBlacklist blacklist)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            if (jti != null && await blacklist.IsBlacklistedAsync(jti))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token revoked");
                return;
            }
        }
        
        await _next(context);
    }
}
```

**DON'T ✗**

```csharp
// No token revocation
[HttpPost("logout")]
public IActionResult Logout()
{
    // Just clear cookie (JWT still valid until expiration!)
    Response.Cookies.Delete("refreshToken");
    return Ok();
}
```

---

## Cryptography

### 1. Encryption at Rest

**DO ✓**

```csharp
// Use SQL Server Transparent Data Encryption (TDE)
ALTER DATABASE SMSDatabase
SET ENCRYPTION ON;

// Column-level encryption for sensitive fields
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public EncryptionService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(configuration["Encryption:Key"]);
        _iv = Convert.FromBase64String(configuration["Encryption:IV"]);
    }
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(cipherBytes);
    }
    
    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
}

// Store encryption keys in Azure Key Vault (never in code!)
```

**DON'T ✗**

```csharp
// Hardcoded encryption keys
private const string Key = "MySecretKey123"; // NEVER DO THIS

// Weak encryption
var encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText)); // Not encryption!

// ECB mode (predictable)
aes.Mode = CipherMode.ECB; // Use CBC or GCM instead
```

### 2. Encryption in Transit

**DO ✓**

```csharp
// Enforce HTTPS
app.UseHttpsRedirection();
app.UseHsts(); // HTTP Strict Transport Security

// Configure Kestrel for TLS 1.2+
.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});

// SQL connection string with encryption
"Server=...;Database=...;Encrypt=True;TrustServerCertificate=False;"
```

**DON'T ✗**

```csharp
// Allow HTTP in production
// options.RequireHttpsMetadata = false;

// Disable certificate validation
TrustServerCertificate=True; // Only for development
```

---

## Error Handling

### 1. Custom Error Responses

**DO ✓**

```csharp
// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception");
            
            // Return generic error message
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "An error occurred processing your request.",
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier
            });
        }
    });
});

// Environment-specific detail level
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detailed errors
}
else
{
    app.UseExceptionHandler("/error"); // Generic errors
}
```

**DON'T ✗**

```csharp
// Expose stack traces in production
app.UseDeveloperExceptionPage(); // In production

// Return internal error details
catch (Exception ex)
{
    return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
}
```

### 2. Validation Error Responses

**DO ✓**

```csharp
// Consistent error response format
public class ErrorResponse
{
    public string Error { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; }
    public string TraceId { get; set; }
}

// Model state errors
if (!ModelState.IsValid)
{
    var errors = ModelState
        .Where(x => x.Value.Errors.Any())
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );
    
    return BadRequest(new ErrorResponse
    {
        Error = "Validation failed",
        ValidationErrors = errors,
        TraceId = HttpContext.TraceIdentifier
    });
}
```

---

## Logging and Monitoring

### 1. Security Event Logging

**DO ✓**

```csharp
// Log security events
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } // Login, Logout, DataAccess, etc.
    public string Resource { get; set; } // Student, Teacher, ChatRoom
    public string? ResourceId { get; set; }
    public bool Success { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

// Audit logging service
public async Task LogAccessAsync(string action, string resource, bool success)
{
    var log = new AuditLog
    {
        Id = Guid.NewGuid(),
        UserId = _currentUser.GetUserId(),
        Action = action,
        Resource = resource,
        Success = success,
        IpAddress = _httpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = _httpContext.Request.Headers["User-Agent"],
        Timestamp = DateTime.UtcNow
    };
    
    await _context.AuditLogs.AddAsync(log);
    await _context.SaveChangesAsync();
}

// Log sensitive operations
[HttpGet("{id}")]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _studentRepository.GetByIdAsync(id);
    
    await _auditService.LogAccessAsync(
        "StudentDataAccess",
        $"Student:{id}",
        student != null
    );
    
    return Ok(student);
}
```

**DON'T ✗**

```csharp
// Log sensitive data
_logger.LogInformation($"User password: {password}"); // NEVER!
_logger.LogDebug($"Credit card: {creditCard}"); // NEVER!

// No security event logging
// No failed login attempt tracking
```

### 2. Monitoring and Alerting

**DO ✓**

```csharp
// Track metrics
public class SecurityMetrics
{
    private static readonly Counter FailedLogins = Metrics
        .CreateCounter("failed_logins_total", "Total failed login attempts");
    
    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("http_request_duration_seconds", "HTTP request duration");
    
    public void RecordFailedLogin()
    {
        FailedLogins.Inc();
    }
    
    public void RecordRequest(double durationSeconds)
    {
        RequestDuration.Observe(durationSeconds);
    }
}

// Alert on suspicious activity
if (failedAttempts >= 5)
{
    await _alertService.SendAlertAsync(
        "Multiple failed login attempts",
        $"User {userName} has {failedAttempts} failed login attempts"
    );
}
```

---

## API Design Security

### 1. Rate Limiting

**DO ✓**

```csharp
// Use AspNetCoreRateLimit
services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
services.AddInMemoryRateLimiting();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configuration
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/Auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  }
}

// Custom rate limit attribute
[RateLimit(Requests = 10, Period = 60)] // 10 requests per minute
public class ChatController : ControllerBase { }
```

**DON'T ✗**

```csharp
// No rate limiting (allows brute force attacks)
```

### 2. CORS Configuration

**DO ✓**

```csharp
// Environment-specific CORS
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Development", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins("https://sms.edu", "https://www.sms.edu")
                  .WithHeaders("Content-Type", "Authorization")
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .AllowCredentials();
        });
    });
}

// Apply CORS
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");
```

**DON'T ✗**

```csharp
// Allow all origins (security risk!)
policy.AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();

// Wildcard with credentials (not allowed)
policy.WithOrigins("*").AllowCredentials(); // Invalid configuration
```

---

## Third-Party Dependencies

### 1. Dependency Management

**DO ✓**

```bash
# Audit dependencies for vulnerabilities
dotnet list package --vulnerable

# Update packages regularly
dotnet outdated

# Use Dependabot (GitHub) for automated updates
```

**DON'T ✗**

```csharp
// Use outdated packages with known vulnerabilities
```

### 2. Secure Package Sources

**DO ✓**

```xml
<!-- nuget.config -->
<packageSources>
  <clear />
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
</packageSources>
```

---

## Deployment Security

### 1. Environment Configuration

**DO ✓**

```csharp
// Use environment variables for secrets
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

// Use Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// appsettings.json (no secrets!)
{
  "JWT": {
    "Issuer": "SMS-API",
    "Audience": "SMS-Client"
    // Secret stored in environment variable or Key Vault
  }
}

// User Secrets (development only)
dotnet user-secrets set "JWT:Secret" "your-secret-key"
```

**DON'T ✗**

```json
// appsettings.json (NEVER store secrets here!)
{
  "JWT": {
    "Secret": "MySecretKey12345" // NEVER!
  },
  "ConnectionStrings": {
    "Default": "Server=...;Password=Pa$$w0rd;" // NEVER!
  }
}
```

### 2. Security Headers

**DO ✓**

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Permissions-Policy", 
        "geolocation=(), microphone=(), camera=()");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    
    await next();
});
```

---

## Code Review Checklist

### Security Review Checklist

**Authentication & Authorization:**
- [ ] All endpoints have `[Authorize]` attribute (except public endpoints)
- [ ] JWT tokens have expiration
- [ ] Refresh tokens implement rotation
- [ ] Passwords hashed with BCrypt (work factor >= 12)
- [ ] MFA implemented for admin roles
- [ ] Resource-based authorization implemented
- [ ] Multi-tenant isolation enforced

**Input Validation:**
- [ ] All user input validated with FluentValidation
- [ ] SQL injection prevented (parameterized queries)
- [ ] XSS prevented (input sanitization + output encoding)
- [ ] File uploads validated (size, type, magic numbers)
- [ ] Path traversal prevented

**Data Protection:**
- [ ] Sensitive data encrypted at rest
- [ ] HTTPS enforced (TLS 1.2+)
- [ ] Secrets stored in environment variables or Key Vault
- [ ] No secrets in source code or logs

**Session Management:**
- [ ] Cookies have HttpOnly, Secure, SameSite attributes
- [ ] Sessions invalidated on logout
- [ ] Token blacklist implemented

**Error Handling:**
- [ ] Generic error messages (no stack traces in production)
- [ ] Errors logged securely (no sensitive data)
- [ ] 500 errors handled gracefully

**Logging & Monitoring:**
- [ ] Security events logged (login, logout, data access)
- [ ] Failed login attempts tracked
- [ ] Audit logs tamper-proof

**API Security:**
- [ ] Rate limiting implemented
- [ ] CORS configured correctly
- [ ] Security headers present
- [ ] No unnecessary HTTP methods enabled

**Dependencies:**
- [ ] All packages up-to-date
- [ ] No vulnerable dependencies
- [ ] Packages from trusted sources only

---

**Related Documentation:**
- [Security Implementation Guide](SECURITY_IMPLEMENTATION_GUIDE.md)
- [Security Testing Checklist](SECURITY_TESTING_CHECKLIST.md)
- [Penetration Testing Guide](PENETRATION_TESTING.md)
