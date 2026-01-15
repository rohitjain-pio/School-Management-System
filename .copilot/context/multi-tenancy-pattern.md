# Multi-Tenancy Pattern - SchoolId Isolation

## Core Concept

**Every piece of data belongs to exactly ONE school (except SuperAdmin data).**

All database tables (except system tables) have a `SchoolId` column, and ALL queries MUST filter by this column to prevent data leakage between schools.

---

## Database Schema Pattern

### Rule: SchoolId Column on Every Table

```sql
CREATE TABLE Students (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,  -- MANDATORY
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    -- ... other columns
    
    CONSTRAINT FK_Students_Schools 
        FOREIGN KEY (SchoolId) REFERENCES Schools(Id) ON DELETE CASCADE
);

-- Non-clustered index for performance
CREATE NONCLUSTERED INDEX IX_Students_SchoolId 
    ON Students(SchoolId);
```

### Tables with SchoolId (35+ total)

**Core Entities:**
- Schools (no SchoolId - this IS the school)
- Students, Teachers, Parents
- Classes, Subjects, Sections

**Academic:**
- Attendance, Grades, Assignments
- Exams, ExamResults
- Timetables

**Administrative:**
- FeeStructures, Payments, FeeTransactions
- Holidays, Announcements
- Documents

**Communication:**
- ChatRooms, ChatMessages
- Notifications

**System Tables (NO SchoolId):**
- AspNetUsers (has SchoolId as FK, but is system table)
- AspNetRoles, AspNetUserRoles
- AuditLogs (has SchoolId but for tracking only)

---

## JWT Token Pattern

### SchoolId in Token Claims

Every JWT token MUST include SchoolId claim (except SuperAdmin):

```csharp
// In AuthService.cs - Login method
var authClaims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    
    // CRITICAL: Add SchoolId claim
    new Claim("SchoolId", user.SchoolId.ToString())
};

// Add roles
foreach (var role in userRoles)
{
    authClaims.Add(new Claim(ClaimTypes.Role, role));
}
```

### SuperAdmin Token (Special Case)

```csharp
// SuperAdmin has SchoolId = Guid.Empty
if (user.SchoolId != Guid.Empty)
{
    authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
}
else
{
    // SuperAdmin - no SchoolId claim
    // They must specify SchoolId when accessing school data
}
```

---

## Middleware Pattern

### SchoolIsolationMiddleware

**File:** `Backend/SMSPrototype1/Middleware/SchoolIsolationMiddleware.cs`

```csharp
public class SchoolIsolationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SchoolIsolationMiddleware> _logger;

    public SchoolIsolationMiddleware(
        RequestDelegate next,
        ILogger<SchoolIsolationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // SuperAdmin bypass (with logging)
        if (context.User.IsInRole("SuperAdmin"))
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogWarning(
                "SuperAdmin {UserId} accessed {Path} without SchoolId restriction",
                userId,
                context.Request.Path
            );
            await _next(context);
            return;
        }

        // Validate SchoolId claim exists
        var schoolIdClaim = context.User.FindFirst("SchoolId");
        if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or missing SchoolId claim",
                message = "Your account is not associated with a school"
            });
            return;
        }

        // Store SchoolId in HttpContext for easy access
        context.Items["SchoolId"] = schoolId;

        await _next(context);
    }
}
```

**Registration in Program.cs:**

```csharp
app.UseAuthentication();
app.UseMiddleware<SchoolIsolationMiddleware>(); // After authentication
app.UseAuthorization();
```

---

## Controller Pattern

### BaseSchoolController

