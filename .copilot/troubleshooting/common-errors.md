# Common Errors & Quick Fixes

## Quick Lookup Table

| Error Type | Symptom | Fix Location | Time to Fix |
|------------|---------|--------------|-------------|
| SchoolId Missing | 403 Forbidden | JWT claims | 5 min |
| Null Reference | 500 error | Add null check | 2 min |
| CORS Error | Frontend can't call API | Program.cs | 3 min |
| Migration Failed | Database out of sync | EF migrations | 10 min |
| JWT Expired | 401 Unauthorized | Refresh token | 1 min |
| SignalR Not Connecting | Chat not working | CORS + WebSocket | 5 min |
| Cross-School Access | Wrong data returned | Repository filter | 10 min |

---

## 🔴 P0: Critical Security Errors

### Error: Cross-School Data Leakage
**Symptom:** User sees data from other schools

**Root Cause:** Missing SchoolId filter in query

**Fix:**
```csharp
// ❌ WRONG
var students = await _context.Students.ToListAsync();

// ✅ CORRECT
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync();
```

**Prevention:** Always use BaseSchoolController and repository pattern

**Test:**
```bash
# Test with Postman
# Login as School A admin, try to GET School B's student by ID
# Should return 404 or 403, never the data
```

---

### Error: SchoolId Claim Missing
**Symptom:** 403 Forbidden on all endpoints

**Root Cause:** JWT token doesn't include SchoolId claim

**Fix Location:** `AuthService.cs` (or `AuthenticationController.cs`)

**Fix:**
```csharp
// In GenerateJwtToken method
if (user.SchoolId != Guid.Empty)
{
    authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
}
```

**Verification:**
```bash
# Decode JWT token at jwt.io
# Look for "SchoolId" in payload
```

---

## 🟡 P1: Common Runtime Errors

### Error: NullReferenceException
**Symptom:** 500 Internal Server Error

**Typical Location:** Accessing properties without null check

**Fix Pattern:**
```csharp
// ❌ WRONG
var studentName = student.FirstName; // Crash if student is null

// ✅ CORRECT
if (student == null)
{
    return NotFound($"Student {id} not found");
}
var studentName = student.FirstName;

// ✅ BETTER (Null-conditional operator)
var studentName = student?.FirstName ?? "Unknown";
```

**Common Scenarios:**
1. User not found: `await _userManager.FindByIdAsync(userId)`
2. Entity not found: `await _repository.GetByIdAsync(id, schoolId)`
3. Claim not found: `User.FindFirst("SchoolId")`

---

### Error: Async Method Not Awaited
**Symptom:** Warning CS4014 or unexpected behavior

**Fix:**
```csharp
// ❌ WRONG
var students = _repository.GetAllAsync(schoolId); // Missing await

// ✅ CORRECT
var students = await _repository.GetAllAsync(schoolId);
```

**Rule:** Every method that returns `Task<T>` must be awaited

---

### Error: InvalidOperationException - Sequence contains no elements
**Symptom:** Crash when using `.First()` or `.Single()`

**Fix:**
```csharp
// ❌ WRONG (crashes if no results)
var student = await _context.Students.FirstAsync(s => s.Id == id);

// ✅ CORRECT
var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
if (student == null)
{
    return NotFound();
}
```

---

## 🟢 P2: Build & Compilation Errors

### Error: Missing Namespace
**Symptom:** "The type or namespace name 'X' could not be found"

**Fix:**
```csharp
// Add missing using statements
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
```

**Quick Command:**
- Visual Studio: Ctrl + . (Quick Actions)
- VS Code: Ctrl + . → Add using statement

---

### Error: Cannot Convert Type
**Symptom:** "Cannot implicitly convert type 'Task<Student>' to 'Student'"

**Root Cause:** Missing `await`

**Fix:**
```csharp
// ❌ WRONG
Student student = _repository.GetByIdAsync(id, schoolId);

// ✅ CORRECT
Student student = await _repository.GetByIdAsync(id, schoolId);
```

---

## 🔵 P3: Database Errors

### Error: Migration Pending
**Symptom:** "The migration 'X' has not been applied"

**Fix:**
```bash
# Check pending migrations
dotnet ef migrations list --project Backend/SMSDataContext

# Apply migrations
dotnet ef database update --project Backend/SMSDataContext --startup-project Backend/SMSPrototype1
```

**Alternative (if migrations are messed up):**
```bash
# Remove last migration
dotnet ef migrations remove --project Backend/SMSDataContext

# Re-create migration
dotnet ef migrations add MigrationName --project Backend/SMSDataContext
```

---

### Error: Foreign Key Constraint Violation
**Symptom:** "The DELETE statement conflicted with the REFERENCE constraint"

**Root Cause:** Trying to delete parent record with child records

**Fix Options:**
1. **Delete children first:**
```csharp
// Delete all student's grades before deleting student
var grades = await _context.Grades.Where(g => g.StudentId == studentId).ToListAsync();
_context.Grades.RemoveRange(grades);
await _context.SaveChangesAsync();

// Now safe to delete student
_context.Students.Remove(student);
await _context.SaveChangesAsync();
```

2. **Use CASCADE DELETE:**
```csharp
// In DbContext OnModelCreating
modelBuilder.Entity<Grade>()
    .HasOne(g => g.Student)
    .WithMany(s => s.Grades)
    .OnDelete(DeleteBehavior.Cascade); // Auto-delete grades when student deleted
```

