# Security Patterns & Best Practices

## Authentication & Authorization

### JWT Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "nameid": "123e4567-e89b-12d3-a456-426614174000",
    "email": "admin@school.com",
    "role": ["SchoolAdmin", "Teacher"],
    "SchoolId": "987f6543-e21b-12d3-a456-426614174000",
    "jti": "unique-token-id",
    "exp": 1705344000,
    "iss": "https://api.schoolsystem.com",
    "aud": "https://schoolsystem.com"
  }
}
```

### Token Generation

```csharp
private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
{
    var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

    // Add SchoolId claim (mandatory for non-SuperAdmin)
    if (user.SchoolId != Guid.Empty)
    {
        authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
    }

    // Add role claims
    foreach (var role in roles)
    {
        authClaims.Add(new Claim(ClaimTypes.Role, role));
    }

    var authSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"])
    );

    var token = new JwtSecurityToken(
        issuer: _configuration["JwtSettings:Issuer"],
        audience: _configuration["JwtSettings:Audience"],
        expires: DateTime.UtcNow.AddMinutes(60),
        claims: authClaims,
        signingCredentials: new SigningCredentials(
            authSigningKey, 
            SecurityAlgorithms.HmacSha256
        )
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Token Validation Middleware

```csharp
// In Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])
        ),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };
});
```

---

## Role-Based Authorization

### Roles Hierarchy

```
SuperAdmin (Full access to ALL schools)
├── SchoolAdmin (Full access to their school)
│   ├── Teacher (Student data, grades, attendance)
│   ├── Accountant (Fee management, payments)
│   └── Receptionist (Student enrollment, general info)
├── Student (View own data only)
└── Parent (View children's data only)
```

### Authorization Policies

```csharp
// In Program.cs - Configure policies
builder.Services.AddAuthorization(options =>
{
    // SuperAdmin can do anything
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    // School staff (Admin, Teacher, etc.)
    options.AddPolicy("RequireSchoolStaff", policy =>
        policy.RequireRole("SuperAdmin", "SchoolAdmin", "Teacher", "Accountant"));

    // School management only
    options.AddPolicy("RequireSchoolManagement", policy =>
        policy.RequireRole("SuperAdmin", "SchoolAdmin"));

    // Students and parents
    options.AddPolicy("RequireStudent", policy =>
        policy.RequireRole("SuperAdmin", "SchoolAdmin", "Teacher", "Student", "Parent"));
});
```

### Using Authorization Attributes

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class StudentController : BaseSchoolController
{
    // Only school staff can create students
    [HttpPost]
    [Authorize(Policy = "RequireSchoolStaff")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
    {
        // Implementation
    }

    // Students can view their own data
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyData()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Implementation
    }

    // SuperAdmin can access any school's data
    [HttpGet("school/{schoolId}")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<IActionResult> GetSchoolStudents(Guid schoolId)
    {
        // Implementation
    }
}
```

---

## Input Validation

### DTO Validation with FluentValidation

```csharp
public class CreateStudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-100)).WithMessage("Invalid date of birth");
    }
}
```

### Model State Validation

```csharp
[HttpPost]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
{
    // FluentValidation runs automatically if registered
    if (!ModelState.IsValid)
    {
        return BadRequest(new
        {
            error = "Validation failed",
            errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        });
    }

    // Proceed with creation
}
```

---

## SQL Injection Prevention

### ✅ CORRECT: Parameterized Queries (EF Core)

```csharp
// EF Core automatically parameterizes queries
public async Task<Student?> GetByEmailAsync(string email, Guid schoolId)
{
    return await _context.Students
        .FirstOrDefaultAsync(s => 
            s.Email == email && 
            s.SchoolId == schoolId); // Automatically parameterized
}
```

### ❌ WRONG: String Concatenation

```csharp
// NEVER DO THIS - SQL Injection vulnerability
public async Task<Student?> GetByEmailAsync(string email, Guid schoolId)
{
    var query = $"SELECT * FROM Students WHERE Email = '{email}' AND SchoolId = '{schoolId}'";
    return await _context.Students.FromSqlRaw(query).FirstOrDefaultAsync();
}
```

### ✅ CORRECT: Raw SQL with Parameters

```csharp
public async Task<Student?> GetByEmailAsync(string email, Guid schoolId)
{
    return await _context.Students
        .FromSqlRaw(
            "SELECT * FROM Students WHERE Email = {0} AND SchoolId = {1}",
            email,
            schoolId
        )
        .FirstOrDefaultAsync();
}
```

---

## XSS (Cross-Site Scripting) Prevention

### Output Encoding (Automatic in ASP.NET Core)

```csharp
// ASP.NET Core automatically encodes JSON responses
[HttpGet("{id}")]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _studentService.GetByIdAsync(id, GetSchoolIdFromClaims());
    
    // Even if student.FirstName contains <script>alert('xss')</script>
    // The JSON serializer will encode it automatically
    return Ok(student);
}
```

### Content Security Policy Headers

```csharp
// In Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'"
    );
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    await next();
});
```

---

## CSRF (Cross-Site Request Forgery) Prevention

### SameSite Cookies

```csharp
// In Program.cs
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
});
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173", // Development
            "https://schoolsystem.com" // Production
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Apply CORS
app.UseCors("AllowFrontend");
```

---

## Password Security

### Password Requirements

```csharp
// In Program.cs
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
});
```

### Password Hashing

```csharp
// ASP.NET Identity automatically handles password hashing
public async Task<IdentityResult> CreateUserAsync(RegisterDto dto)
{
    var user = new ApplicationUser
    {
        UserName = dto.Email,
        Email = dto.Email,
        SchoolId = dto.SchoolId
    };

    // Password is automatically hashed using PBKDF2
    var result = await _userManager.CreateAsync(user, dto.Password);
    
    return result;
}
```

---

## Sensitive Data Protection

### Exclude Sensitive Fields from Logs

```csharp
_logger.LogInformation(
    "User {UserId} updated profile", 
    userId
    // DO NOT log: password, SSN, credit card, etc.
);
```

### Never Return Sensitive Data in API

```csharp
public class StudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // NEVER include these in API response:
    // - SchoolId (internal use only)
    // - PasswordHash
    // - SecurityStamp
    // - Social Security Number
    // - Parent's financial data
}
```

### Encrypt Sensitive Database Columns

```sql
-- Use SQL Server Always Encrypted for sensitive data
CREATE TABLE Students (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Aadhaar NVARCHAR(12) ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK_Auto,
        ENCRYPTION_TYPE = DETERMINISTIC,
        ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
    ) NULL
);
```

---

## API Rate Limiting

### Rate Limiting Middleware

```csharp
// Install: AspNetCoreRateLimit NuGet package
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m" // 100 requests per minute
        },
        new RateLimitRule
        {
            Endpoint = "*/api/auth/*",
            Limit = 10,
            Period = "1m" // Strict limit on auth endpoints
        }
    };
});

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Apply middleware
app.UseIpRateLimiting();
```

---

## Audit Logging

### Audit Log Entity

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? SchoolId { get; set; } // Null for SuperAdmin actions
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
```