**File:** `Backend/SMSPrototype1/Controllers/BaseSchoolController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public abstract class BaseSchoolController : ControllerBase
{
    /// <summary>
    /// Gets the SchoolId from JWT claims
    /// </summary>
    protected Guid GetSchoolIdFromClaims()
    {
        var schoolIdClaim = User.FindFirst("SchoolId");
        
        if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim.Value, out var schoolId))
        {
            return Guid.Empty;
        }
        
        return schoolId;
    }

    /// <summary>
    /// Validates that an entity belongs to the current user's school
    /// </summary>
    protected async Task<bool> ValidateSchoolOwnership<T>(
        Guid entityId,
        Func<Guid, Guid, Task<T?>> getEntityFunc) where T : class
    {
        var schoolId = GetSchoolIdFromClaims();
        
        if (schoolId == Guid.Empty)
        {
            return false;
        }

        var entity = await getEntityFunc(entityId, schoolId);
        return entity != null;
    }

    /// <summary>
    /// Returns standardized 403 Forbidden response
    /// </summary>
    protected IActionResult ForbidSchoolAccess(string? message = null)
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            new
            {
                error = "School Access Denied",
                message = message ?? "You do not have permission to access this resource"
            }
        );
    }

    /// <summary>
    /// Checks if current user is SuperAdmin
    /// </summary>
    protected bool IsSuperAdmin()
    {
        return User.IsInRole("SuperAdmin");
    }

    /// <summary>
    /// Logs SuperAdmin access for audit trail
    /// </summary>
    protected void LogSuperAdminAccess(ILogger logger, string action, Guid? targetSchoolId = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        logger.LogWarning(
            "AUDIT: SuperAdmin {UserId} performed {Action} on school {SchoolId}",
            userId,
            action,
            targetSchoolId
        );
    }
}
```

### Controller Inheritance

