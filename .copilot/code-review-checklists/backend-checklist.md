# Code Review Checklist: Backend (.NET)
**Review Type:** Pull Request  
**Estimated Time:** 15-20 minutes per PR  
**Severity Scale:** P0 (Critical) → P5 (Nice to have)

---

## 🔴 P0: CRITICAL - Must Fix Before Merge

### Multi-Tenant Isolation

```powershell
# Automated check
git diff main | Select-String -Pattern "\.Where\(" | Select-String -NotMatch "SchoolId"
```

- [ ] **Every database query filters by SchoolId**
  - ❌ Violation = Data leak between schools
  - Check: All `.Where()`, `.FirstOrDefault()`, `.Any()`, `.Count()`
  
- [ ] **SchoolId is NEVER accepted from request body**
  - ❌ Violation = Privilege escalation
  - Check: All POST/PUT DTOs, no `SchoolId` property
  
- [ ] **Update/Delete verify ownership before modifying**
  - ❌ Violation = Cross-tenant data modification
  ```csharp
  // ✅ REQUIRED pattern
  var entity = await _context.Entities
      .FirstOrDefaultAsync(e => e.Id == id && e.SchoolId == schoolId);
  if (entity == null) return NotFound();
  ```

- [ ] **Controllers inherit from BaseSchoolController**
  - ❌ Violation = No access to GetSchoolId()
  - Check: `public class XController : BaseSchoolController`

**Automated Test:**
```csharp
[Fact]
public async Task GetById_FromDifferentSchool_Returns404()
{
    // Arrange
    var schoolA = Guid.NewGuid();
    var schoolB = Guid.NewGuid();
    var entityId = Guid.NewGuid();
    
    _context.Entities.Add(new Entity { Id = entityId, SchoolId = schoolB });
    await _context.SaveChangesAsync();
    
    // Act
    var result = await _repository.GetByIdAsync(entityId, schoolA);
    
    // Assert
    Assert.Null(result); // ✅ MUST return null, not entity from different school
}
```

---

### SQL Injection Prevention

- [ ] **No raw SQL with string concatenation**
  - ❌ Violation = SQL injection vulnerability
  ```csharp
  // ❌ WRONG
  var sql = $"SELECT * FROM Students WHERE Name = '{name}'";
  
  // ✅ CORRECT
  var sql = "SELECT * FROM Students WHERE Name = {0}";
  _context.Students.FromSqlRaw(sql, name);
  ```

- [ ] **All raw SQL uses parameters**
  - Check: `FromSqlRaw()`, `ExecuteSqlRaw()` calls

---

### Authentication & Authorization

- [ ] **All endpoints have [Authorize] attribute** (except login/register)
  - ❌ Violation = Unauthenticated access
  ```powershell
  # Find missing [Authorize]
  git diff main | Select-String -Pattern "\[Http(Get|Post|Put|Delete)\]" -Context 2 | Select-String -NotMatch "Authorize"
  ```

- [ ] **JWT token includes SchoolId claim**
  - ❌ Violation = Cannot enforce multi-tenancy
  ```csharp
  // ✅ REQUIRED in token generation
  new Claim("SchoolId", user.SchoolId.ToString())
  ```

---

## 🟠 P1: HIGH - Must Fix Before Merge

### Input Validation

- [ ] **Every request DTO has FluentValidation validator**
  ```powershell
  # Find DTOs without validators
  Get-ChildItem "SMSDataModel/CombineModel" -Filter "*Dto.cs" | ForEach-Object {
      $validator = $_.Name -replace "Dto.cs", "Validator.cs"
      if (-not (Test-Path "SMSServices/Validators/$validator")) {
          Write-Host "Missing: $validator" -ForegroundColor Yellow
      }
  }
  ```

- [ ] **Required fields validated**
  ```csharp
  RuleFor(x => x.FirstName).NotEmpty();
  ```

- [ ] **Max lengths validated** (must match database constraints)
  ```csharp
  RuleFor(x => x.FirstName).MaximumLength(100);
  ```

- [ ] **Email format validated**
  ```csharp
  RuleFor(x => x.Email).EmailAddress();
  ```

---

### Performance Standards

- [ ] **AsNoTracking() used on read-only queries**
  - 🐌 Impact = 30-40% slower, 40% more memory
  ```csharp
  // ✅ REQUIRED for all GET operations
  .AsNoTracking()
  .ToListAsync()
  ```

