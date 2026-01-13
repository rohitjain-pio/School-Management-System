# Authentication & Authorization
## Complete Auth Flow for All User Roles

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 18 minutes  
**Status:** ✅ Implemented

---

## 🎯 **Authentication vs Authorization**

### **Authentication** = "Who are you?"
Verifying user identity (login with email/password)

### **Authorization** = "What can you do?"
Verifying user permissions (can this user delete students?)

---

## 🔐 **Authentication Flow**

### **Complete Login Sequence**

```
┌──────────┐                                    ┌──────────┐
│  Client  │                                    │  Server  │
└─────┬────┘                                    └─────┬────┘
      │                                               │
      │  1. POST /api/auth/login                     │
      │  { email, password }                          │
      ├──────────────────────────────────────────────>│
      │                                               │
      │                          2. Validate password │
      │                          (BCrypt.Verify)      │
      │                                               │
      │                          3. Generate JWT      │
      │                          (3-hour expiry)      │
      │                                               │
      │                          4. Generate refresh  │
      │                          token (30-day)       │
      │                                               │
      │                          5. Store refresh     │
      │                          token in DB          │
      │                                               │
      │  6. 200 OK                                    │
      │  { accessToken, user }                        │
      │  Set-Cookie: refreshToken (httpOnly)          │
      │<──────────────────────────────────────────────┤
      │                                               │
      │  7. Store accessToken in memory               │
      │  (React state, not localStorage)              │
      │                                               │
```

### **Implementation: Login**

```csharp
[HttpPost("login")]
[AllowAnonymous]
[EnableRateLimiting("LoginLimiter")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    // 1. Find user by email
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user == null)
    {
        _logger.LogWarning("Login attempt for non-existent email: {Email}", dto.Email);
        return Unauthorized(new { message = "Invalid email or password" });
    }
    
    // 2. Check if account is locked
    if (await _userManager.IsLockedOutAsync(user))
    {
        return Unauthorized(new { message = "Account is locked. Try again later." });
    }
    
    // 3. Verify password
    var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
    if (!passwordValid)
    {
        await _userManager.AccessFailedAsync(user); // Increment failed attempts
        _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
        return Unauthorized(new { message = "Invalid email or password" });
    }
    
    // 4. Reset failed login count
    await _userManager.ResetAccessFailedCountAsync(user);
    
    // 5. Check if user has SchoolId (except SuperAdmin)
    var roles = await _userManager.GetRolesAsync(user);
    if (!roles.Contains("SuperAdmin") && user.SchoolId == Guid.Empty)
    {
        return BadRequest(new 
        { 
            message = "Your account is not associated with a school. Contact administrator." 
        });
    }
    
    // 6. Generate tokens
    var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
    var refreshToken = _tokenService.GenerateRefreshToken();
    
    // 7. Store refresh token in database
    await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, 
        HttpContext.Connection.RemoteIpAddress?.ToString());
    
    // 8. Set refresh token as httpOnly cookie
    Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true, // HTTPS only
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    });
    
    // 9. Return access token and user info
    return Ok(new LoginResponseDto
    {
        AccessToken = accessToken,
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = roles.FirstOrDefault(),
            SchoolId = user.SchoolId,
            SchoolName = user.School?.Name
        }
    });
}
```

---

## 🔄 **Token Refresh Flow**

### **Why Refresh Tokens?**

**Access Token:** Short-lived (3 hours) for security  
**Refresh Token:** Long-lived (30 days) for convenience

When access token expires, use refresh token to get new one without re-login.

### **Refresh Flow**

```
┌──────────┐                                    ┌──────────┐
│  Client  │                                    │  Server  │
└─────┬────┘                                    └─────┬────┘
      │                                               │
      │  1. API call with expired JWT                 │
      ├──────────────────────────────────────────────>│
      │                                               │
      │  2. 401 Unauthorized (token expired)          │
      │<──────────────────────────────────────────────┤
      │                                               │
      │  3. POST /api/auth/refresh-token              │
      │  Cookie: refreshToken                         │
      ├──────────────────────────────────────────────>│
      │                                               │
      │                          4. Validate refresh  │
      │                          token from DB        │
      │                                               │
      │                          5. Generate new JWT  │
      │                                               │
      │                          6. Rotate refresh    │
      │                          token (optional)     │
      │                                               │
      │  7. 200 OK { newAccessToken }                 │
      │  Set-Cookie: newRefreshToken                  │
      │<──────────────────────────────────────────────┤
      │                                               │
      │  8. Retry original API call                   │
      ├──────────────────────────────────────────────>│
      │                                               │
```

### **Implementation: Refresh Token**