**All controllers (except Auth) must inherit from BaseSchoolController:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentController : BaseSchoolController
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(
        IStudentService studentService,
        ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStudents()
    {
        var schoolId = GetSchoolIdFromClaims();
        
        if (schoolId == Guid.Empty)
        {
            return ForbidSchoolAccess("Invalid SchoolId");
        }

        var students = await _studentService.GetAllAsync(schoolId);
        return Ok(students);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var schoolId = GetSchoolIdFromClaims();
        
        if (schoolId == Guid.Empty)
        {
            return ForbidSchoolAccess();
        }

        var student = await _studentService.GetByIdAsync(id, schoolId);
        
        if (student == null)
        {
            return NotFound($"Student {id} not found in your school");
        }

        return Ok(student);
    }
}
```

---

## Repository Pattern

### Repository Method Signature

**Every repository method MUST accept SchoolId parameter:**

```csharp
public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync(Guid schoolId);
    Task<Student?> GetByIdAsync(Guid id, Guid schoolId);
    Task<Student> AddAsync(Student student); // student.SchoolId already set
    Task<bool> UpdateAsync(Student student, Guid schoolId);
    Task<bool> DeleteAsync(Guid id, Guid schoolId);
}
```

### Repository Implementation

```csharp
public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllAsync(Guid schoolId)
    {
        return await _context.Students
            .Where(s => s.SchoolId == schoolId) // MANDATORY FILTER
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(Guid id, Guid schoolId)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => 
                s.Id == id && 
                s.SchoolId == schoolId); // Double filter
    }

    public async Task<Student> AddAsync(Student student)
    {
        // SchoolId must already be set by caller
        if (student.SchoolId == Guid.Empty)
        {
            throw new ArgumentException("Student must have a valid SchoolId");
        }

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<bool> UpdateAsync(Student student, Guid schoolId)
    {
        // Verify ownership before update
        var existing = await GetByIdAsync(student.Id, schoolId);
        
        if (existing == null)
        {
            return false; // Not found or wrong school
        }

        _context.Entry(existing).CurrentValues.SetValues(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid schoolId)
    {
        var student = await GetByIdAsync(id, schoolId);
        
        if (student == null)
        {
            return false;
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

---

## Service Layer Pattern

### Service Methods

```csharp
public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllAsync(Guid schoolId);
    Task<StudentDto?> GetByIdAsync(Guid id, Guid schoolId);
    Task<StudentDto> CreateAsync(CreateStudentDto dto, Guid schoolId);
    Task<bool> UpdateAsync(Guid id, UpdateStudentDto dto, Guid schoolId);
    Task<bool> DeleteAsync(Guid id, Guid schoolId);
}
```

### Service Implementation

```csharp
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository repository,
        ILogger<StudentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync(Guid schoolId)
    {
        var students = await _repository.GetAllAsync(schoolId);
        
        return students.Select(s => new StudentDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            // Note: SchoolId is NOT exposed to client
        });
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto dto, Guid schoolId)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId, // Set from service layer
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            // ... other properties
        };

        var created = await _repository.AddAsync(student);
        
        _logger.LogInformation(
            "Student {StudentId} created in school {SchoolId}",
            created.Id,
            schoolId
        );

        return new StudentDto
        {
            Id = created.Id,
            FirstName = created.FirstName,
            LastName = created.LastName
        };
    }
}
```

---

## Frontend Pattern

### API Client Configuration

```typescript
// src/lib/api-client.ts
import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add JWT token to all requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

### API Calls (SchoolId Automatic)

```typescript
// src/services/student-service.ts
import apiClient from '@/lib/api-client';

export const studentService = {
  // SchoolId is extracted from JWT token by backend
  // Frontend NEVER sends SchoolId explicitly
  
  async getAllStudents() {
    const response = await apiClient.get('/api/student');
    return response.data;
  },

  async getStudent(id: string) {
    const response = await apiClient.get(`/api/student/${id}`);
    return response.data;
  },

  async createStudent(data: CreateStudentDto) {
    // NO SchoolId in request body
    const response = await apiClient.post('/api/student', data);
    return response.data;
  }
};
```

---

## Testing Pattern

### Unit Test Example

```csharp
[Fact]
public async Task GetStudent_FromDifferentSchool_ShouldReturnNull()
{
    // Arrange
    var schoolA = Guid.NewGuid();
    var schoolB = Guid.NewGuid();
    var studentId = Guid.NewGuid();

    var student = new Student
    {
        Id = studentId,
        SchoolId = schoolA,
        FirstName = "John",
        LastName = "Doe"
    };

    _context.Students.Add(student);
    await _context.SaveChangesAsync();

    var repository = new StudentRepository(_context);

    // Act - Try to get student from School B
    var result = await repository.GetByIdAsync(studentId, schoolB);

    // Assert
    Assert.Null(result); // Should not find student
}
```

### Integration Test Example

```csharp
[Fact]
public async Task GetStudent_WithValidToken_ShouldReturnOnlyOwnSchoolData()
{
    // Arrange
    var schoolA = CreateTestSchool("School A");
    var schoolB = CreateTestSchool("School B");
    
    var studentA = CreateTestStudent(schoolA.Id, "Alice");
    var studentB = CreateTestStudent(schoolB.Id, "Bob");

    var tokenSchoolA = GenerateJwtToken(schoolA.Id, "SchoolAdmin");

    // Act
    var response = await _client.GetAsync(
        "/api/student",
        token: tokenSchoolA
    );

    // Assert
    response.Should().BeSuccessful();
    var students = await response.Content.ReadAsAsync<List<StudentDto>>();
    
    students.Should().ContainSingle();
    students[0].FirstName.Should().Be("Alice");
}
```

---

## Common Mistakes to Avoid

### ❌ WRONG: Accepting SchoolId from client

```csharp
[HttpGet]
public async Task<IActionResult> GetStudents([FromQuery] Guid schoolId)
{
    // SECURITY VULNERABILITY: Client can specify any SchoolId
    var students = await _studentService.GetAllAsync(schoolId);
    return Ok(students);
}
```

### ❌ WRONG: Forgetting SchoolId filter

```csharp
public async Task<Student?> GetByIdAsync(Guid id)
{
    // DATA BREACH: Returns student from ANY school
    return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
}
```

### ❌ WRONG: Using Contains instead of Equals

```csharp
// WRONG: Contains is for collections
.Where(s => s.SchoolId.Contains(schoolId))

// CORRECT: Use equality
.Where(s => s.SchoolId == schoolId)
```

### ✅ CORRECT: Always filter by SchoolId

```csharp
[HttpGet]
public async Task<IActionResult> GetStudents()
{
    var schoolId = GetSchoolIdFromClaims(); // From JWT
    var students = await _studentService.GetAllAsync(schoolId);
    return Ok(students);
}
```
