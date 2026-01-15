# Code Review Checklist: Security
**Review Type:** Security Audit  
**Estimated Time:** 15-20 minutes per PR  
**Risk Level:** CRITICAL

---

## 🔴 P0: CRITICAL VULNERABILITIES - Block Merge

### Multi-Tenant Data Isolation

**Risk:** Data leak between schools (GDPR violation, loss of trust)

**Automated Detection:**
```powershell
# Run this before every security review
cd Backend

# Check for queries without SchoolId filter
git diff main | Select-String -Pattern "\.Where\(" | Select-String -NotMatch "SchoolId"

# Check for SchoolId in request DTOs
git diff main | Select-String -Pattern "class.*Dto" -Context 5 | Select-String "SchoolId"
```

**Manual Checks:**

- [ ] **Every database query filters by SchoolId**
  ```csharp
  // ✅ CORRECT
  var students = await _context.Students
      .Where(s => s.SchoolId == schoolId && s.IsActive == true)
      .ToListAsync();
  
  // ❌ CRITICAL VIOLATION - Data leak!
  var students = await _context.Students
      .Where(s => s.IsActive == true) // Missing SchoolId!
      .ToListAsync();
  ```

- [ ] **SchoolId is NEVER in request DTOs**
  ```csharp
  // ❌ CRITICAL VIOLATION - Privilege escalation!
  public class CreateStudentDto
  {
      public Guid SchoolId { get; set; } // NEVER!
      public string FirstName { get; set; }
  }
  
  // ✅ CORRECT
  public class CreateStudentDto
  {
      // No SchoolId property
      public string FirstName { get; set; }
  }
  ```

- [ ] **SchoolId retrieved from JWT claims only**
  ```csharp
  // ✅ CORRECT
  var schoolId = GetSchoolId(); // From BaseSchoolController
  
  // ❌ WRONG - Never trust client
  var schoolId = dto.SchoolId; // From request body
  ```

- [ ] **Update/Delete verify ownership before modification**
  ```csharp
  // ✅ CORRECT - Verify ownership
  var student = await _context.Students
      .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId);
  
  if (student == null)
      return NotFound(); // Don't reveal existence
  
  // Now safe to update
  student.FirstName = dto.FirstName;
  
  // ❌ CRITICAL VIOLATION - No ownership check!
  var student = await _context.Students.FindAsync(id);
  student.FirstName = dto.FirstName; // Can modify other schools' data!
  ```

**Test Cases (MANDATORY):**
```csharp
[Fact]
public async Task GetById_FromDifferentSchool_Returns404()
{
    // Arrange
    var schoolA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var schoolB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var studentId = Guid.NewGuid();
    
    _context.Students.Add(new Student 
    { 
        Id = studentId, 
        SchoolId = schoolB, // Belongs to School B
        FirstName = "John" 
    });
    await _context.SaveChangesAsync();
    
    // Act - School A tries to access
    var result = await _repository.GetByIdAsync(studentId, schoolA);
    
    // Assert
    Assert.Null(result); // ✅ MUST return null
    // If returns student = CRITICAL VULNERABILITY
}

[Fact]
public async Task Update_DifferentSchool_Returns404()
{
    // Arrange
    var schoolA = Guid.NewGuid();
    var schoolB = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    
    _context.Students.Add(new Student { Id = studentId, SchoolId = schoolB });
    await _context.SaveChangesAsync();
    
    var dto = new UpdateStudentDto { FirstName = "Hacked" };
    
    // Act - School A tries to update School B's student
    var result = await _controller.Update(studentId, dto);
    
    // Assert
    Assert.IsType<NotFoundResult>(result);
    
    // Verify no modification
    var student = await _context.Students.FindAsync(studentId);
    Assert.NotEqual("Hacked", student.FirstName);
}
```

---

### SQL Injection

**Risk:** Database compromise, data theft, data loss

**Automated Detection:**
```powershell
# Check for raw SQL
git diff main | Select-String -Pattern "FromSqlRaw|ExecuteSqlRaw"

# Check for string concatenation in SQL
git diff main | Select-String -Pattern '\$".*SELECT|INSERT|UPDATE|DELETE'
```

**Manual Checks:**

- [ ] **No string concatenation in SQL**
  ```csharp
  // ❌ CRITICAL VIOLATION - SQL injection!
  var name = "Robert'; DROP TABLE Students;--";
  var sql = $"SELECT * FROM Students WHERE Name = '{name}'";
  var students = await _context.Students.FromSqlRaw(sql).ToListAsync();
  
  // ✅ CORRECT - Parameterized query
  var students = await _context.Students
      .FromSqlRaw("SELECT * FROM Students WHERE Name = {0}", name)
      .ToListAsync();
  ```

