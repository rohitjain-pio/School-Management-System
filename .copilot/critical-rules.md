# Critical Rules - NON-NEGOTIABLE

## 🚨 Security Rules (NEVER BREAK THESE)

### Rule 1: SchoolId Isolation
**Every database query MUST filter by SchoolId**

✅ CORRECT:
```csharp
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync();
```

❌ WRONG (DATA BREACH):
```csharp
var students = await _context.Students.ToListAsync(); // Exposes all schools!
```

### Rule 2: BaseSchoolController Inheritance
**All controllers (except Auth & SuperAdmin) MUST inherit from BaseSchoolController**

✅ CORRECT:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentController : BaseSchoolController
{
    // Automatic SchoolId validation
}
```

❌ WRONG:
```csharp
public class StudentController : ControllerBase // Missing isolation!
```

### Rule 3: JWT Claims Validation
**Always verify SchoolId claim exists before data access**

✅ CORRECT:
```csharp
var schoolId = GetSchoolIdFromClaims(); // From BaseSchoolController
if (schoolId == Guid.Empty)
    return BadRequest("Invalid SchoolId");
```

### Rule 4: SuperAdmin Silent Access
**SuperAdmin access MUST be logged for audit trail**

```csharp
if (User.IsInRole("SuperAdmin"))
{
    _logger.LogWarning("SuperAdmin {UserId} accessed school {SchoolId}", 
        userId, schoolId);
}
```

### Rule 5: No Direct SchoolId Input
**Never accept SchoolId from request body - always get from JWT claims**

❌ WRONG (Security vulnerability):
```csharp
public async Task<IActionResult> GetStudents([FromBody] Guid schoolId)
```

✅ CORRECT:
```csharp
var schoolId = GetSchoolIdFromClaims(); // From token only
```

---

## 🏗️ Architecture Rules

### Rule 6: Repository Pattern
**All database access goes through repositories**

```csharp
// Controllers → Services → Repositories → DbContext
// NEVER: Controllers → DbContext directly
```

### Rule 7: DTO Pattern
**Never expose entity models directly to API responses**

✅ Use DTOs:
```csharp
public class StudentResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    // No SchoolId exposed to client
}
```

### Rule 8: Async All The Way
**All data access operations must be asynchronous**

```csharp
public async Task<Student> GetByIdAsync(Guid id, Guid schoolId)
{
    return await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId);
}
```

---

## 📝 Code Quality Rules

### Rule 9: Null Safety
**Always check for null before accessing properties**

```csharp
var user = await _userManager.FindByIdAsync(userId);
if (user == null)
    return NotFound($"User {userId} not found");
```

### Rule 10: Structured Logging
**Use structured logging with Serilog**

✅ CORRECT:
```csharp
_logger.LogInformation("User {UserId} created student {StudentId} in school {SchoolId}",
    userId, studentId, schoolId);
```

❌ WRONG:
```csharp
_logger.LogInformation($"User {userId} created student {studentId}"); // String interpolation
```

---

## 🧪 Testing Rules

### Rule 11: Security Tests Mandatory
**Every endpoint MUST have a cross-school access test**

```csharp
[Fact]
public async Task GetStudent_FromDifferentSchool_ShouldReturn403()
{
    // Verify School A cannot access School B's students
}
```

### Rule 12: Test Coverage Minimum
**Minimum 80% code coverage for services and controllers**

---

## 🚀 Deployment Rules

### Rule 13: Database Migrations
**Never modify database directly - always use EF migrations**

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Rule 14: Environment Variables
**Secrets MUST be in environment variables, never hardcoded**

❌ WRONG:
```csharp
var connString = "Server=...;Password=MyPassword123;";
```

✅ CORRECT:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DB_CONNECTION_STRING}" // From Azure App Settings
  }
}
```

---

## 🔍 Code Review Checklist

Before any commit, verify:
- [ ] SchoolId filter in all queries?
- [ ] Inherits from BaseSchoolController?
- [ ] Using DTOs (not entity models)?
- [ ] Async methods everywhere?
- [ ] Null checks present?
- [ ] Structured logging?
- [ ] Security tests written?
- [ ] No secrets hardcoded?
