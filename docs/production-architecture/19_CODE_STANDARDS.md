# Code Standards & Best Practices
## Maintainable Code for Long-Term Success

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 12 minutes  
**Status:** ✅ Complete

---

## 🎯 **Code Philosophy**

**"Code is read 10x more than it's written. Optimize for readability."**

### **Core Principles**

1. **KISS** (Keep It Simple, Stupid) - Simplicity over cleverness
2. **DRY** (Don't Repeat Yourself) - Reuse, don't duplicate
3. **SOLID** - Object-oriented design principles
4. **YAGNI** (You Aren't Gonna Need It) - Don't over-engineer
5. **Security First** - Every line considers multi-tenant isolation

---

## 📝 **Naming Conventions**

### **C# Backend**

```csharp
// ✅ GOOD: Clear, descriptive names

// Classes: PascalCase
public class StudentService { }
public class SchoolIsolationMiddleware { }

// Interfaces: IPascalCase
public interface IStudentRepository { }
public interface ICacheService { }

// Methods: PascalCase, verb-first
public async Task<Student> GetStudentByIdAsync(Guid id) { }
public async Task<bool> ValidateSchoolOwnershipAsync(Guid schoolId) { }

// Private fields: _camelCase
private readonly ILogger<StudentService> _logger;
private readonly HttpClient _httpClient;

// Properties: PascalCase
public Guid SchoolId { get; set; }
public string FirstName { get; set; }

// Local variables: camelCase
var studentId = Guid.NewGuid();
var schoolName = "ABC School";

// Constants: UPPER_SNAKE_CASE or PascalCase
private const int MAX_STUDENTS_PER_CLASS = 40;
private const string DEFAULT_SCHOOL_CODE = "DEFAULT";

// Boolean properties/methods: Start with Is/Has/Can
public bool IsActive { get; set; }
public bool HasSchoolId() { }
public bool CanAccessSchool(Guid schoolId) { }

// Async methods: End with Async
public async Task<List<Student>> GetStudentsBySchoolIdAsync(Guid schoolId) { }


// ❌ BAD: Unclear, abbreviated names
public class StdSvc { }  // Too abbreviated
public async Task<Student> Get(Guid id) { }  // Not descriptive
private ILogger log;  // Use _logger
public bool active;  // Should be IsActive
```

### **TypeScript Frontend**

```typescript
// ✅ GOOD: Clear, consistent naming

// Components: PascalCase
export function StudentList() { }
export function AddStudentPopup() { }

// Hooks: camelCase, start with 'use'
export function useStudents() { }
export function useAuth() { }

// Functions: camelCase, verb-first
function fetchStudents(schoolId: string) { }
function validateEmail(email: string): boolean { }

// Variables: camelCase
const studentId = "abc-123";
const isLoading = false;

// Constants: UPPER_SNAKE_CASE
const API_BASE_URL = import.meta.env.VITE_API_URL;
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

// Types/Interfaces: PascalCase
interface Student {
  id: string;
  firstName: string;
  schoolId: string;
}

type UserRole = 'Admin' | 'Teacher' | 'Student' | 'Parent';

// Props: camelCase
interface StudentListProps {
  schoolId: string;
  onStudentClick: (id: string) => void;
}


// ❌ BAD: Inconsistent naming
function Students() { }  // Component should be noun: StudentList
const get_students = () => { };  // Use camelCase: getStudents
let student_id: string;  // Use camelCase: studentId
```

---

## 🏗️ **Project Structure**

### **Backend Structure**

```
Backend/
├── SMSPrototype1/ (API Layer)
│   ├── Controllers/          # REST endpoints
│   ├── Middleware/          # Request pipeline (SchoolIsolationMiddleware)
│   ├── Filters/             # Exception filters
│   ├── Extensions/          # Extension methods
│   ├── Attributes/          # Custom attributes
│   └── Program.cs           # App configuration
│
├── SMSServices/ (Business Logic Layer)
│   ├── Services/            # Business logic implementation
│   ├── ServicesInterfaces/  # Service contracts
│   ├── Hubs/               # SignalR hubs
│   └── Validators/          # FluentValidation validators
│
├── SMSRepository/ (Data Access Layer)
│   ├── Repository/          # EF Core repositories
│   └── RepositoryInterfaces/ # Repository contracts
│
├── SMSDataModel/ (Domain Layer)
│   ├── Model/              # Entity classes
│   └── CombineModel/       # DTOs
│
└── SMSDataContext/ (Infrastructure Layer)
    ├── Data/               # DbContext
    ├── Migrations/         # EF migrations
    └── Helpers/            # DB utilities
```

### **Frontend Structure**

```
Frontend/src/
├── components/           # Reusable UI components
│   ├── ui/              # Shadcn components (Button, Input, etc.)
│   └── layout/          # Layout components (Header, Sidebar)
│
├── popups/              # Modal/Popup components
│   ├── students/        # Student-related popups
│   ├── teachers/        # Teacher-related popups
│   └── classes/         # Class-related popups
│
├── hooks/               # Custom React hooks
│   ├── useAuth.ts       # Authentication hook
│   ├── useStudents.ts   # Student data hook
│   └── useCache.ts      # Client-side caching
│
├── services/            # API service layer
│   ├── api.ts           # Axios instance
│   ├── authService.ts   # Auth API calls
│   └── studentService.ts # Student API calls
│
├── types/               # TypeScript type definitions
│   ├── api.ts           # API response types
│   └── models.ts        # Domain models
│
├── utils/               # Utility functions
│   ├── validation.ts    # Form validation
│   └── formatting.ts    # Date/number formatting
│
└── pages/               # Page components (routes)
    ├── Dashboard.tsx
    ├── Students.tsx
    └── Login.tsx
```

---

## 🔒 **Security Best Practices**

### **1. Always Validate SchoolId**

```csharp
// ✅ GOOD: SchoolId from JWT, validated by middleware
[HttpGet]
public async Task<IActionResult> GetStudents()
{
    var schoolId = GetUserSchoolId();  // From BaseSchoolController
    var students = await _studentService.GetStudentsBySchoolIdAsync(schoolId);
    return Ok(students);
}

// ❌ BAD: SchoolId from request body (can be tampered)
[HttpGet]
public async Task<IActionResult> GetStudents([FromQuery] Guid schoolId)
{
    var students = await _studentService.GetStudentsBySchoolIdAsync(schoolId);
    return Ok(students);  // SECURITY HOLE!
}
```

### **2. Validate Ownership Before Operations**

```csharp
// ✅ GOOD: Verify resource belongs to user's school
[HttpPut("{id}")]
public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentDto dto)
{
    var student = await _studentService.GetStudentByIdAsync(id);
    
    if (student == null)
        return NotFound();
    
    // ⭐ CRITICAL: Validate ownership
    ValidateSchoolOwnership(student.SchoolId, "Student");
    
    // Prevent SchoolId change
    dto.SchoolId = student.SchoolId;
    
    var updated = await _studentService.UpdateStudentAsync(id, dto);
    return Ok(updated);
}

// ❌ BAD: No ownership validation
[HttpPut("{id}")]
public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentDto dto)
{
    var updated = await _studentService.UpdateStudentAsync(id, dto);
    return Ok(updated);  // Can update any school's student!
}
```

### **3. Never Log Sensitive Data**

```csharp
// ✅ GOOD: Log without sensitive info
_logger.LogInformation(
    "User {UserId} updated student {StudentId}",
    userId, studentId);

// ❌ BAD: Logs sensitive data
_logger.LogInformation(
    "Updated student: {Student}",
    JsonSerializer.Serialize(student));  // May contain PII, passwords, etc.
```

### **4. Use Parameterized Queries**

```csharp
// ✅ GOOD: EF Core automatically parameterizes
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId && s.FirstName.Contains(searchTerm))
    .ToListAsync();

// ❌ BAD: Raw SQL without parameters (SQL injection risk)
var students = await _context.Students
    .FromSqlRaw($"SELECT * FROM Students WHERE SchoolId = '{schoolId}' AND FirstName LIKE '%{searchTerm}%'")
    .ToListAsync();
```

---

## 🎨 **Code Formatting**

### **C# Code Style**

```csharp
// ✅ GOOD: Clean, readable formatting

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
    
    public async Task<List<Student>> GetStudentsBySchoolIdAsync(Guid schoolId)
    {
        if (schoolId == Guid.Empty)
        {
            throw new ArgumentException("SchoolId cannot be empty", nameof(schoolId));
        }
        
        try
        {
            var students = await _repository.GetBySchoolIdAsync(schoolId);
            
            _logger.LogInformation(
                "Retrieved {Count} students for school {SchoolId}",
                students.Count, schoolId);
            
            return students;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students for school {SchoolId}", schoolId);
            throw;
        }
    }
}

// ❌ BAD: Poor formatting, hard to read
public class StudentService:IStudentService{
private readonly IStudentRepository _repository;
public StudentService(IStudentRepository repository){_repository=repository;}
public async Task<List<Student>> GetStudentsBySchoolIdAsync(Guid schoolId){return await _repository.GetBySchoolIdAsync(schoolId);}
}
```

### **TypeScript Code Style**

```typescript
// ✅ GOOD: Consistent formatting

export function StudentList({ schoolId }: StudentListProps) {
  const [students, setStudents] = useState<Student[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  
  useEffect(() => {
    async function fetchStudents() {
      try {
        setIsLoading(true);
        const response = await api.get<Student[]>('/api/students');
        setStudents(response.data);
      } catch (error) {
        console.error('Error fetching students:', error);
        toast.error('Failed to load students');
      } finally {
        setIsLoading(false);
      }
    }
    
    fetchStudents();
  }, [schoolId]);
  
  if (isLoading) {
    return <LoadingSpinner />;
  }
  
  return (
    <div className="student-list">
      {students.map(student => (
        <StudentCard key={student.id} student={student} />
      ))}
    </div>
  );
}

// ❌ BAD: Inconsistent, hard to read
export function StudentList({schoolId}:StudentListProps){
const [students,setStudents]=useState<Student[]>([])
useEffect(()=>{api.get('/api/students').then(res=>setStudents(res.data))},[])
return <div>{students.map(s=><StudentCard student={s}/>)}</div>
}
```

---

## 📦 **Dependency Injection**

### **Register Services Properly**

```csharp
// ✅ GOOD: Scoped services for per-request instances
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Singleton for stateless services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// Transient for lightweight services
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

// ❌ BAD: Wrong lifetime can cause issues
builder.Services.AddSingleton<IStudentService, StudentService>();  
// DbContext captured in singleton → disposed connection issues
```

---

## 🧪 **Testable Code**

### **Write Testable Methods**

```csharp
// ✅ GOOD: Testable - dependencies injected, returns value
public class StudentService
{
    private readonly IStudentRepository _repository;
    
    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Student> CreateStudentAsync(CreateStudentDto dto)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            SchoolId = dto.SchoolId
        };
        
        return await _repository.AddAsync(student);
    }
}

// Test:
[Fact]
public async Task CreateStudent_WithValidData_ReturnsStudent()
{
    var mockRepo = new Mock<IStudentRepository>();
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);
    
    var service = new StudentService(mockRepo.Object);
    
    var result = await service.CreateStudentAsync(new CreateStudentDto
    {
        FirstName = "John",
        LastName = "Doe",
        SchoolId = Guid.NewGuid()
    });
    
    Assert.NotNull(result);
    Assert.Equal("John", result.FirstName);
}


// ❌ BAD: Hard to test - static dependencies, void return
public class StudentService
{
    public void CreateStudent(CreateStudentDto dto)
    {
        var dbContext = new SMSDbContext();  // Hard-coded dependency
        
        var student = new Student
        {
            FirstName = dto.FirstName,
            SchoolId = dto.SchoolId
        };
        
        dbContext.Students.Add(student);
        dbContext.SaveChanges();
        // No return value - can't verify result
    }
}
```

---

## 📚 **Comments & Documentation**

### **When to Comment**

```csharp
// ✅ GOOD: Comments explain WHY, not WHAT

/// <summary>
/// Validates that a resource belongs to the user's school.
/// SuperAdmin can access any school (logged separately in audit log).
/// </summary>
/// <exception cref="UnauthorizedAccessException">
/// Thrown when user tries to access resource from different school.
/// This is logged as a CRITICAL security incident.
/// </exception>
protected void ValidateSchoolOwnership(Guid resourceSchoolId, string resourceType)
{
    if (IsSuperAdmin()) return;  // SuperAdmin bypass allowed
    
    var userSchoolId = GetUserSchoolId();
    
    if (resourceSchoolId != userSchoolId)
    {
        // Log security violation before throwing
        _logger.LogCritical(
            "SECURITY: User {UserId} from School {UserSchool} attempted to access {ResourceType} from School {ResourceSchool}",
            GetUserId(), userSchoolId, resourceType, resourceSchoolId);
        
        throw new UnauthorizedAccessException(
            $"You do not have permission to access this {resourceType}.");
    }
}

// ❌ BAD: Comments explain obvious code
public async Task<Student> GetStudentByIdAsync(Guid id)
{
    // Check if id is empty
    if (id == Guid.Empty)
    {
        // Throw exception
        throw new ArgumentException("Id cannot be empty");
    }
    
    // Get student from repository
    var student = await _repository.GetByIdAsync(id);
    
    // Return student
    return student;
}
```

### **XML Documentation**

```csharp
// ✅ GOOD: Clear XML docs for public APIs

/// <summary>
/// Gets all students belonging to the specified school.
/// </summary>
/// <param name="schoolId">The unique identifier of the school.</param>
/// <returns>
/// A list of students in the school. Returns empty list if no students found.
/// </returns>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="schoolId"/> is <see cref="Guid.Empty"/>.
/// </exception>
/// <remarks>
/// This method filters by SchoolId to enforce multi-tenant isolation.
/// SuperAdmin users can access students from any school.
/// </remarks>
public async Task<List<Student>> GetStudentsBySchoolIdAsync(Guid schoolId)
{
    // Implementation
}
```

---

## ⚡ **Performance Best Practices**

### **1. Async/Await Properly**

```csharp
// ✅ GOOD: Proper async usage
public async Task<List<Student>> GetStudentsAsync(Guid schoolId)
{
    var students = await _context.Students
        .Where(s => s.SchoolId == schoolId)
        .ToListAsync();
    
    return students;
}

// ❌ BAD: Blocking on async (deadlock risk)
public List<Student> GetStudents(Guid schoolId)
{
    var students = _context.Students
        .Where(s => s.SchoolId == schoolId)
        .ToListAsync()
        .Result;  // Blocking!
    
    return students;
}
```

### **2. Use Projection**

```csharp
// ✅ GOOD: Project to DTO, select only needed fields
public async Task<List<StudentSummaryDto>> GetStudentSummariesAsync(Guid schoolId)
{
    return await _context.Students
        .Where(s => s.SchoolId == schoolId)
        .Select(s => new StudentSummaryDto
        {
            Id = s.Id,
            FullName = $"{s.FirstName} {s.LastName}",
            RollNumber = s.RollNumber
        })
        .ToListAsync();
}

// ❌ BAD: Fetch entire entity then map (slow, memory-intensive)
public async Task<List<StudentSummaryDto>> GetStudentSummariesAsync(Guid schoolId)
{
    var students = await _context.Students
        .Where(s => s.SchoolId == schoolId)
        .ToListAsync();  // All columns loaded
    
    return students.Select(s => new StudentSummaryDto
    {
        Id = s.Id,
        FullName = $"{s.FirstName} {s.LastName}",
        RollNumber = s.RollNumber
    }).ToList();
}
```

### **3. Cache Expensive Queries**

```csharp
// ✅ GOOD: Cache frequently accessed data
public async Task<School> GetSchoolByIdAsync(Guid schoolId)
{
    var cacheKey = $"school:{schoolId}";
    
    var cached = await _cache.GetAsync<School>(cacheKey);
    if (cached != null)
    {
        return cached;
    }
    
    var school = await _context.Schools.FindAsync(schoolId);
    
    if (school != null)
    {
        await _cache.SetAsync(cacheKey, school, TimeSpan.FromMinutes(30));
    }
    
    return school;
}
```

---

## ✅ **Code Review Checklist**

Use this checklist when reviewing pull requests:

```
Security:
[ ] SchoolId validated on all CRUD operations
[ ] No SchoolId accepted from request body (only from JWT)
[ ] ValidateSchoolOwnership() called before updates/deletes
[ ] No sensitive data logged (passwords, tokens, PII)
[ ] SQL queries use parameterization (no string concatenation)

Architecture:
[ ] Controllers inherit BaseSchoolController
[ ] Services injected via constructor (not new'd up)
[ ] Async methods end with "Async" suffix
[ ] Proper HTTP status codes returned (200, 201, 400, 403, 404, 500)

Code Quality:
[ ] Naming follows conventions (PascalCase, camelCase, _privateFields)
[ ] No magic numbers (use constants)
[ ] Complex logic has explanatory comments
[ ] No commented-out code committed
[ ] No TODO comments without issue numbers

Testing:
[ ] Unit tests written for business logic
[ ] Integration tests for API endpoints
[ ] Security tests for cross-school access prevention
[ ] Edge cases covered (null, empty, invalid inputs)

Performance:
[ ] Async/await used correctly (no .Result or .Wait())
[ ] Database queries use projection (Select) not full entities
[ ] N+1 queries avoided (use Include or batch queries)
[ ] Expensive queries cached where appropriate

Frontend:
[ ] TypeScript types defined (no 'any')
[ ] Error states handled (try/catch, error boundaries)
[ ] Loading states shown (spinners, skeletons)
[ ] API calls in services layer (not components)
```

---

## 🚀 **Git Commit Messages**

### **Format**

```
<type>(<scope>): <subject>

<body>

<footer>
```

### **Types**

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code formatting (no logic change)
- `refactor`: Code restructuring (no behavior change)
- `test`: Add/update tests
- `chore`: Maintenance tasks

### **Examples**

```bash
# ✅ GOOD: Clear, descriptive commits

feat(security): implement SchoolIsolationMiddleware

- Add middleware to validate SchoolId on all requests
- Exempt auth endpoints from validation
- Log security violations as CRITICAL
- Closes #42

fix(student): prevent cross-school access in GetStudentById

User from School A could access School B's students by direct ID.
Added ValidateSchoolOwnership() call before returning student.

Fixes #78

test(security): add 20 multi-tenant isolation test cases

Covers all scenarios from security audit:
- Direct ID access
- JWT tampering
- SQL injection attempts
- SuperAdmin access with audit logging


# ❌ BAD: Vague, unhelpful commits

fix: bug
update: changes
wip
asdfasdf
```

---

## 📚 **Additional Resources**

- **C# Coding Conventions:** https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- **React Best Practices:** https://react.dev/learn/thinking-in-react
- **TypeScript Style Guide:** https://google.github.io/styleguide/tsguide.html
- **SOLID Principles:** https://www.digitalocean.com/community/conceptual-articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design

---

## 🎓 **Learning Path for New Developers**

1. Read [01_SYSTEM_OVERVIEW.md](./01_SYSTEM_OVERVIEW.md) for architecture
2. Review [02_MULTI_TENANCY_DESIGN.md](./02_MULTI_TENANCY_DESIGN.md) (CRITICAL)
3. Study existing controllers (StudentController as reference)
4. Write unit tests before implementing features
5. Run security tests after changes
6. Submit small, focused pull requests

---

**Document Status:** ✅ Complete  
**Adoption:** 🟡 In Progress  
**Priority:** Required reading for all contributors