- [ ] **Pagination implemented for list endpoints**
  - 🐌 Impact = 95% slower, 500KB → 25KB data transfer
  ```csharp
  .Skip((pageNumber - 1) * pageSize)
  .Take(pageSize)
  ```

- [ ] **No N+1 query patterns**
  - 🐌 Impact = 97% slower (3.2s → 85ms)
  ```csharp
  // ✅ Use Include() for navigation properties
  .Include(s => s.Enrollments)
  ```

---

### Error Handling

- [ ] **Global exception handler configured**
  - Check: `app.UseExceptionHandler()` in Program.cs

- [ ] **Specific exceptions caught** (not just `catch (Exception)`)
  ```csharp
  // ✅ CORRECT
  catch (DbUpdateException ex) { /* Handle */ }
  catch (ValidationException ex) { /* Handle */ }
  
  // ❌ WRONG
  catch (Exception ex) { /* Swallows all errors */ }
  ```

- [ ] **Meaningful error messages** (no stack traces to client)
  ```csharp
  return BadRequest(new { error = "Invalid email format" });
  // Not: return BadRequest(ex.ToString());
  ```

---

## 🟡 P2: MEDIUM - Fix Within 24 Hours

### Code Quality

- [ ] **No code duplication** (DRY principle)
  - Look for: Copy-pasted controller methods, repeated validation logic

- [ ] **Meaningful variable names**
  ```csharp
  // ✅ GOOD
  var activeStudents = await _context.Students
      .Where(s => s.SchoolId == schoolId && s.IsActive == true)
      .ToListAsync();
  
  // ❌ BAD
  var data = await _context.Students
      .Where(x => x.SchoolId == y && x.IsActive == true)
      .ToListAsync();
  ```

- [ ] **Async all the way** (no `.Result` or `.Wait()`)
  - ❌ Violation = Deadlock risk
  ```csharp
  // ❌ WRONG
  var student = _studentService.GetByIdAsync(id).Result;
  
  // ✅ CORRECT
  var student = await _studentService.GetByIdAsync(id);
  ```

---

### Repository Pattern

- [ ] **Repository methods have `Guid schoolId` parameter**
  ```csharp
  Task<Student?> GetByIdAsync(Guid id, Guid schoolId); // ✅
  Task<Student?> GetByIdAsync(Guid id); // ❌
  ```

- [ ] **Delete is soft delete** (IsActive = false)
  ```csharp
  // ✅ CORRECT
  entity.IsActive = false;
  entity.UpdatedAt = DateTime.UtcNow;
  _context.Update(entity);
  ```

- [ ] **Repositories use specific interfaces** (not generic)
  ```csharp
  public interface IStudentRepository // ✅
  public interface IRepository<T> // ❌ Too generic, can't enforce SchoolId
  ```

---

### Testing

- [ ] **Unit tests for new methods**
  - Minimum: Happy path + error case

- [ ] **AAA pattern used** (Arrange-Act-Assert)
  ```csharp
  [Fact]
  public async Task GetById_ValidId_ReturnsStudent()
  {
      // Arrange
      var schoolId = Guid.NewGuid();
      var studentId = Guid.NewGuid();
      // ... setup
      
      // Act
      var result = await _repository.GetByIdAsync(studentId, schoolId);
      
      // Assert
      Assert.NotNull(result);
      Assert.Equal(studentId, result.Id);
  }
  ```

- [ ] **Cross-school access tests**
  ```csharp
  [Fact]
  public async Task GetById_DifferentSchool_ReturnsNull()
  {
      // Test that School A cannot access School B data
  }
  ```

---

## 🟢 P3: LOW - Fix in Next Sprint

### Documentation

- [ ] **XML comments on public methods**
  ```csharp
  /// <summary>
  /// Retrieves a student by ID within the specified school.
  /// </summary>
  /// <param name="id">The student's unique identifier</param>
  /// <param name="schoolId">The school's unique identifier</param>
  /// <returns>Student if found, null otherwise</returns>
  public async Task<Student?> GetByIdAsync(Guid id, Guid schoolId)
  ```

- [ ] **README updated** (if new feature added)

---

### Logging

