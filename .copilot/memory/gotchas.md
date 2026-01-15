# Gotchas - Common Mistakes & Quick Fixes
**Purpose:** Document mistakes we've made and how to avoid/fix them quickly

**Updated:** January 15, 2026  
**Rule:** Add entry immediately after fixing a gotcha (don't forget!)

---

## 🚨 Critical Gotchas (Security/Data Loss)

### Gotcha #1: Forgetting SchoolId Filter
**Severity:** 🔴 CRITICAL (Data Leak)  
**Symptom:** Users can see data from other schools  
**Mistake:**
```csharp
// ❌ WRONG - No SchoolId filter
var students = await _context.Students
    .Where(s => s.IsActive == true)
    .ToListAsync();
```

**Fix:**
```csharp
// ✅ CORRECT - Always filter by SchoolId
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId && s.IsActive == true)
    .ToListAsync();
```

**Prevention:**
- Use BaseSchoolController for all controllers
- Always call `GetSchoolIdFromClaims()` at start of action
- Repository methods must require `Guid schoolId` parameter
- Write cross-school access tests for every endpoint

**How to Find:** Search codebase for `_context.` without `.Where(s => s.SchoolId`

---

### Gotcha #2: Accepting SchoolId from Request Body
**Severity:** 🔴 CRITICAL (Privilege Escalation)  
**Symptom:** Attacker can create data in other schools  
**Mistake:**
```csharp
public class CreateStudentDto
{
    public Guid SchoolId { get; set; } // ❌ Attacker can set this!
    public string FirstName { get; set; }
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var student = new Student
    {
        SchoolId = dto.SchoolId, // ❌ Using attacker's value!
        FirstName = dto.FirstName
    };
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    return Ok(student);
}
```

**Fix:**
```csharp
public class CreateStudentDto
{
    // ✅ NO SchoolId property!
    public string FirstName { get; set; }
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var schoolId = GetSchoolIdFromClaims(); // ✅ From JWT, not request!
    if (schoolId == Guid.Empty)
        return ForbidSchoolAccess();

    var student = new Student
    {
        SchoolId = schoolId, // ✅ From authenticated user
        FirstName = dto.FirstName
    };
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
}
```

**Prevention:**
- NEVER add SchoolId property to Create/Update DTOs
- Always get SchoolId from JWT claims
- Code review checklist: Search for `SchoolId` in DTO classes

---

### Gotcha #3: Not Checking SchoolId on Update/Delete
**Severity:** 🔴 CRITICAL (Data Modification Across Tenants)  
**Symptom:** User can update/delete entities from other schools  
**Mistake:**
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
{
    var student = await _context.Students.FindAsync(id); // ❌ No SchoolId check
    if (student == null)
        return NotFound();

    student.FirstName = dto.FirstName; // Updating wrong school's student!
    await _context.SaveChangesAsync();
    return NoContent();
}
```

**Fix:**
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
{
    var schoolId = GetSchoolIdFromClaims();
    if (schoolId == Guid.Empty)
        return ForbidSchoolAccess();

    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId); // ✅ Both checks
    
    if (student == null)
        return NotFound(); // Either doesn't exist OR different school

    student.FirstName = dto.FirstName;
    student.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return NoContent();
}
```

**Prevention:**
- Use FirstOrDefaultAsync with SchoolId filter (not FindAsync)
- Repository Update/Delete methods must verify SchoolId
- Write security tests for cross-school update/delete attempts

---

## ⚠️ High-Priority Gotchas (Performance/Bugs)

### Gotcha #4: N+1 Query Problem
**Severity:** 🟠 HIGH (Performance)  
**Symptom:** Queries become slow as data grows  
**Mistake:**
```csharp
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync();

foreach (var student in students) // ❌ N+1: 1 query + N queries
{
    var className = _context.Classes
        .Where(c => c.Id == student.ClassId)
        .Select(c => c.Name)
        .FirstOrDefault(); // Database query inside loop!
}
```

**Fix:**
```csharp
// Option 1: Eager loading
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .Include(s => s.Class) // ✅ Load classes in one query
    .ToListAsync();

foreach (var student in students)
{
    var className = student.Class.Name; // No query, already loaded
}

// Option 2: Projection (better for read-only)
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .Select(s => new StudentWithClassDto
    {
        Id = s.Id,
        FirstName = s.FirstName,
        ClassName = s.Class.Name // ✅ Joined in one query
    })
    .AsNoTracking()
    .ToListAsync();
```

**Detection:**
- Enable EF Core logging: `builder.Services.AddDbContext<ApplicationDbContext>(options => options.EnableSensitiveDataLogging().LogTo(Console.WriteLine));`
- Look for multiple SELECT queries in console
- Use SQL Profiler to see query count

**Prevention:**
- Use `.Include()` for navigation properties
- Use `.Select()` for read-only projections
- Never query database inside a loop

---

### Gotcha #5: Forgetting AsNoTracking
**Severity:** 🟡 MEDIUM (Performance)  
**Symptom:** Queries 30-40% slower than necessary  
**Mistake:**
```csharp
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var students = await _context.Students
        .Where(s => s.SchoolId == schoolId)
        .ToListAsync(); // ❌ Change tracking enabled (unnecessary overhead)
    return Ok(students);
}
```

**Fix:**
```csharp
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var students = await _context.Students
        .Where(s => s.SchoolId == schoolId)
        .AsNoTracking() // ✅ 30-40% faster for read-only
        .ToListAsync();
    return Ok(students);
}
```

**When to Use:**
- ✅ Use AsNoTracking: GET endpoints (read-only)
- ❌ Don't use: POST/PUT/DELETE (need change tracking)

**Prevention:**
- Code review: All GET repository methods should have AsNoTracking
- Create helper extension: `GetAllReadOnlyAsync()` that includes AsNoTracking

---

### Gotcha #6: Using .Result or .Wait() (Deadlock Risk)
**Severity:** 🟠 HIGH (Deadlock)  
**Symptom:** Application hangs randomly  
**Mistake:**
```csharp
[HttpGet("{id}")]
public IActionResult GetById(Guid id) // ❌ Not async
{
    var student = _service.GetByIdAsync(id, schoolId).Result; // ❌ Blocking!
    return Ok(student);
}
```

**Fix:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id) // ✅ Async
{
    var schoolId = GetSchoolIdFromClaims();
    if (schoolId == Guid.Empty)
        return ForbidSchoolAccess();

    var student = await _service.GetByIdAsync(id, schoolId); // ✅ Await
    if (student == null)
        return NotFound();
    
    return Ok(student);
}
```

**Why Deadlock Happens:**
- ASP.NET Core uses thread pool
- `.Result` blocks thread waiting for async operation
- Async operation needs that thread to complete
- Result: Deadlock (thread waiting for itself)

**Prevention:**
- Rule: async/await all the way (never mix sync/async)
- Search codebase for `.Result` and `.Wait()` → remove all
- Use analyzer: `Microsoft.VisualStudio.Threading.Analyzers` NuGet package

---

## 🟡 Medium-Priority Gotchas (Bugs)

### Gotcha #7: Forgetting [Authorize] Attribute
**Severity:** 🟡 MEDIUM (Security)  
**Symptom:** Anonymous users can access protected endpoints  
**Mistake:**
```csharp
[HttpGet("{id}")] // ❌ No [Authorize]!
public async Task<IActionResult> GetById(Guid id)
{
    // Anyone can call this!
}
```

**Fix:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // ✅ At class level = all actions require auth
public class StudentController : BaseSchoolController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Only authenticated users can access
    }
}
```