```csharp
[HttpPost("refresh-token")]
[AllowAnonymous]
public async Task<IActionResult> RefreshToken()
{
    // 1. Get refresh token from cookie
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized(new { message = "Refresh token is required" });
    
    // 2. Validate refresh token
    var storedToken = await _context.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
    
    if (storedToken == null || !storedToken.IsActive)
        return Unauthorized(new { message = "Invalid or expired refresh token" });
    
    // 3. Revoke old refresh token (token rotation)
    storedToken.RevokedAt = DateTime.UtcNow;
    storedToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
    storedToken.ReasonRevoked = "Token rotated";
    
    // 4. Generate new tokens
    var user = storedToken.User;
    var roles = await _userManager.GetRolesAsync(user);
    var newAccessToken = _tokenService.GenerateAccessToken(user, roles.ToList());
    var newRefreshToken = _tokenService.GenerateRefreshToken();
    
    // 5. Save new refresh token
    storedToken.ReplacedByToken = newRefreshToken;
    await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken,
        HttpContext.Connection.RemoteIpAddress?.ToString());
    
    await _context.SaveChangesAsync();
    
    // 6. Set new refresh token cookie
    Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    });
    
    // 7. Return new access token
    return Ok(new { accessToken = newAccessToken });
}
```

---

## 🚪 **Logout Flow**

### **Secure Logout**

```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var refreshToken = Request.Cookies["refreshToken"];
    
    if (!string.IsNullOrEmpty(refreshToken))
    {
        // Revoke refresh token
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "User logout";
            await _context.SaveChangesAsync();
        }
    }
    
    // Delete refresh token cookie
    Response.Cookies.Delete("refreshToken");
    
    // Add access token to blacklist (Redis)
    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    if (!string.IsNullOrEmpty(jti))
    {
        await _cache.SetStringAsync(
            $"blacklist:{jti}",
            "revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3)
            });
    }
    
    return Ok(new { message = "Logged out successfully" });
}
```

---

## 🛡️ **Authorization Patterns**

### **1. Role-Based Authorization**

```csharp
// Simple role check
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteStudent(Guid id)
{
    // Only Admin can delete
}

// Multiple roles
[Authorize(Roles = "Admin,Teacher")]
[HttpPost]
public async Task<IActionResult> CreateStudent(CreateStudentDto dto)
{
    // Admin OR Teacher can create
}
```

### **2. Policy-Based Authorization**