- [ ] **All raw SQL uses parameters**
  ```csharp
  // ✅ CORRECT
  await _context.Database.ExecuteSqlRawAsync(
      "UPDATE Students SET IsActive = {0} WHERE SchoolId = {1}",
      false, schoolId);
  ```

- [ ] **Prefer LINQ over raw SQL**
  ```csharp
  // ✅ BEST - EF Core handles parameterization
  var students = await _context.Students
      .Where(s => s.Name.Contains(searchTerm))
      .ToListAsync();
  ```

**Test Case:**
```csharp
[Fact]
public async Task Search_SqlInjection_DoesNotDropTable()
{
    // Arrange
    var maliciousInput = "Robert'; DROP TABLE Students;--";
    
    // Act
    var result = await _service.SearchStudentsAsync(maliciousInput, schoolId);
    
    // Assert - Should not throw, table should still exist
    Assert.NotNull(result);
    var tableExists = await _context.Students.AnyAsync();
    Assert.True(tableExists); // ✅ Table not dropped
}
```

---

### Authentication Bypass

**Risk:** Unauthorized access to all data

**Automated Detection:**
```powershell
# Find endpoints without [Authorize]
git diff main | Select-String -Pattern "\[Http(Get|Post|Put|Delete)\]" -Context 2 | Select-String -NotMatch "Authorize"
```

**Manual Checks:**

- [ ] **All endpoints have [Authorize] attribute** (except Login/Register)
  ```csharp
  // ✅ CORRECT
  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  
  // ❌ CRITICAL VIOLATION - Anyone can access!
  [HttpGet] // Missing [Authorize]
  public async Task<IActionResult> GetAll()
  ```

- [ ] **JWT token includes required claims**
  ```csharp
  // ✅ REQUIRED claims
  var claims = new List<Claim>
  {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim("SchoolId", user.SchoolId.ToString()), // ✅ MANDATORY
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
  };
  ```

- [ ] **Token expiration configured** (15-60 minutes)
  ```csharp
  // ✅ CORRECT
  Expires = DateTime.UtcNow.AddMinutes(60),
  
  // ❌ WRONG - Token never expires
  // (omitting Expires property)
  ```

- [ ] **Refresh tokens implemented** (for sessions > 1 hour)

**Test Cases:**
```csharp
[Fact]
public async Task GetAll_WithoutToken_Returns401()
{
    // Arrange
    _httpClient.DefaultRequestHeaders.Authorization = null;
    
    // Act
    var response = await _httpClient.GetAsync("/api/students");
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}

[Fact]
public async Task GetAll_WithExpiredToken_Returns401()
{
    // Arrange
    var expiredToken = GenerateExpiredToken();
    _httpClient.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", expiredToken);
    
    // Act
    var response = await _httpClient.GetAsync("/api/students");
    
    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

### XSS (Cross-Site Scripting)

**Risk:** Session hijacking, credential theft, malware injection

**Frontend Detection:**
```powershell
cd Frontend
Select-String -Path "src/**/*.tsx" -Pattern "dangerouslySetInnerHTML|innerHTML|outerHTML"
```

**Manual Checks:**

- [ ] **No dangerouslySetInnerHTML without DOMPurify**
  ```typescript
  // ❌ CRITICAL VIOLATION - XSS!
  <div dangerouslySetInnerHTML={{ __html: userComment }} />
  
  // ✅ CORRECT - Sanitized
  import DOMPurify from 'dompurify';
  <div dangerouslySetInnerHTML={{ 
    __html: DOMPurify.sanitize(userComment) 
  }} />
  
  // ✅ BEST - Let React handle escaping
  <div>{userComment}</div>
  ```

- [ ] **Backend sanitizes HTML input**
  ```csharp
  // Install: HtmlSanitizer NuGet package
  using Ganss.XSS;
  
  var sanitizer = new HtmlSanitizer();
  dto.Description = sanitizer.Sanitize(dto.Description);
  ```

- [ ] **Content-Security-Policy header configured**
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add(
          "Content-Security-Policy",
          "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");
      await next();
  });
  ```

**Test Case:**
```typescript
test('displays user input without executing scripts', () => {
  const maliciousInput = '<img src=x onerror="alert(\'XSS\')">';
  render(<StudentCard description={maliciousInput} />);
  
  // Should render as text, not execute script
  expect(screen.getByText(maliciousInput)).toBeInTheDocument();
  
  // Alert should not have been called
  expect(window.alert).not.toHaveBeenCalled();
});
```