**Prevention:**
- Add `[Authorize]` at controller class level (applies to all actions)
- For public endpoints: Use `[AllowAnonymous]` on specific actions
- Security test: Try calling endpoint without token (should return 401)

---

### Gotcha #8: Not Validating Foreign Keys Belong to Same School
**Severity:** 🟡 MEDIUM (Data Integrity)  
**Symptom:** Student assigned to class from different school  
**Mistake:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var schoolId = GetSchoolIdFromClaims();
    
    // ❌ Not checking if dto.ClassId belongs to user's school!
    var student = new Student
    {
        SchoolId = schoolId,
        ClassId = dto.ClassId, // Could be from different school!
        FirstName = dto.FirstName
    };
    
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    return Ok(student);
}
```

**Fix:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var schoolId = GetSchoolIdFromClaims();
    if (schoolId == Guid.Empty)
        return ForbidSchoolAccess();

    // ✅ Verify class belongs to user's school
    var classExists = await _context.Classes
        .AnyAsync(c => c.Id == dto.ClassId && c.SchoolId == schoolId);
    
    if (!classExists)
        return BadRequest(new { error = "Invalid ClassId or class does not belong to your school" });

    var student = new Student
    {
        SchoolId = schoolId,
        ClassId = dto.ClassId,
        FirstName = dto.FirstName
    };
    
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
}
```

**Prevention:**
- Validate all foreign key references in service layer
- Add FluentValidation rule for FK validation
- Database check constraint (advanced): `CK_Students_ClassId_SchoolId_Match`

---

