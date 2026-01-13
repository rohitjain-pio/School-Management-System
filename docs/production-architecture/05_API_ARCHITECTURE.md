# API Architecture
## RESTful API Design Patterns & Standards

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 18 minutes  
**Status:** ✅ Current

---

## 🎯 **API Design Principles**

### **REST Principles**

1. **Resource-Based:** URLs represent entities, not actions
2. **HTTP Verbs:** Use GET, POST, PUT, DELETE appropriately
3. **Stateless:** Each request contains all needed information
4. **Consistent:** Predictable patterns across all endpoints
5. **Versioned:** Support multiple API versions simultaneously

### **Our API Standards**

```
✅ DO: /api/students               (plural, lowercase)
❌ DON'T: /api/GetStudents         (verbs in URL)
❌ DON'T: /api/Student             (singular)
❌ DON'T: /api/Students            (capitalized)
```

---

## 🛣️ **API Endpoint Structure**

### **Base URL**

```
Development:  https://localhost:7266/api
Production:   https://api.schoolms.com/api
```

### **Endpoint Patterns**

| HTTP Method | Endpoint | Description | Auth Required |
|-------------|----------|-------------|---------------|
| **GET** | `/api/students` | Get all students (filtered by SchoolId) | ✅ |
| **GET** | `/api/students/{id}` | Get student by ID | ✅ |
| **POST** | `/api/students` | Create new student | ✅ Admin/Teacher |
| **PUT** | `/api/students/{id}` | Update student | ✅ Admin/Teacher |
| **DELETE** | `/api/students/{id}` | Soft delete student | ✅ Admin only |
| **GET** | `/api/students/{id}/attendance` | Get student attendance | ✅ |
| **GET** | `/api/students/{id}/grades` | Get student grades | ✅ |

---

## 📋 **Complete API Reference**

### **1. Authentication APIs**

```http
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh-token
POST   /api/auth/logout
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
GET    /api/auth/verify-email
```

**Example: Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "teacher@school.com",
  "password": "SecurePass123!"
}

Response 200 OK:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "stored-in-httponly-cookie",
  "user": {
    "id": "guid",
    "email": "teacher@school.com",
    "role": "Teacher",
    "schoolId": "school-guid",
    "schoolName": "ABC Public School"
  }
}
```

### **2. School Management (SuperAdmin Only)**

```http
GET    /api/schools                    # List all schools
GET    /api/schools/{id}               # Get school details
POST   /api/schools                    # Create new school
PUT    /api/schools/{id}               # Update school
DELETE /api/schools/{id}               # Delete school (soft)
PATCH  /api/schools/{id}/suspend       # Suspend subscription
PATCH  /api/schools/{id}/activate      # Activate subscription
GET    /api/schools/{id}/statistics    # School analytics
```

**Example: Create School**
```http
POST /api/schools
Authorization: Bearer {superadmin-token}
Content-Type: application/json

{
  "name": "XYZ Public School",
  "address": "123 Main St",
  "city": "Mumbai",
  "state": "Maharashtra",
  "pinCode": "400001",
  "phone": "022-12345678",
  "email": "admin@xyzschool.com",
  "adminEmail": "principal@xyzschool.com",
  "adminName": "Dr. Sharma"
}

Response 201 Created:
{
  "id": "new-school-guid",
  "name": "XYZ Public School",
  "adminCredentials": {
    "email": "principal@xyzschool.com",
    "temporaryPassword": "TempPass2026!",
    "mustChangePassword": true
  },
  "message": "School created. Admin credentials sent to principal@xyzschool.com"
}
```

### **3. Student APIs**

```http
GET    /api/students                   # List students (school-filtered)
GET    /api/students/{id}              # Get student by ID
POST   /api/students                   # Create student
PUT    /api/students/{id}              # Update student
DELETE /api/students/{id}              # Soft delete
POST   /api/students/bulk-import       # CSV import
GET    /api/students/{id}/attendance   # Student attendance history
GET    /api/students/{id}/grades       # Student grades
GET    /api/students/export            # Export to CSV/Excel
```

**Query Parameters (Filtering):**
```http
GET /api/students?classId={guid}&grade=10&search=raj&page=1&pageSize=20

