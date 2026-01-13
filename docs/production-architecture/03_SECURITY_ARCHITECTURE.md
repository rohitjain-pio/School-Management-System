# Security Architecture
## 7-Layer Defense-in-Depth Strategy

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 20 minutes  
**Classification:** ⭐ CRITICAL  
**Status:** 🟡 Partially Implemented

---

## 🎯 **Security Philosophy**

### **Defense-in-Depth**

**Principle:** Multiple layers of security. If one layer fails, others still protect the system.

**Analogy:** Medieval castle defense
- Moat (Network firewall)
- Outer walls (API gateway)
- Guard towers (Monitoring)
- Inner walls (Authentication)
- Vaults (Encryption)
- Sentries (Audit logging)

**One layer is never enough.** We implement 7 layers.

---

## 🏰 **Layer 1: Network Security**

### **1.1 HTTPS Enforcement**

**What:** All traffic encrypted with TLS 1.3

**Why:** Prevents man-in-the-middle attacks, eavesdropping

**Implementation:**
```csharp
// Program.cs
app.UseHttpsRedirection(); // Redirects HTTP → HTTPS

app.UseHsts(); // HTTP Strict Transport Security
// Tells browsers: "Only use HTTPS for this domain"
```

**Configuration:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:7266",
        "Protocols": "Http1AndHttp2And3", // HTTP/3 support
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "secure-password"
        }
      }
    }
  }
}
```

### **1.2 CORS Policy**

**What:** Control which domains can access API

**Implementation:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SchoolManagementPolicy", policy =>
    {
        policy.WithOrigins(
            "https://yourdomain.com",
            "https://www.yourdomain.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // For cookies
    });
});

app.UseCors("SchoolManagementPolicy");
```

**Production:** Whitelist only your domains, never use `AllowAnyOrigin()`

### **1.3 Rate Limiting**

**What:** Prevent brute-force attacks

**Implementation:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginLimiter", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // Max 5 login attempts per minute
        opt.QueueLimit = 0;
    });
    
    options.AddFixedWindowLimiter("ApiLimiter", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100; // Max 100 API calls per minute
    });
});

// Apply to endpoints
[EnableRateLimiting("LoginLimiter")]
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto) { }
```

---

## 🔐 **Layer 2: Authentication**

### **2.1 JWT Token Structure**

**Access Token (3-hour expiry):**
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "nameid": "user-guid",
    "email": "user@school.com",
    "role": "Teacher",
    "schoolId": "school-guid",
    "exp": 1705334400,
    "iat": 1705323600,
    "iss": "SchoolManagementSystem",
    "aud": "SchoolManagementClient"
  },
  "signature": "HMACSHA256(base64(header) + '.' + base64(payload), secret)"
}
```

**Refresh Token (30-day expiry):**
- Stored in database: `RefreshTokens` table
- HttpOnly cookie (not accessible via JavaScript)
- Used to get new access token without re-login
- Rotated on each use (old token invalidated)

### **2.2 Token Generation**

```csharp
public string GenerateAccessToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("SchoolId", user.SchoolId.ToString())
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])
    );
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(3),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### **2.3 Password Security**

**BCrypt Hashing:**
```csharp
// On registration
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// On login
bool isValid = BCrypt.Net.BCrypt.Verify(passwordAttempt, user.PasswordHash);
```

**Password Requirements:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character
- Not a common password (checked against list)

**Account Lockout:**
```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
});
```

---

## 🛡️ **Layer 3: Authorization**

### **3.1 Role-Based Authorization**

**Simple Role Check:**
```csharp
[Authorize(Roles = "Admin,Teacher")]
[HttpPost]
public async Task<IActionResult> CreateStudent(StudentDto dto) { }
```

**Policy-Based Authorization:**
```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
    
    options.AddPolicy("RequireTeacher", policy =>
        policy.RequireRole("Teacher", "Admin", "SuperAdmin"));
    
    options.AddPolicy("CanManageStudents", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.IsInRole("Teacher")
        ));
});

