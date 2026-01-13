# Multi-Tenancy Design
## School Isolation Strategy - CRITICAL SECURITY DOCUMENT

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 20 minutes  
**Classification:** ⭐ CRITICAL - MUST IMPLEMENT FIRST  
**Status:** 🔴 NOT YET IMPLEMENTED - BLOCKING PRODUCTION

---

## ⚠️ **CRITICAL WARNING**

**This document describes the MOST IMPORTANT security feature of the system.**

**Without proper implementation:**
- ❌ Schools can access each other's data
- ❌ Students from School A visible to School B
- ❌ Grades, attendance, chat messages exposed
- ❌ CATASTROPHIC data breach
- ❌ Legal liability, business failure

**Implementation Priority:** #1 - DO THIS FIRST  
**Testing Required:** Mandatory before any production use  
**Review Frequency:** Every deployment

---

## 🎯 **What is Multi-Tenancy?**

### **Definition**

**Multi-Tenancy:** Multiple schools (tenants) use the same application and database, but each school's data is completely isolated from others.

**Analogy:** 
Apartment building (multi-tenant) vs. separate houses (single-tenant)
- Same building structure (shared application)
- Same utilities (shared database)
- But residents can't access each other's apartments (data isolation)

### **Our Multi-Tenancy Model**

**Pattern:** Shared Database + Shared Schema + Discriminator Column

```
┌─────────────────────────────────────────┐
│         SQL Server Database              │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │        Students Table              │ │
│  ├────┬─────────┬───────────┬────────┤ │
│  │ Id │SchoolId │ FirstName │ Grade  │ │
│  ├────┼─────────┼───────────┼────────┤ │
│  │ 1  │ AAA-111 │ Raj       │ 10     │ │ ← School A
│  │ 2  │ AAA-111 │ Priya     │ 10     │ │ ← School A
│  │ 3  │ BBB-222 │ Amit      │ 10     │ │ ← School B
│  │ 4  │ BBB-222 │ Sneha     │ 10     │ │ ← School B
│  │ 5  │ CCC-333 │ Rohit     │ 10     │ │ ← School C
│  └────┴─────────┴───────────┴────────┘ │
└─────────────────────────────────────────┘

Query from School A user:
SELECT * FROM Students WHERE SchoolId = 'AAA-111'
Result: Only rows 1, 2 (Raj, Priya)
```

**Key Principle:** SchoolId is ALWAYS in the WHERE clause

---

## 🏗️ **SchoolId: The Discriminator**

### **What is SchoolId?**

**Type:** Guid (Globally Unique Identifier)  
**Format:** `12345678-1234-1234-1234-123456789abc`  
**Storage:** Every multi-tenant table has SchoolId column

### **SchoolId in Database**

**Tables WITH SchoolId (Multi-Tenant):**
- ✅ Students, Teachers, Parents, Classes, Subjects
- ✅ Attendance, Grades, Assignments
- ✅ ChatRooms, ChatMessages
- ✅ Announcements, Files, Reports
- ✅ SchoolSettings, AcademicYears

**Tables WITHOUT SchoolId (Platform-Wide):**
- ❌ Schools (this IS the school table)
- ❌ AspNetUsers (user can belong to multiple schools as Parent)
- ❌ AspNetRoles, AspNetUserRoles (global roles)
- ❌ AuditLogs (cross-school audit trail)
- ❌ SystemSettings (platform configuration)

### **SchoolId in Code**

**Entity Definition:**
```csharp
public class Student
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid SchoolId { get; set; }  // ← THE DISCRIMINATOR
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Grade { get; set; }
    
    // Navigation property
    public School School { get; set; }
}
```

**Database Index (CRITICAL for Performance):**
```sql
CREATE INDEX IX_Students_SchoolId 
ON Students(SchoolId) 
INCLUDE (FirstName, LastName, Grade);
```

---

## 🔒 **3-Layer Isolation Strategy**

### **Layer 1: Authentication (JWT Claims)**

**When User Logs In:**
```csharp
// AuthService.GenerateJwtToken()
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("SchoolId", user.SchoolId.ToString()), // ← CRITICAL
};

var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.UtcNow.AddHours(3),
    signingCredentials: credentials
);
```

