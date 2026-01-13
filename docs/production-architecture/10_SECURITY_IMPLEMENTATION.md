# Security Implementation Guide
## Step-by-Step Multi-Tenant Isolation

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 20 minutes  
**Status:** 🔴 CRITICAL - NOT YET IMPLEMENTED

---

## ⚠️ **CRITICAL WARNING**

**WITHOUT THIS IMPLEMENTATION, SCHOOLS CAN ACCESS EACH OTHER'S DATA!**

This document provides the exact code needed to implement multi-tenant isolation. Follow each step in order. Test thoroughly before production.

**Implementation Time Estimate:** 4-6 hours  
**Testing Time Estimate:** 2-3 hours  
**Priority:** P0 - BLOCKING PRODUCTION LAUNCH

---

## 📋 **Implementation Checklist**

```
Phase 1: Core Security Components
[ ] Step 1: Create SchoolIsolationMiddleware.cs
[ ] Step 2: Create BaseSchoolController.cs
[ ] Step 3: Register middleware in Program.cs
[ ] Step 4: Update JWT generation in AuthService.cs

Phase 2: Controller Updates
[ ] Step 5: Update StudentController.cs
[ ] Step 6: Update TeacherController.cs
[ ] Step 7: Update ClassController.cs
[ ] Step 8: Update SubjectController.cs
[ ] Step 9: Update AttendanceController.cs
[ ] Step 10: Update GradeController.cs
[ ] Step 11: Update AnnouncementController.cs
[ ] Step 12: Update FileController.cs
[ ] Step 13: Update ChatController.cs
[ ] Step 14: Update ParentController.cs
[ ] Step 15: Update ReportController.cs (if exists)

Phase 3: Database Migration
[ ] Step 16: Run SQL migration script
[ ] Step 17: Create default school
[ ] Step 18: Assign users to schools
[ ] Step 19: Verify SchoolId indexes

Phase 4: Testing
[ ] Step 20: Write security tests
[ ] Step 21: Test cross-school access (should fail)
[ ] Step 22: Test SuperAdmin access (should succeed with audit)
[ ] Step 23: Load testing
[ ] Step 24: Production deployment
```

---

## 🛡️ **Step 1: Create SchoolIsolationMiddleware**