// Controller
[Authorize(Policy = "RequireAdmin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteSchool(Guid id) { }
```

### **3.2 Resource-Based Authorization**

**Scenario:** Teacher can only edit their own classes

```csharp
public class ClassAuthorizationHandler : 
    AuthorizationHandler<OperationAuthorizationRequirement, Class>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Class resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Allow if teacher is assigned to this class
        if (resource.TeacherId.ToString() == userId)
        {
            context.Succeed(requirement);
        }
        // Always allow Admin
        else if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

// Usage in controller
var authResult = await _authorizationService
    .AuthorizeAsync(User, classObj, Operations.Update);
    
if (!authResult.Succeeded)
    return Forbid();
```

---

## 🔒 **Layer 4: Multi-Tenant Isolation**

**See:** [02_MULTI_TENANCY_DESIGN.md](./02_MULTI_TENANCY_DESIGN.md) for complete details

**Key Points:**
- SchoolId in JWT claims
- Middleware validates on every request
- BaseSchoolController enforces ownership
- Database queries always filter by SchoolId
- SuperAdmin bypass with audit logging

---

## 🔐 **Layer 5: Data Protection**

### **5.1 Database Encryption at Rest**

**SQL Server Transparent Data Encryption (TDE):**
```sql
-- Create master key
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'StrongPassword123!';

-- Create certificate
CREATE CERTIFICATE TDECert WITH SUBJECT = 'TDE Certificate';

-- Create database encryption key
USE SchoolManagementDB;
CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE TDECert;

-- Enable encryption
ALTER DATABASE SchoolManagementDB
SET ENCRYPTION ON;
```

**Result:** All data files encrypted on disk. If attacker steals hard drive, data is unreadable.

### **5.2 Sensitive Field Encryption**

**Encrypt PII (Personally Identifiable Information):**

```csharp
public class Student
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [EncryptedColumn] // Custom attribute
    public string AadhaarNumber { get; set; } // Encrypted in DB
    
    [EncryptedColumn]
    public string FatherPhone { get; set; }
    
    [EncryptedColumn]
    public string MedicalHistory { get; set; }
}
```

**Implementation:**
```csharp
public class EncryptionService
{
    private readonly byte[] _key;
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        
        return Convert.ToBase64String(result);
    }
    
    public string Decrypt(string cipherText) { /* ... */ }
}
```

### **5.3 Chat Message Encryption**

**End-to-End Encryption (Future Enhancement):**
```csharp
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    
    [EncryptedColumn]
    public string Message { get; set; } // Encrypted before storage
    
    public DateTime SentAt { get; set; }
}
```

---

## 🛡️ **Layer 6: Input Validation**

### **6.1 FluentValidation**

```csharp
public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z\s]+$") // Only letters and spaces
            .WithMessage("First name must contain only letters");
        
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
        
        RuleFor(x => x.Grade)
            .InclusiveBetween(1, 12)
            .WithMessage("Grade must be between 1 and 12");
        
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Now.AddYears(-5))
            .WithMessage("Student must be at least 5 years old");
    }
}
```

### **6.2 SQL Injection Prevention**

**NEVER do this:**
```csharp
// ❌ VULNERABLE
string sql = $"SELECT * FROM Students WHERE Name = '{name}'";
var students = _context.Students.FromSqlRaw(sql).ToList();
```

**ALWAYS do this:**
```csharp
// ✅ SAFE (parameterized)
var students = await _context.Students
    .Where(s => s.Name == name)
    .ToListAsync();

// Or with raw SQL (parameterized)
var students = await _context.Students
    .FromSqlRaw("SELECT * FROM Students WHERE Name = {0}", name)
    .ToListAsync();
```

### **6.3 XSS Prevention**

**Backend:** Sanitize HTML input
```csharp
public string SanitizeHtml(string input)
{
    return HttpUtility.HtmlEncode(input);
}
```

**Frontend:** React automatically escapes JSX
```tsx
// ✅ SAFE - React escapes by default
<div>{student.name}</div>

// ❌ DANGEROUS - dangerouslySetInnerHTML
<div dangerouslySetInnerHTML={{ __html: student.bio }} />