**JWT Token Contains:**
```json
{
  "nameid": "user-guid",
  "email": "teacher@school-a.com",
  "role": "Teacher",
  "schoolId": "AAA-111-222-333", // ← This is embedded in token
  "exp": 1705334400
}
```

**Key Point:** SchoolId is cryptographically signed in JWT. User cannot change it without invalidating signature.

### **Layer 2: Middleware (Request Validation)**

**SchoolIsolationMiddleware.cs** (TO BE CREATED)

```csharp
public class SchoolIsolationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SchoolIsolationMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/Auth"))
        {
            await _next(context);
            return;
        }

        // Extract SchoolId from JWT claims
        var schoolIdClaim = context.User.FindFirst("SchoolId");
        
        // Check if user is SuperAdmin (can bypass)
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "SuperAdmin")
        {
            // Allow but LOG this access
            _logger.LogWarning("SuperAdmin accessed {Path} at {Time}", 
                context.Request.Path, DateTime.UtcNow);
            await _next(context);
            return;
        }

        // Validate SchoolId present
        if (schoolIdClaim == null || 
            !Guid.TryParse(schoolIdClaim.Value, out var schoolId) ||
            schoolId == Guid.Empty)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "School ID is required",
                message = "Your account is not associated with a school" 
            });
            return;
        }

        // Store in HttpContext for controllers
        context.Items["SchoolId"] = schoolId;
        
        await _next(context);
    }
}
```

**Register in Program.cs:**
```csharp
// After app.UseAuthentication()
app.UseMiddleware<SchoolIsolationMiddleware>();
```

### **Layer 3: Controller/Service (Data Filtering)**

**BaseSchoolController.cs** (TO BE CREATED)

```csharp
[ApiController]
[Authorize]
public abstract class BaseSchoolController : ControllerBase
{
    protected Guid GetUserSchoolId()
    {
        // Extract from claims (already validated by middleware)
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        
        if (Guid.TryParse(schoolIdClaim, out var schoolId))
            return schoolId;
            
        throw new UnauthorizedAccessException("Invalid School ID");
    }

    protected async Task<bool> ValidateSchoolOwnership(
        Guid schoolId, 
        string resourceName)
    {
        var userSchoolId = GetUserSchoolId();
        
        if (userSchoolId != schoolId)
        {
            _logger.LogWarning(
                "User {UserId} from School {UserSchool} " +
                "attempted to access {Resource} from School {TargetSchool}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userSchoolId,
                resourceName,
                schoolId
            );
            return false;
        }
        
        return true;
    }
}
```

**Usage in Controller:**
```csharp
public class StudentController : BaseSchoolController
{
    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var schoolId = GetUserSchoolId(); // From BaseSchoolController
        
        var students = await _studentService
            .GetStudentsBySchoolAsync(schoolId);
            
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        
        if (student == null)
            return NotFound();
            
        // CRITICAL: Validate ownership
        if (!await ValidateSchoolOwnership(student.SchoolId, "Student"))
            return Forbid();
            
        return Ok(student);
    }
}
```

---

## 🛡️ **Attack Scenarios & Mitigations**

### **Attack 1: Modify JWT Token**

**Attack:**
```
1. User from School A gets JWT token
2. Decodes JWT (it's base64, not encrypted)
3. Changes SchoolId claim to School B's ID
4. Re-encodes JWT
5. Sends request with modified token
```

**Mitigation:**
```
✅ JWT is SIGNED with secret key
✅ Modified token has invalid signature
✅ Middleware rejects invalid signature
✅ User gets 401 Unauthorized
```

**Code:**
```csharp
// In token validation (Program.cs)
TokenValidationParameters = new()
{
    ValidateIssuerSigningKey = true,  // ← Validates signature
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
}
```

### **Attack 2: SQL Injection to Bypass SchoolId**

**Attack:**
```sql
-- User sends malicious input
studentName = "'; DROP TABLE Students; --"

-- Hoping to generate query like:
SELECT * FROM Students 
WHERE SchoolId = 'AAA-111' AND Name = ''; DROP TABLE Students; --'
```