- [ ] **Appropriate log levels**
  - Information: Business events
  - Warning: Recoverable errors
  - Error: Exceptions
  - Critical: Data loss, security breaches

- [ ] **No sensitive data in logs**
  ```csharp
  // ✅ CORRECT
  _logger.LogInformation("User {UserId} logged in", userId);
  
  // ❌ WRONG
  _logger.LogInformation("Login: {Email} / {Password}", email, password);
  ```

- [ ] **Structured logging used**
  ```csharp
  _logger.LogInformation("Student {StudentId} created in school {SchoolId}", 
      student.Id, student.SchoolId);
  ```

---

### Database Migrations

- [ ] **Migration has meaningful name**
  ```powershell
  # ✅ GOOD
  dotnet ef migrations add AddStudentBirthDateColumn
  
  # ❌ BAD
  dotnet ef migrations add Update1
  ```

- [ ] **Down() method implemented**
  ```csharp
  protected override void Down(MigrationBuilder migrationBuilder)
  {
      migrationBuilder.DropColumn(name: "BirthDate", table: "Students");
  }
  ```

- [ ] **Default values for new required columns**
  ```csharp
  migrationBuilder.AddColumn<bool>(
      name: "IsActive",
      table: "Students",
      nullable: false,
      defaultValue: true); // ✅ Existing rows get default
  ```

---

## 📋 Review Workflow

### 1. Automated Checks (5 minutes)
```powershell
cd Backend

# Run security scan
./security-scan.ps1

# Run tests
dotnet test

# Check code coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporter=lcov
```

### 2. Manual Review (10 minutes)
- [ ] Read through all changed files
- [ ] Check for patterns from `.copilot/memory/gotchas.md`
- [ ] Verify against `.copilot/critical-rules.md`
- [ ] Test API endpoints in Postman

### 3. Approval (5 minutes)
- [ ] All P0 issues fixed
- [ ] All P1 issues fixed
- [ ] P2 issues documented (if not fixed)
- [ ] Tests passing
- [ ] Security scan passing

---

## 🚨 Common Issues Found in PRs

| Issue | Frequency | Severity | Fix Time |
|-------|-----------|----------|----------|
| Missing SchoolId filter | 40% | P0 | 5 min |
| No [Authorize] attribute | 25% | P0 | 2 min |
| Missing AsNoTracking | 35% | P1 | 3 min |
| No pagination | 20% | P1 | 10 min |
| Using .Result/.Wait() | 15% | P2 | 5 min |
| No input validation | 20% | P1 | 15 min |
| Missing unit tests | 30% | P2 | 20 min |
| Accepting SchoolId from body | 10% | P0 | 10 min |

---

## ✅ Approval Comments Template

**For Approving:**
```markdown
✅ **APPROVED**

**Reviewed:**
- [x] Multi-tenant isolation verified
- [x] Security checks passed
- [x] Performance standards met
- [x] Tests added and passing

**Notes:**
- Great implementation of SchoolId filtering
- Good test coverage (85%)

**Minor Suggestions (P3):**
- Consider adding XML comments to GetByIdAsync()
```

**For Requesting Changes:**
```markdown
🔄 **CHANGES REQUESTED**

**P0 Issues (MUST FIX):**
- [ ] Line 45: Missing SchoolId filter in query
- [ ] Line 67: Accepting SchoolId from request body

**P1 Issues (MUST FIX):**
- [ ] Line 92: No AsNoTracking on read-only query
- [ ] Line 123: Missing pagination (endpoint returns 10,000+ records)

**P2 Issues (Fix if time allows):**
- [ ] Line 156: Using .Result instead of await
- [ ] Missing unit test for GetAll method

**How to Fix:**
See `.copilot/memory/gotchas.md` for examples.
```

---

## 📊 Code Review Metrics

Track these over time to improve team performance:

- **Average time to review:** Target < 24 hours
- **P0 issues per PR:** Target < 1
- **P1 issues per PR:** Target < 2
- **Test coverage:** Target > 80%
- **Reviews requiring changes:** Target < 30%

**Save metrics in:** `docs/code-review-metrics.md`

---

**Related Files:**
- `.copilot/code-review-checklists/frontend-checklist.md`
- `.copilot/code-review-checklists/security-checklist.md`
- `.copilot/memory/gotchas.md`
- `.copilot/critical-rules.md`