// ✅ SAFE - Use DOMPurify library
import DOMPurify from 'dompurify';
<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(student.bio) }} />
```

---

## 📊 **Layer 7: Audit & Monitoring**

### **7.1 Audit Logging**

**What to Log:**
- All authentication attempts (success + failure)
- All data modifications (create, update, delete)
- All SuperAdmin access
- All failed authorization attempts
- All security-related errors

**Implementation:**
```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SchoolId { get; set; }
    public string Action { get; set; } // "CreateStudent", "DeleteClass"
    public string EntityType { get; set; } // "Student", "Class"
    public Guid? EntityId { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string Details { get; set; } // JSON with old/new values
}
```

**Middleware:**
```csharp
public class AuditMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        
        await _next(context);
        
        // Log if modification occurred
        if (context.Request.Method != "GET" && 
            context.Response.StatusCode < 400)
        {
            await LogAudit(context);
        }
        
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBody);
    }
}
```

### **7.2 Serilog Configuration**

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.MSSqlServer(
        connectionString: Configuration.GetConnectionString("Default"),
        tableName: "Logs",
        autoCreateSqlTable: true)
    .CreateLogger();
```

### **7.3 Real-Time Alerts**

**Alert on Critical Events:**
```csharp
public class SecurityAlertService
{
    public async Task AlertFailedLogins(string email, int attemptCount)
    {
        if (attemptCount >= 5)
        {
            await _emailService.SendAsync(
                to: "admin@yourdomain.com",
                subject: "SECURITY ALERT: Multiple Failed Logins",
                body: $"User {email} has {attemptCount} failed login attempts"
            );
        }
    }
    
    public async Task AlertCrossSchoolAccess(Guid userId, Guid attemptedSchoolId)
    {
        _logger.LogCritical(
            "SECURITY: User {UserId} attempted cross-school access to {SchoolId}",
            userId, attemptedSchoolId
        );
        
        // Send immediate notification
        await _notificationService.NotifySuperAdminAsync(
            "Cross-school access attempt detected"
        );
    }
}
```

---

## 🚨 **Security Incident Response**

### **Incident Categories**

**P0 - Critical (Immediate Response):**
- Data breach detected
- SQL injection successful
- Cross-school access successful
- Database compromise

**P1 - High (1-hour Response):**
- Repeated failed authentication
- DDoS attack in progress
- Suspicious SuperAdmin activity

**P2 - Medium (24-hour Response):**
- Invalid JWT usage spike
- Excessive API calls from IP

### **Response Procedure**

**Immediate Actions:**
1. Alert sent to SuperAdmin phone/email
2. Log incident details
3. Block offending IP/user
4. Take affected system offline if needed
5. Preserve evidence (logs, database snapshots)

**Investigation:**
6. Review audit logs
7. Identify scope of breach
8. Determine attack vector
9. Document findings

**Remediation:**
10. Patch vulnerability
11. Reset compromised credentials
12. Notify affected schools (if required)
13. Update security documentation

---

## ✅ **Security Checklist**

**Before Production:**

- [ ] HTTPS enforced with valid certificate
- [ ] JWT secret key 256-bit minimum
- [ ] Rate limiting on all endpoints
- [ ] CORS whitelist configured
- [ ] Password policy enforced
- [ ] Account lockout enabled
- [ ] Database TDE enabled
- [ ] Sensitive fields encrypted
- [ ] Input validation on all DTOs
- [ ] SQL injection tests passed
- [ ] XSS prevention verified
- [ ] Audit logging implemented
- [ ] Serilog configured
- [ ] Security alerts configured
- [ ] Incident response plan documented
- [ ] Penetration testing completed
- [ ] Security review by external auditor

---

## 📚 **Next Steps**

1. **Implement:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md) ⭐
2. **Test:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)
3. **Monitor:** [17_MONITORING_LOGGING.md](./17_MONITORING_LOGGING.md)

---

**Document Status:** ✅ Complete  
**Implementation Status:** 🟡 Partially Complete  
**Critical for:** Production Launch