---

## 🟠 P1: HIGH - Must Fix Before Merge

### Input Validation

**Risk:** Data corruption, application crashes, logic bypass

- [ ] **Every DTO has FluentValidation validator**
  ```csharp
  public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
  {
      public CreateStudentDtoValidator()
      {
          RuleFor(x => x.FirstName)
              .NotEmpty()
              .MaximumLength(100)
              .Matches("^[a-zA-Z\\s]+$"); // ✅ Whitelist pattern
          
          RuleFor(x => x.Email)
              .NotEmpty()
              .EmailAddress()
              .MaximumLength(200);
          
          RuleFor(x => x.PhoneNumber)
              .Matches(@"^\\+?[1-9]\\d{1,14}$") // E.164 format
              .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
      }
  }
  ```

- [ ] **Max lengths match database constraints**
  ```csharp
  // Database: VARCHAR(100)
  RuleFor(x => x.FirstName).MaximumLength(100); // ✅ Match
  ```

- [ ] **Regex patterns use whitelists** (not blacklists)
  ```csharp
  // ✅ GOOD - Whitelist allowed characters
  .Matches("^[a-zA-Z0-9\\s]+$")
  
  // ❌ BAD - Trying to blacklist everything dangerous
  .Matches("^(?!.*(<|>|script)).*$")
  ```

---

### CORS (Cross-Origin Resource Sharing)

**Risk:** Unauthorized API access from malicious sites

- [ ] **CORS allows specific origins only** (not `*`)
  ```csharp
  // ✅ CORRECT - Production
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("Production", policy =>
      {
          policy.WithOrigins("https://yourdomain.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
      });
  });
  
  // ⚠️  DEVELOPMENT ONLY
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("Development", policy =>
      {
          policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
      });
  });
  
  // ❌ CRITICAL VIOLATION - Never in production!
  policy.AllowAnyOrigin() // Allows all websites!
  ```

- [ ] **CORS policy switches based on environment**
  ```csharp
  if (app.Environment.IsDevelopment())
      app.UseCors("Development");
  else
      app.UseCors("Production");
  ```

---

### Sensitive Data Exposure

**Risk:** Credential theft, compliance violations (GDPR)

- [ ] **Entity models NEVER returned directly**
  ```csharp
  // ❌ WRONG - Exposes PasswordHash, SecurityStamp
  [HttpGet]
  public async Task<ActionResult<IEnumerable<User>>> GetAll()
  {
      return await _context.Users.ToListAsync();
  }
  
  // ✅ CORRECT - Use DTO
  [HttpGet]
  public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
  {
      return await _context.Users
          .Select(u => new UserDto 
          {
              Id = u.Id,
              Email = u.Email,
              FirstName = u.FirstName
              // PasswordHash NOT included
          })
          .ToListAsync();
  }
  ```

- [ ] **No sensitive data in logs**
  ```csharp
  // ❌ CRITICAL VIOLATION
  _logger.LogInformation("Login attempt: {Email} / {Password}", email, password);
  
  // ✅ CORRECT
  _logger.LogInformation("Login attempt for user {UserId}", userId);
  ```

- [ ] **Connection strings in environment variables**
  ```json
  // ❌ WRONG - appsettings.json
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Password=MyPassword123;"
  }
  
  // ✅ CORRECT - User secrets / Azure Key Vault
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=..."
  ```

- [ ] **API keys not in source code**
  ```csharp
  // ❌ WRONG
  var apiKey = "sk_live_12345abcdef";
  
  // ✅ CORRECT
  var apiKey = _configuration["ExternalApi:ApiKey"];
  ```

---

## 🟡 P2: MEDIUM - Fix Within 24 Hours

### Security Headers