Response:
{
  "data": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### **4. Teacher APIs**

```http
GET    /api/teachers
GET    /api/teachers/{id}
POST   /api/teachers
PUT    /api/teachers/{id}
DELETE /api/teachers/{id}
GET    /api/teachers/{id}/classes      # Classes assigned
GET    /api/teachers/{id}/subjects     # Subjects taught
POST   /api/teachers/bulk-import
```

### **5. Class APIs**

```http
GET    /api/classes
GET    /api/classes/{id}
POST   /api/classes
PUT    /api/classes/{id}
DELETE /api/classes/{id}
GET    /api/classes/{id}/students      # Students in class
POST   /api/classes/{id}/students      # Add student to class
DELETE /api/classes/{id}/students/{studentId}  # Remove from class
GET    /api/classes/{id}/subjects      # Subjects for class
POST   /api/classes/{id}/subjects      # Add subject to class
```

### **6. Attendance APIs**

```http
GET    /api/attendance?date=2026-01-13&classId={guid}
POST   /api/attendance                 # Mark attendance (bulk)
PUT    /api/attendance/{id}            # Update single record
GET    /api/attendance/report?studentId={guid}&from=2026-01-01&to=2026-01-31
GET    /api/attendance/statistics?classId={guid}&month=2026-01
```

**Example: Bulk Mark Attendance**
```http
POST /api/attendance
Authorization: Bearer {teacher-token}
Content-Type: application/json

{
  "classId": "class-guid",
  "date": "2026-01-13",
  "records": [
    { "studentId": "student1-guid", "status": "Present" },
    { "studentId": "student2-guid", "status": "Absent", "remarks": "Sick leave" },
    { "studentId": "student3-guid", "status": "Late" }
  ]
}

Response 200 OK:
{
  "success": true,
  "totalRecords": 3,
  "message": "Attendance marked for 3 students"
}
```

### **7. Grades APIs**

```http
GET    /api/grades?studentId={guid}
POST   /api/grades                     # Enter grades
PUT    /api/grades/{id}
DELETE /api/grades/{id}
GET    /api/grades/report?studentId={guid}&term=Term1&year=2025-2026
GET    /api/grades/class-report?classId={guid}&subjectId={guid}&examType=Quarterly
POST   /api/grades/bulk-import         # Import from Excel
```

### **8. Chat APIs**

```http
GET    /api/chat/rooms                 # User's chat rooms
GET    /api/chat/rooms/{id}            # Room details
POST   /api/chat/rooms                 # Create room (group chat)
GET    /api/chat/rooms/{id}/messages   # Get messages (paginated)
POST   /api/chat/rooms/{id}/messages   # Send message
PUT    /api/chat/messages/{id}         # Edit message
DELETE /api/chat/messages/{id}         # Delete message
POST   /api/chat/rooms/{id}/members    # Add member to room
DELETE /api/chat/rooms/{id}/members/{userId}  # Remove member
```

### **9. Announcements APIs**

```http
GET    /api/announcements              # School announcements
GET    /api/announcements/{id}
POST   /api/announcements              # Create (Admin/Teacher)
PUT    /api/announcements/{id}
DELETE /api/announcements/{id}
POST   /api/announcements/{id}/publish # Publish announcement
```

### **10. File Upload APIs**

```http
POST   /api/files/upload               # Upload file (multipart/form-data)
GET    /api/files/{id}                 # Download file
DELETE /api/files/{id}                 # Delete file
GET    /api/files?entityType=Student&entityId={guid}  # Get entity files
```

**Example: File Upload**
```http
POST /api/files/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

FormData:
  file: [binary]
  entityType: "Student"
  entityId: "student-guid"
  category: "Profile Picture"

Response 200 OK:
{
  "fileId": "file-guid",
  "fileName": "profile.jpg",
  "fileUrl": "https://storage.blob.core.windows.net/school-files/...",
  "size": 245678,
  "uploadedAt": "2026-01-13T10:30:00Z"
}
```

---

## 📦 **Request/Response DTOs**

### **Common Patterns**

**CreateStudentDto:**
```csharp
public class CreateStudentDto
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    public string Gender { get; set; } // Male, Female, Other
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    [Required]
    public Guid ClassId { get; set; }
    
    // Note: SchoolId comes from JWT, not from request body
}
```

**StudentResponseDto:**
```csharp
public class StudentResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; } // Calculated
    public string Gender { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string AdmissionNumber { get; set; }
    public DateTime AdmissionDate { get; set; }
    
    // Related entities
    public ClassDto Class { get; set; }
    public SchoolDto School { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

## ⚠️ **Error Handling**

### **Standard Error Response**

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "FirstName": ["First name is required"],
    "Email": ["Invalid email format"]
  },
  "timestamp": "2026-01-13T10:30:00Z",
  "path": "/api/students"
}
```

### **HTTP Status Codes**

| Code | Meaning | When to Use |
|------|---------|-------------|
| **200** | OK | Successful GET, PUT |
| **201** | Created | Successful POST (created resource) |
| **204** | No Content | Successful DELETE |
| **400** | Bad Request | Validation errors, malformed request |
| **401** | Unauthorized | Missing or invalid JWT token |
| **403** | Forbidden | Valid token but insufficient permissions |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Duplicate entry (e.g., email already exists) |
| **500** | Server Error | Unexpected server error |

### **Error Handling Middleware**

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            ValidationException ex => new ErrorResponse
            {
                StatusCode = 400,
                Message = "Validation failed",
                Errors = ex.Errors
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = 403,
                Message = "You don't have permission to access this resource"
            },
            NotFoundException ex => new ErrorResponse
            {
                StatusCode = 404,
                Message = ex.Message
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                Message = "An unexpected error occurred"
            }
        };
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        
        return true;
    }
}
```

---

## 🔒 **Security Headers**

### **Required Headers**

**Request Headers:**
```http
Authorization: Bearer {jwt-token}
Content-Type: application/json
X-Request-ID: {unique-id}  # For tracing
```

**Response Headers:**
```http
Content-Type: application/json
X-Request-ID: {same-unique-id}
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1705334400
```

### **CORS Configuration**

```csharp
app.UseCors(policy => policy
    .WithOrigins("https://yourdomain.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithExposedHeaders("X-Request-ID", "X-RateLimit-Remaining"));
```

---

## 📊 **Pagination & Filtering**

### **Pagination Pattern**

```http
GET /api/students?page=1&pageSize=20&sortBy=firstName&sortOrder=asc

Response:
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

### **Implementation**

```csharp
public class PagedResult<T>
{
    public List<T> Data { get; set; }
    public PaginationMetadata Pagination { get; set; }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

// Usage
[HttpGet]
public async Task<ActionResult<PagedResult<StudentDto>>> GetStudents(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var schoolId = GetUserSchoolId();
    
    var query = _context.Students
        .Where(s => s.SchoolId == schoolId && !s.IsDeleted);
    
    var totalCount = await query.CountAsync();
    
    var students = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
    
    return new PagedResult<StudentDto>
    {
        Data = students,
        Pagination = new PaginationMetadata
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }
    };
}
```

---

## 🚀 **Performance Best Practices**

### **1. Use Projection (Select Only Needed Fields)**

```csharp
// ❌ BAD: Fetches all columns
var students = await _context.Students.ToListAsync();

// ✅ GOOD: Fetches only needed columns
var students = await _context.Students
    .Select(s => new StudentListDto
    {
        Id = s.Id,
        FullName = s.FirstName + " " + s.LastName,
        ClassId = s.ClassId,
        ClassName = s.Class.Name
    })
    .ToListAsync();
```

### **2. Eager Loading vs. Lazy Loading**

```csharp
// Include related entities when needed
var student = await _context.Students
    .Include(s => s.Class)
    .Include(s => s.School)
    .FirstOrDefaultAsync(s => s.Id == id);
```

### **3. Caching**

```csharp
[ResponseCache(Duration = 300)] // 5 minutes
[HttpGet("{id}")]
public async Task<IActionResult> GetSchool(Guid id)
{
    var school = await _cache.GetOrCreateAsync(
        $"school:{id}",
        async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _schoolService.GetByIdAsync(id);
        });
    
    return Ok(school);
}
```

---

## 📚 **API Documentation**

### **Swagger/OpenAPI**

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Management System API",
        Version = "v1",
        Description = "Multi-tenant school management platform"
    });
    
    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Access at: https://localhost:7266/swagger
```

---

## 📚 **Next Steps**

1. **Auth Details:** [06_AUTHENTICATION_AUTHORIZATION.md](./06_AUTHENTICATION_AUTHORIZATION.md)
2. **User Workflows:** [07_USER_WORKFLOWS.md](./07_USER_WORKFLOWS.md)
3. **Testing:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)

---

**Document Status:** ✅ Complete  
**API Version:** v1  
**Last Updated:** January 13, 2026