---

### Error: Duplicate Key Violation
**Symptom:** "Cannot insert duplicate key in object 'dbo.Students'"

**Root Cause:** Trying to insert record with existing primary key

**Fix:**
```csharp
// Always use Guid.NewGuid() for new records
var student = new Student
{
    Id = Guid.NewGuid(), // Generate new unique ID
    SchoolId = schoolId,
    // ... other properties
};
```

---

## 🟣 P4: Authentication & Authorization Errors

### Error: 401 Unauthorized
**Symptom:** API returns 401 even with valid token

**Checklist:**
1. **Token expired?**
   - Check `exp` claim in JWT (decode at jwt.io)
   - Solution: Refresh token

2. **Token not sent?**
   - Check `Authorization` header: `Bearer <token>`
   - Solution: Add header in frontend

3. **Token signature invalid?**
   - Check JWT secret matches in appsettings.json
   - Solution: Regenerate token with correct secret

**Frontend Fix:**
```typescript
// Ensure token is sent with every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

---

### Error: 403 Forbidden
**Symptom:** Authenticated but not authorized

**Root Causes:**
1. **Missing role:**
```csharp
// User doesn't have required role
[Authorize(Roles = "SchoolAdmin")] // User is "Teacher"
```

2. **Missing SchoolId claim:**
```csharp
// Check middleware logs
// Fix: Ensure SchoolId added during login
```

3. **Policy not met:**
```csharp
// User doesn't satisfy custom policy
[Authorize(Policy = "RequireSchoolStaff")]
```

**Debug Steps:**
```csharp
// In controller, check user claims
var claims = User.Claims.Select(c => new { c.Type, c.Value });
_logger.LogInformation("User claims: {@Claims}", claims);
```

---

## 🟠 P5: Frontend Errors

### Error: CORS Policy Blocked
**Symptom:** "Access to fetch at 'https://localhost:7266' from origin 'http://localhost:5173' has been blocked by CORS policy"

**Fix Location:** `Backend/SMSPrototype1/Program.cs`

**Fix:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Add frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// IMPORTANT: Must be before app.UseAuthorization()
app.UseCors("AllowFrontend");
```

---

### Error: SignalR Connection Failed
**Symptom:** "WebSocket connection to 'wss://localhost:7266/chathub' failed"

**Fix 1: Enable WebSockets in CORS**
```csharp
policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials()
      .WithExposedHeaders("*"); // Add this
```

**Fix 2: Check SignalR Hub Registration**
```csharp
// In Program.cs
app.MapHub<ChatHub>("/chathub");
```

**Fix 3: Frontend Connection**
```typescript
const connection = new HubConnectionBuilder()
  .withUrl("https://localhost:7266/chathub", {
    accessTokenFactory: () => localStorage.getItem('accessToken') || '',
    transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling
  })
  .build();
```

---

## 🔧 Quick Diagnostic Commands

### Check Application Status
```powershell
# Backend running?
Test-NetConnection -ComputerName localhost -Port 7266

# Database accessible?
sqlcmd -S localhost -Q "SELECT @@VERSION"

# Frontend running?
Test-NetConnection -ComputerName localhost -Port 5173
```

### Check Logs
```powershell
# Backend logs
Get-Content Backend/SMSPrototype1/logs/log-*.txt -Tail 50

# Filter for errors
Get-Content Backend/SMSPrototype1/logs/log-*.txt | Select-String "Error|Exception"
```

### Database Diagnostics
```sql
-- Check SchoolId columns
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'SchoolId'
ORDER BY TABLE_NAME;

-- Check foreign keys
SELECT 
    fk.name AS ForeignKey,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables AS tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns AS cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
ORDER BY tp.name, fk.name;
```

---

## 📞 Emergency Recovery

### If Everything is Broken
```powershell
# 1. Stop all processes
Stop-Process -Name "dotnet" -Force
Stop-Process -Name "node" -Force

# 2. Clean build artifacts
cd Backend
dotnet clean
Remove-Item -Recurse -Force */bin, */obj

cd ../Frontend
Remove-Item -Recurse -Force node_modules

# 3. Restore packages
cd ../Backend
dotnet restore

cd ../Frontend
npm install # or bun install

# 4. Rebuild
cd ../Backend
dotnet build

cd ../Frontend
npm run build

# 5. Reset database (CAUTION: Data loss)
cd ../Backend/SMSDataContext
dotnet ef database drop --force
dotnet ef database update

# 6. Restart applications
cd ../SMSPrototype1
dotnet run

# In new terminal
cd ../../Frontend
npm run dev
```

---

## 🔍 Error Pattern Recognition

### If Error Contains "SchoolId"
→ Check: JWT claims, middleware, repository filters

### If Error Contains "Unauthorized" or "Forbidden"
→ Check: Authentication middleware order, JWT token, role claims

### If Error Contains "NullReferenceException"
→ Check: Null checks, FirstOrDefaultAsync vs FirstAsync

### If Error Contains "CORS"
→ Check: CORS policy configuration, middleware order

### If Error Contains "Migration"
→ Check: Pending migrations, database connection string

### If Error Contains "Foreign key constraint"
→ Check: Delete order (children first), cascade settings