- [ ] **Security headers configured**
  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
      context.Response.Headers.Add("X-Frame-Options", "DENY");
      context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
      context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
      context.Response.Headers.Add(
          "Permissions-Policy", 
          "geolocation=(), microphone=(), camera=()");
      await next();
  });
  ```

- [ ] **HTTPS enforced in production**
  ```csharp
  if (!app.Environment.IsDevelopment())
  {
      app.UseHttpsRedirection();
      app.UseHsts();
  }
  ```

---

### Rate Limiting

- [ ] **Rate limiting configured** (prevent brute force)
  ```csharp
  // Install: AspNetCoreRateLimit
  builder.Services.AddMemoryCache();
  builder.Services.Configure<IpRateLimitOptions>(options =>
  {
      options.GeneralRules = new List<RateLimitRule>
      {
          new RateLimitRule
          {
              Endpoint = "POST:/api/auth/login",
              Period = "1m",
              Limit = 5 // 5 attempts per minute
          }
      };
  });
  ```

---

### Error Messages

- [ ] **No stack traces to client** (production)
  ```csharp
  if (app.Environment.IsDevelopment())
      app.UseDeveloperExceptionPage();
  else
      app.UseExceptionHandler("/error"); // Generic error page
  ```

- [ ] **Error messages don't reveal system details**
  ```csharp
  // ❌ WRONG - Reveals database structure
  return BadRequest($"Foreign key constraint violation on Students.SchoolId");
  
  // ✅ CORRECT - Generic message
  return BadRequest("Unable to delete. Related records exist.");
  ```

---

## 📋 Security Review Workflow

### 1. Run Automated Scans (5 minutes)
```powershell
cd Backend

# Security scan script
./security-scan.ps1

# Dependency vulnerabilities
dotnet list package --vulnerable

# Frontend dependencies
cd ../Frontend
npm audit
```

### 2. Manual Code Review (10 minutes)
- [ ] Check all database queries for SchoolId
- [ ] Verify no SchoolId in DTOs
- [ ] Check for raw SQL
- [ ] Verify [Authorize] on all endpoints
- [ ] Check for XSS vulnerabilities (frontend)

### 3. Security Testing (10 minutes)
- [ ] Run cross-school access tests
- [ ] Test SQL injection attempts
- [ ] Test expired JWT token
- [ ] Test unauthorized access (no token)
- [ ] Test XSS payloads (frontend)

### 4. Security Approval
- [ ] All P0 issues resolved
- [ ] All P1 issues resolved
- [ ] P2 issues documented
- [ ] All security tests passing

---

## 🚨 Security Incident Response

**If Security Vulnerability Found:**

1. **Immediate Actions:**
   - Block PR merge
   - Notify team lead
   - Assess severity (P0 = Critical)

2. **P0 Critical Vulnerabilities:**
   - Create hotfix branch
   - Fix immediately
   - Deploy to production ASAP
   - Notify affected users (if data accessed)

3. **Documentation:**
   - Log in security incident register
   - Document root cause
   - Add to `.copilot/memory/gotchas.md`
   - Update security tests

---

## ✅ Security Approval Template

```markdown
🔒 **SECURITY REVIEW - APPROVED**

**Reviewed By:** [Your Name]
**Date:** [Date]
**PR:** #[PR Number]

**Security Checks:**
- [x] Multi-tenant isolation verified (all queries filter by SchoolId)
- [x] No SchoolId accepted from request body
- [x] SQL injection prevention verified (parameterized queries)
- [x] Authentication present ([Authorize] on all endpoints)
- [x] XSS prevention verified (no dangerouslySetInnerHTML)
- [x] Input validation implemented (FluentValidation)
- [x] CORS configured correctly (specific origins)
- [x] No sensitive data in logs
- [x] Security headers configured
- [x] All security tests passing

**Vulnerabilities Found:** 0 P0, 0 P1, 0 P2

**Approval:** ✅ Safe to merge
```

```markdown
🚨 **SECURITY REVIEW - CRITICAL ISSUES FOUND**

**Reviewed By:** [Your Name]
**Date:** [Date]
**PR:** #[PR Number]

**P0 CRITICAL VULNERABILITIES (BLOCK MERGE):**
1. ❌ Line 67: Query missing SchoolId filter
   - **Risk:** Data leak between schools
   - **Fix:** Add `.Where(s => s.SchoolId == schoolId)`

2. ❌ Line 123: SchoolId accepted from request body
   - **Risk:** Privilege escalation
   - **Fix:** Remove SchoolId from DTO, use GetSchoolId()

3. ❌ Line 201: SQL injection vulnerability
   - **Risk:** Database compromise
   - **Fix:** Use parameterized query

**P1 HIGH ISSUES:**
- Line 89: Missing [Authorize] attribute
- Line 145: No input validation

**STATUS:** 🚫 DO NOT MERGE - Fix P0 issues immediately

**Next Steps:**
1. Fix all P0 issues
2. Re-request security review
3. Run security tests
```

---

**Related Files:**
- `.copilot/critical-rules.md`
- `.copilot/memory/gotchas.md`
- `.copilot/workflows/security-review-checklist.md`
- `.copilot/agents/security-agent.md`