### Audit Logging Service

```csharp
public interface IAuditLogService
{
    Task LogActionAsync(
        Guid userId,
        Guid? schoolId,
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null
    );
}

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task LogActionAsync(
        Guid userId,
        Guid? schoolId,
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown",
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
```

### Usage in Controllers

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteStudent(Guid id)
{
    var schoolId = GetSchoolIdFromClaims();
    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    
    var student = await _studentService.GetByIdAsync(id, schoolId);
    
    if (student == null)
    {
        return NotFound();
    }

    // Log the deletion
    await _auditLogService.LogActionAsync(
        userId,
        schoolId,
        "DELETE",
        "Student",
        id,
        oldValues: student,
        newValues: null
    );

    await _studentService.DeleteAsync(id, schoolId);
    
    return NoContent();
}
```

---

## HTTPS Enforcement

### Redirect HTTP to HTTPS

```csharp
// In Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}
```

### HSTS Configuration

```csharp
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

---

## Secrets Management

### Development (User Secrets)

```bash
# Initialize user secrets
dotnet user-secrets init --project Backend/SMSPrototype1

# Add secrets
dotnet user-secrets set "JwtSettings:Secret" "your-256-bit-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..."
```

### Production (Azure Key Vault)

```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential()
    );
}
```

---

## Security Checklist

Before deploying to production:

- [ ] SchoolId isolation implemented and tested
- [ ] JWT tokens include SchoolId claim
- [ ] All endpoints require authentication (except /auth)
- [ ] Role-based authorization configured
- [ ] Input validation on all DTOs
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS prevention (output encoding)
- [ ] CSRF protection (SameSite cookies, CORS)
- [ ] HTTPS enforced in production
- [ ] Secrets in environment variables / Key Vault
- [ ] Rate limiting enabled
- [ ] Audit logging for sensitive actions
- [ ] Password requirements enforced
- [ ] Failed login attempt lockout
- [ ] Content Security Policy headers
- [ ] Security headers (X-Frame-Options, etc.)
- [ ] Database backups automated
- [ ] Regular security updates scheduled