### Gotcha #9: Exposing Entity Models Directly (Not Using DTOs)
**Severity:** 🟡 MEDIUM (Security/Over-posting)  
**Symptom:** Sensitive data exposed, mass assignment vulnerabilities  
**Mistake:**
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var student = await _context.Students.FindAsync(id);
    return Ok(student); // ❌ Exposing entity model!
    // Includes: PasswordHash, InternalNotes, etc.
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] Student student)
{
    // ❌ Attacker can set Id, CreatedAt, IsActive, etc.
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    return Ok(student);
}
```

**Fix:**
```csharp
// Define DTOs
public class StudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    // Only safe properties
}

public class CreateStudentDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    // Only properties user can set
}

[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var student = await _context.Students.FindAsync(id);
    var dto = _mapper.Map<StudentDto>(student); // ✅ Use DTO
    return Ok(dto);
}

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var student = _mapper.Map<Student>(dto); // ✅ Map from DTO
    student.SchoolId = GetSchoolIdFromClaims();
    student.CreatedAt = DateTime.UtcNow;
    
    await _context.AddAsync(student);
    await _context.SaveChangesAsync();
    
    var responseDto = _mapper.Map<StudentDto>(student);
    return CreatedAtAction(nameof(GetById), new { id = student.Id }, responseDto);
}
```

**Prevention:**
- Never accept entity models in controller parameters
- Always return DTOs from controllers
- Use AutoMapper for entity ↔ DTO conversion

---

## 🟢 Low-Priority Gotchas (Code Quality)

### Gotcha #10: Not Using UTC for DateTime
**Severity:** 🟢 LOW (Timezone Issues)  
**Symptom:** Dates/times off by hours depending on server location  
**Mistake:**
```csharp
student.CreatedAt = DateTime.Now; // ❌ Local time (varies by server)
```

**Fix:**
```csharp
student.CreatedAt = DateTime.UtcNow; // ✅ UTC (consistent everywhere)
```

**Prevention:**
- Always use `DateTime.UtcNow` (never `DateTime.Now`)
- Store all dates in UTC in database
- Convert to local time only in frontend

---

### Gotcha #11: Missing Migration Down Method
**Severity:** 🟢 LOW (Deployment Rollback)  
**Symptom:** Can't rollback database changes  
**Mistake:**
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // ❌ Empty! Can't rollback
}
```

**Fix:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<Guid>(
        name: "SchoolId",
        table: "Students",
        nullable: false);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ Reverse the Up operations
    migrationBuilder.DropColumn(
        name: "SchoolId",
        table: "Students");
}
```

**Prevention:**
- Always implement Down() method
- Test rollback: `dotnet ef migrations remove`

---

### Gotcha #12: Hardcoding Connection Strings
**Severity:** 🟢 LOW (Configuration)  
**Symptom:** Can't change DB connection without recompiling  
**Mistake:**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=SMS;...")); // ❌ Hardcoded!
```

**Fix:**
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SMS;..."
  }
}

// Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))); // ✅ From config
```

**Prevention:**
- All secrets in appsettings.json (dev) or Azure Key Vault (prod)
- Never commit appsettings.Development.json to git

---

## 🔍 How to Find These Gotchas

### Automated Search Patterns
```powershell
# Find missing SchoolId filters
Get-ChildItem -Recurse -Include *.cs | Select-String "_context\." | Select-String -NotMatch "SchoolId"

# Find .Result or .Wait()
Get-ChildItem -Recurse -Include *.cs | Select-String "\.Result|\.Wait\("

# Find DateTime.Now
Get-ChildItem -Recurse -Include *.cs | Select-String "DateTime\.Now"

# Find missing [Authorize]
Get-ChildItem -Recurse -Include *Controller.cs | Select-String "public class" | Select-String -NotMatch "\[Authorize\]"

# Find exposed entity models
Get-ChildItem -Recurse -Include *Controller.cs | Select-String "<Student>|<Teacher>|<Class>" | Select-String -NotMatch "Dto"
```

---

## 📋 Prevention Checklist

**Before Committing Code:**
- [ ] All queries filter by SchoolId
- [ ] No SchoolId in request DTOs
- [ ] All actions use async/await (no .Result/.Wait)
- [ ] All GET queries use AsNoTracking
- [ ] [Authorize] on all controllers
- [ ] Only DTOs exposed (not entity models)
- [ ] All DateTime values use UTC
- [ ] All migrations have Down() method
- [ ] No hardcoded secrets
- [ ] Cross-school access tests added

---

**Last Updated:** January 15, 2026  
**Add new gotchas immediately when discovered!**