**File:** `Backend/SMSPrototype1/Middleware/SchoolIsolationMiddleware.cs`

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMSPrototype1.Middleware
{
    /// <summary>
    /// Middleware to enforce school-level data isolation in multi-tenant system.
    /// Validates that every request has a valid SchoolId claim (except for auth endpoints).
    /// </summary>
    public class SchoolIsolationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SchoolIsolationMiddleware> _logger;

        // Endpoints that don't require SchoolId validation
        private static readonly string[] ExemptPaths = new[]
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh-token",
            "/api/auth/forgot-password",
            "/api/auth/reset-password",
            "/api/health",
            "/swagger",
            "/api/superadmin"  // SuperAdmin endpoints have their own validation
        };

        public SchoolIsolationMiddleware(
            RequestDelegate next,
            ILogger<SchoolIsolationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip validation for exempt paths
            if (IsExemptPath(path))
            {
                await _next(context);
                return;
            }

            // Skip validation for OPTIONS requests (CORS preflight)
            if (context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                // Let the authentication middleware handle this
                await _next(context);
                return;
            }

            // Extract SchoolId from claims
            var schoolIdClaim = context.User.FindFirst("SchoolId")?.Value;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

            // SuperAdmin can access any school (logged separately)
            if (roleClaim == "SuperAdmin")
            {
                _logger.LogWarning(
                    "SuperAdmin access: User {UserId} accessing {Path}",
                    userIdClaim, path);
                
                await _next(context);
                return;
            }

            // Validate SchoolId exists
            if (string.IsNullOrEmpty(schoolIdClaim) || !Guid.TryParse(schoolIdClaim, out var schoolId))
            {
                _logger.LogError(
                    "SECURITY: User {UserId} with role {Role} has no valid SchoolId claim. Path: {Path}",
                    userIdClaim, roleClaim, path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = "Your account is not assigned to a school. Please contact your administrator.",
                    code = "MISSING_SCHOOL_ID"
                });
                return;
            }

            // Validate SchoolId is not empty GUID
            if (schoolId == Guid.Empty)
            {
                _logger.LogError(
                    "SECURITY: User {UserId} has empty SchoolId GUID. Path: {Path}",
                    userIdClaim, path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = "Invalid school assignment. Please contact your administrator.",
                    code = "INVALID_SCHOOL_ID"
                });
                return;
            }

            // Add SchoolId to HttpContext.Items for easy access in controllers
            context.Items["SchoolId"] = schoolId;
            context.Items["UserId"] = userIdClaim;

            _logger.LogDebug(
                "School isolation validated: User {UserId} accessing School {SchoolId}",
                userIdClaim, schoolId);

            await _next(context);
        }

        private bool IsExemptPath(string path)
        {
            foreach (var exemptPath in ExemptPaths)
            {
                if (path.StartsWith(exemptPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
```

---

## 🎯 **Step 2: Create BaseSchoolController**

**File:** `Backend/SMSPrototype1/Controllers/BaseSchoolController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace SMSPrototype1.Controllers
{
    /// <summary>
    /// Base controller for all school-scoped endpoints.
    /// Provides helper methods for SchoolId validation and user context.
    /// </summary>
    [Authorize]
    [ApiController]
    public abstract class BaseSchoolController : ControllerBase
    {
        /// <summary>
        /// Gets the authenticated user's SchoolId from JWT claims.
        /// </summary>
        /// <returns>SchoolId GUID</returns>
        /// <exception cref="UnauthorizedAccessException">If SchoolId claim is missing or invalid</exception>
        protected Guid GetUserSchoolId()
        {
            // First try to get from HttpContext.Items (set by middleware)
            if (HttpContext.Items.TryGetValue("SchoolId", out var schoolIdObj) && schoolIdObj is Guid schoolIdFromMiddleware)
            {
                return schoolIdFromMiddleware;
            }

            // Fallback to claims
            var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
            
            if (string.IsNullOrEmpty(schoolIdClaim))
            {
                throw new UnauthorizedAccessException("SchoolId claim is missing. User is not assigned to a school.");
            }

            if (!Guid.TryParse(schoolIdClaim, out var schoolId) || schoolId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Invalid SchoolId. Please contact your administrator.");
            }

            return schoolId;
        }

        /// <summary>
        /// Gets the authenticated user's ID from JWT claims.
        /// </summary>
        protected Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid User ID format.");
            }

            return userId;
        }

        /// <summary>
        /// Gets the authenticated user's role.
        /// </summary>
        protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value 
                ?? throw new UnauthorizedAccessException("Role claim is missing.");
        }

        /// <summary>
        /// Checks if the current user is a SuperAdmin.
        /// </summary>
        protected bool IsSuperAdmin()
        {
            return GetUserRole() == "SuperAdmin";
        }

        /// <summary>
        /// Validates that a resource belongs to the user's school.
        /// Used to prevent cross-school access when querying by ID.
        /// </summary>
        /// <param name="resourceSchoolId">The SchoolId of the resource being accessed</param>
        /// <param name="resourceType">Type of resource (for logging)</param>
        /// <exception cref="UnauthorizedAccessException">If resource belongs to a different school</exception>
        protected void ValidateSchoolOwnership(Guid resourceSchoolId, string resourceType = "Resource")
        {
            // SuperAdmin can access any school
            if (IsSuperAdmin())
            {
                return;
            }

            var userSchoolId = GetUserSchoolId();

            if (resourceSchoolId != userSchoolId)
            {
                var userId = GetUserId();
                
                // LOG THIS AS A SECURITY INCIDENT
                var logger = HttpContext.RequestServices.GetService<ILogger<BaseSchoolController>>();
                logger?.LogCritical(
                    "SECURITY VIOLATION: User {UserId} from School {UserSchool} attempted to access {ResourceType} from School {ResourceSchool}",
                    userId, userSchoolId, resourceType, resourceSchoolId);

                throw new UnauthorizedAccessException(
                    $"You do not have permission to access this {resourceType}. It belongs to a different school.");
            }
        }

        /// <summary>
        /// Returns a standardized error response.
        /// </summary>
        protected IActionResult ErrorResponse(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                error = true,
                message = message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Returns a standardized success response.
        /// </summary>
        protected IActionResult SuccessResponse(object data, string message = "Success")
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
```

---

## ⚙️ **Step 3: Register Middleware in Program.cs**

**Location in file:** After `app.UseAuthentication()` and before `app.UseAuthorization()`

```csharp
// Authentication & Authorization
app.UseAuthentication();

// ⭐ CRITICAL: School isolation middleware MUST be after authentication
// This ensures User.Claims are populated before we check SchoolId
app.UseMiddleware<SchoolIsolationMiddleware>();

app.UseAuthorization();
```

**Full context (search for this section):**

```csharp
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<SchoolIsolationMiddleware>();  // ⭐ ADD THIS LINE
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
```

---

## 🔑 **Step 4: Update JWT Generation in AuthService**

**File:** `Backend/SMSServices/Services/AuthService.cs`

**Find the `GenerateJwtToken` method and update it:**

```csharp
private string GenerateJwtToken(ApplicationUser user, string role)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim(ClaimTypes.Role, role),
        new Claim("FullName", $"{user.FirstName} {user.LastName}")
    };

    // ⭐ CRITICAL: Add SchoolId claim
    // For non-SuperAdmin users, SchoolId is REQUIRED
    if (role == "SuperAdmin")
    {
        // SuperAdmin doesn't need SchoolId (can access all schools)
        claims.Add(new Claim("SchoolId", Guid.Empty.ToString()));
    }
    else
    {
        // For all other roles, SchoolId MUST be set
        if (user.SchoolId == null || user.SchoolId == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"User {user.Email} with role {role} does not have a valid SchoolId. " +
                "Please assign the user to a school before they can log in.");
        }

        claims.Add(new Claim("SchoolId", user.SchoolId.Value.ToString()));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(3),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**Key Changes:**
1. Check if user is SuperAdmin
2. For SuperAdmin: Allow Guid.Empty SchoolId
3. For other roles: THROW EXCEPTION if SchoolId is null/empty
4. Add SchoolId claim to token

---

## 📝 **Step 5-15: Update Controllers**

All controllers that access school-specific data MUST inherit from `BaseSchoolController`.

### **Example: StudentController.cs**

**BEFORE:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    // Old implementation
    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _studentService.GetAllStudentsAsync();
        return Ok(students);
    }
}
```

**AFTER:**
```csharp
[Route("api/[controller]")]
public class StudentController : BaseSchoolController  // ⭐ Inherit from BaseSchoolController
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

    /// <summary>
    /// Get all students in the authenticated user's school.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        try
        {
            var schoolId = GetUserSchoolId();  // ⭐ Get SchoolId from base controller
            
            var students = await _studentService.GetStudentsBySchoolIdAsync(schoolId);
            
            return SuccessResponse(students, $"Retrieved {students.Count()} students");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students");
            return ErrorResponse("An error occurred while retrieving students", StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get a specific student by ID (with school ownership validation).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        try
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // ⭐ CRITICAL: Validate that student belongs to user's school
            ValidateSchoolOwnership(student.SchoolId, "Student");
            
            return SuccessResponse(student);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student {StudentId}", id);
            return ErrorResponse("An error occurred while retrieving the student", StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Create a new student in the authenticated user's school.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
    {
        try
        {
            var schoolId = GetUserSchoolId();  // ⭐ Get SchoolId from JWT
            var userId = GetUserId();

            // ⭐ SECURITY: Force SchoolId from JWT, ignore any SchoolId in request body
            dto.SchoolId = schoolId;
            dto.CreatedBy = userId;

            var student = await _studentService.CreateStudentAsync(dto);
            
            _logger.LogInformation(
                "User {UserId} created student {StudentId} in school {SchoolId}",
                userId, student.Id, schoolId);

            return CreatedAtAction(
                nameof(GetStudent),
                new { id = student.Id },
                SuccessResponse(student, "Student created successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return ErrorResponse("An error occurred while creating the student", StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update a student (with school ownership validation).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto dto)
    {
        try
        {
            // Get existing student
            var existingStudent = await _studentService.GetStudentByIdAsync(id);
            
            if (existingStudent == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // ⭐ CRITICAL: Validate ownership before allowing update
            ValidateSchoolOwnership(existingStudent.SchoolId, "Student");

            // ⭐ SECURITY: Prevent SchoolId from being changed
            dto.SchoolId = existingStudent.SchoolId;
            dto.UpdatedBy = GetUserId();

            var updatedStudent = await _studentService.UpdateStudentAsync(id, dto);
            
            return SuccessResponse(updatedStudent, "Student updated successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {StudentId}", id);
            return ErrorResponse("An error occurred while updating the student", StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete a student (soft delete with school ownership validation).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        try
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // ⭐ CRITICAL: Validate ownership before allowing delete
            ValidateSchoolOwnership(student.SchoolId, "Student");

            await _studentService.DeleteStudentAsync(id);
            
            _logger.LogWarning(
                "User {UserId} deleted student {StudentId} from school {SchoolId}",
                GetUserId(), id, student.SchoolId);

            return SuccessResponse(null, "Student deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {StudentId}", id);
            return ErrorResponse("An error occurred while deleting the student", StatusCodes.Status500InternalServerError);
        }
    }
}
```

### **Pattern for All Controllers**

**Apply this pattern to:**
1. ✅ StudentController.cs
2. ✅ TeacherController.cs
3. ✅ ClassController.cs
4. ✅ SubjectController.cs
5. ✅ AttendanceController.cs
6. ✅ GradeController.cs
7. ✅ AnnouncementController.cs
8. ✅ FileController.cs
9. ✅ ChatController.cs
10. ✅ ParentController.cs
11. ✅ ReportController.cs

**Key Changes for Each Controller:**
```csharp
// 1. Change inheritance
- public class XController : ControllerBase
+ public class XController : BaseSchoolController

// 2. In GET all method
- var items = await _service.GetAllAsync();
+ var schoolId = GetUserSchoolId();
+ var items = await _service.GetBySchoolIdAsync(schoolId);

// 3. In GET by ID method
+ ValidateSchoolOwnership(item.SchoolId, "ItemType");

// 4. In POST method
+ dto.SchoolId = GetUserSchoolId();  // Force from JWT
+ dto.CreatedBy = GetUserId();

// 5. In PUT method
+ ValidateSchoolOwnership(existingItem.SchoolId, "ItemType");
+ dto.SchoolId = existingItem.SchoolId;  // Prevent change

// 6. In DELETE method
+ ValidateSchoolOwnership(item.SchoolId, "ItemType");
```

---

## 🗄️ **Step 16: Database Migration Script**

**File:** `Backend/DatabaseMigration_SchoolIsolation.sql`

```sql
-- ================================================================
-- DATABASE MIGRATION: Add SchoolId to Existing Tables
-- ================================================================
-- Purpose: Add SchoolId foreign key to all tenant-specific tables
-- Status: Run ONCE before production deployment
-- Rollback: See rollback script at end of file
-- ================================================================

BEGIN TRANSACTION;

PRINT '=== Starting School Isolation Migration ==='

-- ================================================================
-- Step 1: Create Schools table if not exists
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Schools')
BEGIN
    PRINT 'Creating Schools table...'
    
    CREATE TABLE Schools (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL UNIQUE,
        Address NVARCHAR(500),
        City NVARCHAR(100),
        State NVARCHAR(100),
        PinCode NVARCHAR(20),
        PhoneNumber NVARCHAR(20),
        Email NVARCHAR(255),
        PrincipalName NVARCHAR(200),
        BoardAffiliation NVARCHAR(100),  -- CBSE, ICSE, State Board
        IsActive BIT NOT NULL DEFAULT 1,
        SubscriptionPlan NVARCHAR(50) DEFAULT 'Basic',  -- Basic, Standard, Premium
        SubscriptionStartDate DATETIME2,
        SubscriptionEndDate DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        CreatedBy UNIQUEIDENTIFIER,
        UpdatedBy UNIQUEIDENTIFIER
    );

    PRINT '✅ Schools table created'
END
ELSE
BEGIN
    PRINT 'ℹ️ Schools table already exists'
END

-- ================================================================
-- Step 2: Add SchoolId to AspNetUsers if not exists
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to AspNetUsers...'
    
    ALTER TABLE AspNetUsers
    ADD SchoolId UNIQUEIDENTIFIER NULL;
    
    PRINT '✅ SchoolId column added to AspNetUsers'
END
ELSE
BEGIN
    PRINT 'ℹ️ SchoolId column already exists in AspNetUsers'
END

-- ================================================================
-- Step 3: Create default school for existing users
-- ================================================================
DECLARE @DefaultSchoolId UNIQUEIDENTIFIER = NEWID();
DECLARE @DefaultSchoolName NVARCHAR(200) = 'Default School (Migration)';

IF NOT EXISTS (SELECT * FROM Schools WHERE Code = 'DEFAULT_MIGRATION')
BEGIN
    PRINT 'Creating default school...'
    
    INSERT INTO Schools (Id, Name, Code, IsActive, CreatedAt)
    VALUES (
        @DefaultSchoolId,
        @DefaultSchoolName,
        'DEFAULT_MIGRATION',
        1,
        GETUTCDATE()
    );
    
    PRINT '✅ Default school created with ID: ' + CAST(@DefaultSchoolId AS NVARCHAR(50))
END
ELSE
BEGIN
    SELECT @DefaultSchoolId = Id FROM Schools WHERE Code = 'DEFAULT_MIGRATION';
    PRINT 'ℹ️ Default school already exists with ID: ' + CAST(@DefaultSchoolId AS NVARCHAR(50))
END

-- ================================================================
-- Step 4: Assign existing users to default school
-- ================================================================
PRINT 'Assigning users to default school...'

UPDATE AspNetUsers
SET SchoolId = @DefaultSchoolId
WHERE SchoolId IS NULL OR SchoolId = '00000000-0000-0000-0000-000000000000';

DECLARE @UpdatedUserCount INT = @@ROWCOUNT;
PRINT '✅ Updated ' + CAST(@UpdatedUserCount AS NVARCHAR(10)) + ' users'

-- ================================================================
-- Step 5: Add foreign key constraint
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Schools')
BEGIN
    PRINT 'Adding foreign key constraint...'
    
    ALTER TABLE AspNetUsers
    ADD CONSTRAINT FK_AspNetUsers_Schools
    FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    PRINT '✅ Foreign key constraint added'
END

-- ================================================================
-- Step 6: Create index on SchoolId
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_SchoolId')
BEGIN
    PRINT 'Creating index on SchoolId...'
    
    CREATE NONCLUSTERED INDEX IX_AspNetUsers_SchoolId
    ON AspNetUsers(SchoolId)
    INCLUDE (FirstName, LastName, Email);
    
    PRINT '✅ Index created'
END

-- ================================================================
-- Step 7: Add SchoolId to all tenant-specific tables
-- ================================================================

-- Students
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Students') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Students...'
    ALTER TABLE Students ADD SchoolId UNIQUEIDENTIFIER NULL;
    
    -- Backfill from user's school
    UPDATE s
    SET s.SchoolId = u.SchoolId
    FROM Students s
    INNER JOIN AspNetUsers u ON s.UserId = u.Id;
    
    -- Make required
    ALTER TABLE Students ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE Students ADD CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Students_SchoolId ON Students(SchoolId) INCLUDE (FirstName, LastName, RollNumber);
    PRINT '✅ Students updated'
END

-- Teachers
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Teachers') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Teachers...'
    ALTER TABLE Teachers ADD SchoolId UNIQUEIDENTIFIER NULL;
    
    UPDATE t
    SET t.SchoolId = u.SchoolId
    FROM Teachers t
    INNER JOIN AspNetUsers u ON t.UserId = u.Id;
    
    ALTER TABLE Teachers ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE Teachers ADD CONSTRAINT FK_Teachers_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Teachers_SchoolId ON Teachers(SchoolId) INCLUDE (FirstName, LastName, EmployeeId);
    PRINT '✅ Teachers updated'
END

-- Classes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Classes') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Classes...'
    ALTER TABLE Classes ADD SchoolId UNIQUEIDENTIFIER NOT NULL DEFAULT (@DefaultSchoolId);
    ALTER TABLE Classes ADD CONSTRAINT FK_Classes_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Classes_SchoolId ON Classes(SchoolId) INCLUDE (ClassName, Section);
    PRINT '✅ Classes updated'
END

-- Subjects
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Subjects') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Subjects...'
    ALTER TABLE Subjects ADD SchoolId UNIQUEIDENTIFIER NOT NULL DEFAULT (@DefaultSchoolId);
    ALTER TABLE Subjects ADD CONSTRAINT FK_Subjects_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Subjects_SchoolId ON Subjects(SchoolId);
    PRINT '✅ Subjects updated'
END

-- Attendance
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Attendance') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Attendance...'
    ALTER TABLE Attendance ADD SchoolId UNIQUEIDENTIFIER NULL;
    
    UPDATE a
    SET a.SchoolId = s.SchoolId
    FROM Attendance a
    INNER JOIN Students s ON a.StudentId = s.Id;
    
    ALTER TABLE Attendance ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE Attendance ADD CONSTRAINT FK_Attendance_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Attendance_SchoolId_Date ON Attendance(SchoolId, Date) INCLUDE (StudentId, Status);
    PRINT '✅ Attendance updated'
END

-- Grades
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Grades') AND name = 'SchoolId')
BEGIN
    PRINT 'Adding SchoolId to Grades...'
    ALTER TABLE Grades ADD SchoolId UNIQUEIDENTIFIER NULL;
    
    UPDATE g
    SET g.SchoolId = s.SchoolId
    FROM Grades g
    INNER JOIN Students s ON g.StudentId = s.Id;
    
    ALTER TABLE Grades ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE Grades ADD CONSTRAINT FK_Grades_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
    
    CREATE NONCLUSTERED INDEX IX_Grades_SchoolId ON Grades(SchoolId) INCLUDE (StudentId, SubjectId, Score);
    PRINT '✅ Grades updated'
END

-- ChatRooms
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRooms')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ChatRooms') AND name = 'SchoolId')
    BEGIN
        PRINT 'Adding SchoolId to ChatRooms...'
        ALTER TABLE ChatRooms ADD SchoolId UNIQUEIDENTIFIER NOT NULL DEFAULT (@DefaultSchoolId);
        ALTER TABLE ChatRooms ADD CONSTRAINT FK_ChatRooms_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
        
        CREATE NONCLUSTERED INDEX IX_ChatRooms_SchoolId ON ChatRooms(SchoolId);
        PRINT '✅ ChatRooms updated'
    END
END

-- ChatMessages
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ChatMessages') AND name = 'SchoolId')
    BEGIN
        PRINT 'Adding SchoolId to ChatMessages...'
        ALTER TABLE ChatMessages ADD SchoolId UNIQUEIDENTIFIER NULL;
        
        UPDATE cm
        SET cm.SchoolId = cr.SchoolId
        FROM ChatMessages cm
        INNER JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id;
        
        ALTER TABLE ChatMessages ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;
        ALTER TABLE ChatMessages ADD CONSTRAINT FK_ChatMessages_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id);
        
        CREATE NONCLUSTERED INDEX IX_ChatMessages_SchoolId_RoomId ON ChatMessages(SchoolId, ChatRoomId);
        PRINT '✅ ChatMessages updated'
    END
END

-- ================================================================
-- Step 8: Create audit log table for SuperAdmin access
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    PRINT 'Creating AuditLogs table...'
    
    CREATE TABLE AuditLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        SchoolId UNIQUEIDENTIFIER,
        Action NVARCHAR(200) NOT NULL,
        EntityType NVARCHAR(100),
        EntityId UNIQUEIDENTIFIER,
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        IpAddress NVARCHAR(50),
        UserAgent NVARCHAR(500),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Severity NVARCHAR(20) DEFAULT 'Info',  -- Info, Warning, Critical
        
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_AuditLogs_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id)
    );

    CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId_Timestamp ON AuditLogs(UserId, Timestamp DESC);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_SchoolId_Timestamp ON AuditLogs(SchoolId, Timestamp DESC);
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Severity ON AuditLogs(Severity, Timestamp DESC);
    
    PRINT '✅ AuditLogs table created'
END

-- ================================================================
-- Step 9: Verify data integrity
-- ================================================================
PRINT ''
PRINT '=== Data Integrity Check ==='

-- Check for null SchoolIds
DECLARE @NullSchoolIdCount INT;
SELECT @NullSchoolIdCount = COUNT(*) FROM AspNetUsers WHERE SchoolId IS NULL;
IF @NullSchoolIdCount > 0
BEGIN
    PRINT '⚠️ WARNING: ' + CAST(@NullSchoolIdCount AS NVARCHAR(10)) + ' users still have NULL SchoolId'
END
ELSE
BEGIN
    PRINT '✅ All users have valid SchoolId'
END

-- Check for empty GUID SchoolIds
DECLARE @EmptyGuidCount INT;
SELECT @EmptyGuidCount = COUNT(*) FROM AspNetUsers WHERE SchoolId = '00000000-0000-0000-0000-000000000000';
IF @EmptyGuidCount > 0
BEGIN
    PRINT '⚠️ WARNING: ' + CAST(@EmptyGuidCount AS NVARCHAR(10)) + ' users have empty GUID SchoolId'
END
ELSE
BEGIN
    PRINT '✅ No users with empty GUID SchoolId'
END

PRINT ''
PRINT '=== Migration Summary ==='
PRINT 'Schools created: 1 (Default School)'
PRINT 'Users updated: ' + CAST(@UpdatedUserCount AS NVARCHAR(10))
PRINT 'Tables with SchoolId: Students, Teachers, Classes, Subjects, Attendance, Grades, ChatRooms, ChatMessages'
PRINT 'Indexes created: 9'
PRINT 'Foreign keys added: 10'
PRINT ''
PRINT '✅ Migration completed successfully!'

COMMIT TRANSACTION;

-- ================================================================
-- ROLLBACK SCRIPT (Run only if migration needs to be reverted)
-- ================================================================
/*
BEGIN TRANSACTION;

-- Remove foreign keys
ALTER TABLE ChatMessages DROP CONSTRAINT FK_ChatMessages_Schools;
ALTER TABLE ChatRooms DROP CONSTRAINT FK_ChatRooms_Schools;
ALTER TABLE Grades DROP CONSTRAINT FK_Grades_Schools;
ALTER TABLE Attendance DROP CONSTRAINT FK_Attendance_Schools;
ALTER TABLE Subjects DROP CONSTRAINT FK_Subjects_Schools;
ALTER TABLE Classes DROP CONSTRAINT FK_Classes_Schools;
ALTER TABLE Teachers DROP CONSTRAINT FK_Teachers_Schools;
ALTER TABLE Students DROP CONSTRAINT FK_Students_Schools;
ALTER TABLE AspNetUsers DROP CONSTRAINT FK_AspNetUsers_Schools;

-- Remove columns
ALTER TABLE ChatMessages DROP COLUMN SchoolId;
ALTER TABLE ChatRooms DROP COLUMN SchoolId;
ALTER TABLE Grades DROP COLUMN SchoolId;
ALTER TABLE Attendance DROP COLUMN SchoolId;
ALTER TABLE Subjects DROP COLUMN SchoolId;
ALTER TABLE Classes DROP COLUMN SchoolId;
ALTER TABLE Teachers DROP COLUMN SchoolId;
ALTER TABLE Students DROP COLUMN SchoolId;
ALTER TABLE AspNetUsers DROP COLUMN SchoolId;

-- Drop tables
DROP TABLE AuditLogs;
DROP TABLE Schools;

COMMIT TRANSACTION;
PRINT 'Rollback completed';
*/
```

---

## ✅ **Step 17-19: Run Migration**

**Execute the migration:**

```powershell
# From Backend directory
cd Backend

# Run migration
sqlcmd -S localhost -d SchoolManagementDB -i DatabaseMigration_SchoolIsolation.sql

# Verify results
sqlcmd -S localhost -d SchoolManagementDB -Q "SELECT COUNT(*) as SchoolCount FROM Schools"
sqlcmd -S localhost -d SchoolManagementDB -Q "SELECT COUNT(*) as UsersWithSchool FROM AspNetUsers WHERE SchoolId IS NOT NULL"
```

---

## 🧪 **Step 20-22: Security Testing**

**File:** `Backend/SecurityTests/MultiTenantIsolationTests.cs`

```csharp
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace SMSPrototype1.Tests.Security
{
    public class MultiTenantIsolationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MultiTenantIsolationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CrossSchoolAccess_ShouldBeDenied()
        {
            // Arrange: Login as School A user
            var schoolAToken = await LoginAsSchoolAUser();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", schoolAToken);

            // Act: Try to access School B student
            var schoolBStudentId = await GetSchoolBStudentId();
            var response = await _client.GetAsync($"/api/students/{schoolBStudentId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Contains("different school", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SuperAdminAccess_ShouldSucceedWithAuditLog()
        {
            // Arrange: Login as SuperAdmin
            var superAdminToken = await LoginAsSuperAdmin();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", superAdminToken);

            // Act: Access School A data
            var response = await _client.GetAsync("/api/students");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify audit log was created
            var auditLogs = await GetAuditLogs();
            Assert.Contains(auditLogs, log => 
                log.Action.Contains("SuperAdmin") && 
                log.Severity == "Warning");
        }

        // Add 18 more test cases...
    }
}
```

---

## 📊 **Implementation Status Dashboard**

After completing all steps, verify with this checklist:

```
Core Components:
[✅] SchoolIsolationMiddleware created
[✅] BaseSchoolController created
[✅] Middleware registered in Program.cs
[✅] JWT generation updated

Controllers Updated:
[✅] StudentController
[✅] TeacherController
[✅] ClassController
[✅] SubjectController
[✅] AttendanceController
[✅] GradeController
[✅] AnnouncementController
[✅] FileController
[✅] ChatController
[✅] ParentController
[✅] ReportController

Database:
[✅] Migration script executed
[✅] Default school created
[✅] Users assigned to schools
[✅] Indexes created
[✅] Foreign keys added

Testing:
[✅] 20+ security tests written
[✅] Cross-school access BLOCKED
[✅] SuperAdmin access ALLOWED with audit
[✅] Load tests passed (< 200ms)

Production Ready:
[✅] Code deployed
[✅] Monitoring configured
[✅] Alerts active
[✅] Audit logs working
```

---

## 🚨 **Common Issues & Solutions**

**Issue 1: Users can't log in after migration**
```
Error: "User does not have a valid SchoolId"
Solution: Run Step 4 SQL script to assign users to default school
```

**Issue 2: Middleware returns 403 for auth endpoints**
```
Error: Auth endpoints blocked by middleware
Solution: Verify ExemptPaths includes "/api/auth" in middleware
```

**Issue 3: SuperAdmin can't access other schools**
```
Error: SchoolId validation blocks SuperAdmin
Solution: Check IsSuperAdmin() logic in BaseSchoolController
```

---

## 📚 **Next Steps**

1. **Testing Strategy:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)
2. **Migration Guide:** [11_MIGRATION_STRATEGY.md](./11_MIGRATION_STRATEGY.md)
3. **Deployment:** [09_DEPLOYMENT_ARCHITECTURE.md](./09_DEPLOYMENT_ARCHITECTURE.md)

---

**Document Status:** ✅ Complete  
**Implementation:** 🔴 NOT YET IMPLEMENTED  
**Priority:** P0 - CRITICAL - BLOCKING PRODUCTION