**Mitigation:**
```
✅ Use parameterized queries (Entity Framework)
✅ Input validation (FluentValidation)
✅ Never concatenate user input into SQL
```

**Safe Code (Entity Framework):**
```csharp
// This is SAFE - EF uses parameters
var student = await _context.Students
    .Where(s => s.SchoolId == schoolId && s.Name == studentName)
    .FirstOrDefaultAsync();

// Generated SQL (parameterized):
SELECT * FROM Students 
WHERE SchoolId = @p0 AND Name = @p1
```

### **Attack 3: Direct Database Access**

**Attack:**
```
1. Attacker compromises database credentials
2. Directly connects to SQL Server
3. Runs: SELECT * FROM Students (gets all schools)
```

**Mitigation:**
```
✅ Database encryption at rest (TDE)
✅ Sensitive columns encrypted (SSN, Aadhaar)
✅ Database firewall rules
✅ Audit logging at database level
✅ Principle of least privilege (app user has limited permissions)
```

### **Attack 4: API Parameter Manipulation**

**Attack:**
```
GET /api/Student/12345?schoolId=DIFFERENT_SCHOOL_ID
```

**Mitigation:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetStudent(Guid id, [FromQuery] Guid? schoolId)
{
    // NEVER trust schoolId from query parameter!
    // Always use authenticated user's SchoolId
    
    var userSchoolId = GetUserSchoolId(); // From JWT, not parameter
    
    var student = await _studentService
        .GetStudentByIdAsync(id, userSchoolId); // Pass user's school
        
    return Ok(student);
}
```

---

## ✅ **Testing School Isolation**

### **Test Case 1: Cross-School Data Access**

```csharp
[Fact]
public async Task TeacherCannotAccessOtherSchoolStudents()
{
    // Arrange
    var schoolA = Guid.NewGuid();
    var schoolB = Guid.NewGuid();
    
    var teacherSchoolA = CreateTeacher(schoolA);
    var studentSchoolB = CreateStudent(schoolB);
    
    // Act
    var token = GenerateJwtToken(teacherSchoolA);
    var response = await client.GetAsync(
        $"/api/Student/{studentSchoolB.Id}",
        headers: new { Authorization = $"Bearer {token}" }
    );
    
    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

### **Test Case 2: SuperAdmin Can Access Any School**

```csharp
[Fact]
public async Task SuperAdminCanAccessAllSchools()
{
    // Arrange
    var schoolA = Guid.NewGuid();
    var superAdmin = CreateSuperAdmin(); // No specific school
    var studentSchoolA = CreateStudent(schoolA);
    
    // Act
    var token = GenerateJwtToken(superAdmin);
    var response = await client.GetAsync(
        $"/api/Student/{studentSchoolA.Id}",
        headers: new { Authorization = $"Bearer {token}" }
    );
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    // Verify audit log created
    var auditLog = await _context.AuditLogs
        .Where(a => a.Action == "SuperAdminAccess")
        .FirstOrDefaultAsync();
    Assert.NotNull(auditLog);
}
```

---

## 🚨 **Production Checklist**

**Before Going Live:**

- [ ] SchoolIsolationMiddleware implemented and registered
- [ ] BaseSchoolController created
- [ ] All 11 controllers inherit BaseSchoolController
- [ ] SchoolId claim added to JWT generation
- [ ] All multi-tenant tables have SchoolId column
- [ ] Database indexes created on SchoolId columns
- [ ] Cross-school access tests passing (20+ test cases)
- [ ] SuperAdmin bypass with audit logging working
- [ ] Security review completed
- [ ] Penetration testing done

**Monitoring (Post-Launch):**

- [ ] Alert on failed SchoolId validations
- [ ] Alert on SuperAdmin access
- [ ] Daily report of cross-school access attempts
- [ ] Monthly security audit

---

## 📚 **Next Steps**

1. **Read:** [03_SECURITY_ARCHITECTURE.md](./03_SECURITY_ARCHITECTURE.md) ⭐
2. **Implement:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md) ⭐
3. **Test:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)

---

**Document Status:** ✅ Complete  
**Implementation Status:** 🔴 NOT IMPLEMENTED  
**Blocking:** Production Launch