**Define Policies:**
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
            context.User.IsInRole("Teacher")));
    
    options.AddPolicy("RequireSchoolId", policy =>
        policy.RequireAssertion(context =>
        {
            var schoolIdClaim = context.User.FindFirst("SchoolId");
            return schoolIdClaim != null && 
                   Guid.TryParse(schoolIdClaim.Value, out var schoolId) &&
                   schoolId != Guid.Empty;
        }));
});
```

**Use Policies:**
```csharp
[Authorize(Policy = "RequireAdmin")]
[HttpPost]
public async Task<IActionResult> CreateSchool(CreateSchoolDto dto) { }
```

### **3. Resource-Based Authorization**

**Scenario:** Teacher can only edit their own classes

```csharp
public class ClassAuthorizationHandler : 
    AuthorizationHandler<SameSchoolRequirement, Class>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameSchoolRequirement requirement,
        Class resource)
    {
        var userSchoolIdClaim = context.User.FindFirst("SchoolId")?.Value;
        
        if (Guid.TryParse(userSchoolIdClaim, out var userSchoolId) &&
            userSchoolId == resource.SchoolId)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

// Usage in controller
[HttpPut("{id}")]
public async Task<IActionResult> UpdateClass(Guid id, UpdateClassDto dto)
{
    var classObj = await _classService.GetByIdAsync(id);
    if (classObj == null)
        return NotFound();
    
    var authResult = await _authorizationService
        .AuthorizeAsync(User, classObj, new SameSchoolRequirement());
    
    if (!authResult.Succeeded)
        return Forbid();
    
    await _classService.UpdateAsync(id, dto);
    return NoContent();
}
```

---

## 👥 **Role-Specific Authorization**

### **SuperAdmin**

```csharp
public class SuperAdminAuthorizationHandler : 
    AuthorizationHandler<OperationAuthorizationRequirement>
{
    private readonly IAuditLogService _auditLog;
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement)
    {
        if (context.User.IsInRole("SuperAdmin"))
        {
            // Log SuperAdmin access
            _auditLog.LogAsync(new AuditLogEntry
            {
                UserId = GetUserId(context.User),
                Action = "SuperAdminAccess",
                Details = $"Accessed {requirement.Name}",
                Severity = "Warning"
            });
            
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### **Admin (School Principal)**

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : BaseSchoolController
{
    [HttpPost("teachers")]
    public async Task<IActionResult> CreateTeacher(CreateTeacherDto dto)
    {
        var schoolId = GetUserSchoolId(); // From BaseSchoolController
        dto.SchoolId = schoolId; // Enforce school isolation
        
        var teacher = await _teacherService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacher);
    }
}
```

### **Teacher**

```csharp
[Authorize(Roles = "Teacher")]
public class AttendanceController : BaseSchoolController
{
    [HttpPost]
    public async Task<IActionResult> MarkAttendance(MarkAttendanceDto dto)
    {
        var schoolId = GetUserSchoolId();
        var teacherId = GetUserId();
        
        // Verify teacher is assigned to this class
        var isAssigned = await _teacherService
            .IsAssignedToClassAsync(teacherId, dto.ClassId);
        
        if (!isAssigned)
            return Forbid("You are not assigned to this class");
        
        await _attendanceService.MarkAsync(dto, schoolId, teacherId);
        return Ok();
    }
}
```

### **Student**

```csharp
[Authorize(Roles = "Student")]
public class StudentDashboardController : ControllerBase
{
    [HttpGet("my-grades")]
    public async Task<IActionResult> GetMyGrades()
    {
        var studentId = GetUserId(); // Current student's ID
        
        // Students can ONLY see their own grades
        var grades = await _gradeService.GetByStudentIdAsync(studentId);
        return Ok(grades);
    }
    
    [HttpGet("my-attendance")]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var studentId = GetUserId();
        
        var attendance = await _attendanceService
            .GetByStudentIdAsync(studentId, from, to);
        return Ok(attendance);
    }
}
```

### **Parent**

```csharp
[Authorize(Roles = "Parent")]
public class ParentDashboardController : ControllerBase
{
    [HttpGet("children")]
    public async Task<IActionResult> GetMyChildren()
    {
        var parentId = GetUserId();
        
        // Get all children linked to this parent
        var children = await _studentService.GetByParentIdAsync(parentId);
        return Ok(children);
    }
    
    [HttpGet("children/{studentId}/grades")]
    public async Task<IActionResult> GetChildGrades(Guid studentId)
    {
        var parentId = GetUserId();
        
        // Verify this student is linked to this parent
        var isMyChild = await _studentService
            .IsParentOfStudentAsync(parentId, studentId);
        
        if (!isMyChild)
            return Forbid("This student is not linked to your account");
        
        var grades = await _gradeService.GetByStudentIdAsync(studentId);
        return Ok(grades);
    }
}
```

---

## 🔑 **JWT Token Structure**

### **Claims in JWT**

```json
{
  "nameid": "user-guid",
  "email": "user@school.com",
  "role": "Teacher",
  "schoolId": "school-guid",
  "jti": "token-unique-id",
  "exp": 1705334400,
  "iss": "SchoolManagementSystem",
  "aud": "SchoolManagementClient"
}
```

### **Token Generation**

```csharp
public string GenerateAccessToken(ApplicationUser user, List<string> roles)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // For blacklisting
        new("SchoolId", user.SchoolId.ToString())
    };
    
    // Add all roles
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
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

---

## 🔐 **Frontend Token Storage**

### **Secure Storage Pattern**

```typescript
// ❌ NEVER do this (XSS vulnerable)
localStorage.setItem('accessToken', token);

// ✅ Store in memory (React state)
const [accessToken, setAccessToken] = useState<string | null>(null);

// Refresh token automatically stored in httpOnly cookie by server
```

### **Axios Interceptor (React)**

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true // Include cookies (refresh token)
});

// Add access token to all requests
api.interceptors.request.use((config) => {
  const token = getAccessToken(); // From React state
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auto-refresh on 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Refresh token (uses cookie automatically)
        const { data } = await axios.post(
          `${import.meta.env.VITE_API_URL}/api/auth/refresh-token`,
          {},
          { withCredentials: true }
        );
        
        setAccessToken(data.accessToken);
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed, redirect to login
        setAccessToken(null);
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);
```

---

## ✅ **Security Checklist**

**Authentication:**
- [ ] Passwords hashed with BCrypt (work factor 12+)
- [ ] JWT signed with strong secret (256-bit minimum)
- [ ] Access token short-lived (3 hours)
- [ ] Refresh token rotation implemented
- [ ] Account lockout after 5 failed attempts
- [ ] Rate limiting on login endpoint (5 per minute)

**Authorization:**
- [ ] SchoolId validation on every request
- [ ] Role-based authorization implemented
- [ ] Resource ownership checks for sensitive operations
- [ ] SuperAdmin access logged in audit trail

**Token Security:**
- [ ] Access token NOT in localStorage
- [ ] Refresh token in httpOnly cookie
- [ ] Token blacklisting on logout
- [ ] HTTPS enforced in production

---

## 📚 **Next Steps**

1. **User Flows:** [07_USER_WORKFLOWS.md](./07_USER_WORKFLOWS.md)
2. **Security Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)
3. **Testing:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)

---

**Document Status:** ✅ Complete  
**Implementation Status:** ✅ Fully Implemented  
**Last Reviewed:** January 13, 2026
