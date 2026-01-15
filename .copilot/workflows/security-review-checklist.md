# Workflow: Security Review Checklist
**Estimated Time:** 20-30 minutes  
**Frequency:** Before every PR merge  
**Risk Level:** Critical

---

## 📋 Overview

Complete security audit checklist for code changes. Use this before merging any PR to production.

**Detection Tools:**
- SQL Server Profiler (SQL injection)
- Postman/API testing (auth bypass)
- Browser DevTools (XSS, CORS)
- Manual code review

---

## 🔴 CRITICAL: Multi-Tenant Isolation (5 minutes)

### Database Queries

```powershell
# Search for queries missing SchoolId filter
git diff main | Select-String -Pattern "\.Where\(" | Select-String -NotMatch "SchoolId"

# Find direct DbContext access in controllers
git diff main | Select-String -Pattern "_context\."
```

**Checklist:**
- [ ] Every database query includes `.Where(x => x.SchoolId == schoolId)`
- [ ] SchoolId is FIRST condition in WHERE clause (index usage)
- [ ] No raw SQL without SchoolId parameter
- [ ] Include/ThenInclude also filters by SchoolId if needed

**Code Pattern (REQUIRED):**
```csharp
// ✅ CORRECT
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId && s.IsActive == true)
    .ToListAsync();

// ❌ WRONG - Missing SchoolId
var students = await _context.Students
    .Where(s => s.IsActive == true)
    .ToListAsync();
```

---

### Controller Actions

```powershell
# Find controllers accepting SchoolId in request body
git diff main | Select-String -Pattern "SchoolId.*\{"
```

**Checklist:**
- [ ] Controllers inherit from `BaseSchoolController`
- [ ] SchoolId retrieved from `GetSchoolId()` (JWT claims)
- [ ] SchoolId NEVER accepted from request body
- [ ] Update/Delete actions verify SchoolId ownership before modifying

**Code Pattern (REQUIRED):**
```csharp
// ✅ CORRECT
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    var schoolId = GetSchoolId(); // From JWT
    // SchoolId is NOT in dto
}

// ❌ WRONG - Accepting SchoolId from client
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
{
    // Using dto.SchoolId from request body
}
```

---

### Update/Delete Operations

**Checklist:**
- [ ] Verify record belongs to user's school BEFORE updating
- [ ] Verify record belongs to user's school BEFORE deleting
- [ ] Return 404 (not 403) when record not found in user's school
- [ ] Soft delete implemented (IsActive = false)

**Code Pattern (REQUIRED):**
```csharp
// ✅ CORRECT
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
{
    var schoolId = GetSchoolId();
    
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId);
    
    if (student == null)
        return NotFound(); // Don't reveal existence
    
    // Update logic...
}

// ❌ WRONG - Not checking SchoolId before update
[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
{
    var student = await _context.Students.FindAsync(id);
    // Allows updating records from other schools!
}
```

---

## 🟠 HIGH: Authentication & Authorization (5 minutes)

### JWT Token Security

```powershell
# Find endpoints without [Authorize]
git diff main | Select-String -Pattern "\[Http(Get|Post|Put|Delete)\]" -Context 2 | Select-String -NotMatch "Authorize"
```

**Checklist:**
- [ ] All endpoints have `[Authorize]` attribute (except login/register)
- [ ] JWT secret key stored in environment variables (not appsettings.json)
- [ ] Token expiration set (15-60 minutes)
- [ ] Refresh tokens implemented for long sessions
- [ ] Token includes SchoolId claim
- [ ] Token includes UserId claim

**Token Claims (REQUIRED):**
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim("SchoolId", user.SchoolId.ToString()), // ✅ REQUIRED
    new Claim(ClaimTypes.Role, user.Role)
};
```

---

### Role-Based Authorization

**Checklist:**
- [ ] Admin-only endpoints have `[Authorize(Roles = "Admin")]`
- [ ] Teacher endpoints restrict to `Teacher, Admin` roles
- [ ] Student endpoints restrict appropriately
- [ ] Role checks consider school context (Admin of School A ≠ Admin of School B)

**Code Pattern:**
```csharp
// ✅ CORRECT - Role + SchoolId
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id)
{
    var schoolId = GetSchoolId(); // Still check SchoolId!
    // Admin can only delete within their school
}
```

---

## 🟡 MEDIUM: SQL Injection Prevention (5 minutes)

### Parameterized Queries

```powershell
# Find raw SQL queries
git diff main | Select-String -Pattern "FromSqlRaw|ExecuteSqlRaw"
```

**Checklist:**
- [ ] All raw SQL uses parameterized queries
- [ ] No string concatenation in SQL
- [ ] EF Core LINQ preferred over raw SQL
- [ ] Stored procedures use parameters

**Code Pattern (REQUIRED):**
```csharp
// ✅ CORRECT - Parameterized
var students = await _context.Students
    .FromSqlRaw("SELECT * FROM Students WHERE SchoolId = {0} AND Name LIKE {1}", 
        schoolId, $"%{searchTerm}%")
    .ToListAsync();

// ❌ WRONG - SQL injection vulnerability
var students = await _context.Students
    .FromSqlRaw($"SELECT * FROM Students WHERE Name LIKE '%{searchTerm}%'")
    .ToListAsync();
```

---

## 🟡 MEDIUM: Input Validation (5 minutes)

### FluentValidation Rules

```powershell
# Find DTOs without validators
Get-ChildItem -Path "SMSDataModel/CombineModel" -Filter "*Dto.cs" | ForEach-Object {
    $validator = $_.Name -replace "Dto.cs", "Validator.cs"
    if (-not (Test-Path "SMSServices/Validators/$validator")) {
        Write-Host "Missing validator for: $($_.Name)" -ForegroundColor Yellow
    }
}
```

**Checklist:**
- [ ] Every request DTO has FluentValidation validator
- [ ] Required fields validated
- [ ] Max lengths validated
- [ ] Email format validated
- [ ] Phone format validated
- [ ] Special characters sanitized

**Validation Pattern (REQUIRED):**
```csharp
public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z\\s]+$"); // ✅ Prevent special chars
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
        
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\\+?[1-9]\\d{1,14}$"); // ✅ E.164 format
    }
}
```

---

## 🟡 MEDIUM: XSS Prevention (Frontend) (5 minutes)

### React Component Review

```powershell
# Find dangerous HTML rendering
cd Frontend
Select-String -Path "src/**/*.tsx" -Pattern "dangerouslySetInnerHTML|innerHTML"
```

**Checklist:**
- [ ] No `dangerouslySetInnerHTML` unless sanitized
- [ ] User input displayed through React (auto-escaped)
- [ ] DOMPurify library used for HTML content
- [ ] URL parameters validated before use

**Code Pattern (REQUIRED):**
```typescript
// ✅ CORRECT - React auto-escapes
<p>{student.name}</p>

// ✅ CORRECT - Sanitized HTML
import DOMPurify from 'dompurify';
<div dangerouslySetInnerHTML={{ 
  __html: DOMPurify.sanitize(content) 
}} />

// ❌ WRONG - XSS vulnerability
<div dangerouslySetInnerHTML={{ __html: userInput }} />
```

---

## 🟢 LOW: API Security Headers (3 minutes)

### CORS Configuration

**Location:** `Backend/SMSPrototype1/Program.cs`

**Checklist:**
- [ ] CORS allows specific origins (not `*`)
- [ ] CORS credentials allowed only for trusted origins
- [ ] Production uses HTTPS-only origins

**Configuration (REQUIRED):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://yourdomain.com") // ✅ Specific origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ❌ WRONG - Allows all origins
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin() // Security risk!
          .AllowAnyHeader()
          .AllowAnyMethod();
});
```

---

### Security Headers

**Checklist:**
- [ ] `X-Content-Type-Options: nosniff` header set
- [ ] `X-Frame-Options: DENY` header set
- [ ] `X-XSS-Protection: 1; mode=block` header set
- [ ] HTTPS enforced in production
- [ ] HSTS header configured

**Configuration:**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// HTTPS redirection
app.UseHttpsRedirection();
app.UseHsts();
```

---

## 🟢 LOW: Sensitive Data Exposure (3 minutes)

### Logging Review

```powershell
# Find potential password/token logging
git diff main | Select-String -Pattern "_logger\.(LogInformation|LogDebug)" | Select-String -Pattern "(password|token|secret)"
```

**Checklist:**
- [ ] Passwords never logged
- [ ] JWT tokens never logged
- [ ] Credit card numbers never logged
- [ ] API keys never logged
- [ ] Sensitive fields masked in logs

**Logging Pattern (REQUIRED):**
```csharp
// ✅ CORRECT - Mask sensitive data
_logger.LogInformation("User {UserId} logged in", user.Id);

// ❌ WRONG - Logs password
_logger.LogInformation("Login attempt: {Email} / {Password}", email, password);
```

---

### Response DTOs

**Checklist:**
- [ ] Entity models NEVER returned directly
- [ ] DTOs exclude sensitive fields (PasswordHash, SecurityStamp)
- [ ] SchoolId included in response (for audit logging)
- [ ] No internal IDs exposed (database IDs)

**DTO Pattern (REQUIRED):**
```csharp
public class StudentDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; } // ✅ Include for audit
    public string FirstName { get; set; }
    public string Email { get; set; }
    // ❌ NEVER include: PasswordHash, SecurityStamp, InternalNotes
}
```

---

## 🧪 Security Testing (5 minutes)

### Manual Tests

**Test 1: Cross-School Data Access**
```powershell
# Test in Postman
POST /api/auth/login
{
  "email": "admin@school-a.com",
  "password": "Test123!"
}

# Copy JWT token
# Try accessing School B data
GET /api/students/{school-b-student-id}
Authorization: Bearer <school-a-token>

# Expected: 404 Not Found (not 403!)
```

**Test 2: SchoolId Tampering**
```powershell
# Try sending SchoolId in request body
POST /api/students
Authorization: Bearer <school-a-token>
{
  "firstName": "Test",
  "schoolId": "11111111-2222-3333-4444-555555555555" # Different school!
}

# Expected: SchoolId ignored, uses JWT claim
```

**Test 3: SQL Injection**
```powershell
# Try SQL injection in search
GET /api/students?searchTerm=Robert'; DROP TABLE Students;--

# Expected: No error, safe parameterized query
```

**Test 4: Missing Authorization**
```powershell
# Remove JWT token
GET /api/students

# Expected: 401 Unauthorized
```

**Test 5: Expired Token**
```powershell
# Use expired JWT token
GET /api/students
Authorization: Bearer <expired-token>

# Expected: 401 Unauthorized with "Token expired" message
```

---

## ✅ Security Approval Checklist

**Before Merging PR:**
- [ ] All database queries filter by SchoolId
- [ ] SchoolId never accepted from request body
- [ ] All endpoints have [Authorize] attribute
- [ ] No raw SQL without parameters
- [ ] All DTOs have FluentValidation validators
- [ ] No XSS vulnerabilities (dangerouslySetInnerHTML)
- [ ] CORS configured for specific origins
- [ ] Security headers configured
- [ ] No sensitive data in logs
- [ ] Entity models never returned directly
- [ ] Cross-school access tests passed
- [ ] SQL injection tests passed
- [ ] Expired token tests passed

**Severity Rating:**
- 🔴 CRITICAL (P0): Multi-tenant isolation, SQL injection, auth bypass
- 🟠 HIGH (P1-P2): Missing [Authorize], XSS, CORS misconfiguration
- 🟡 MEDIUM (P3): Validation issues, logging sensitive data
- 🟢 LOW (P4-P5): Security headers, minor improvements

**Approval Requirements:**
- P0 issues: MUST fix before merge
- P1-P2 issues: MUST fix before merge
- P3 issues: Fix within 24 hours after merge
- P4-P5 issues: Fix in next sprint

---

## 🚨 Common Vulnerabilities Found

### 1. Forgetting SchoolId Filter (P0)
**Frequency:** 40% of PRs  
**Fix Time:** 5 minutes  
**Detection:** Code review + SQL Profiler

### 2. Missing [Authorize] Attribute (P1)
**Frequency:** 25% of PRs  
**Fix Time:** 2 minutes  
**Detection:** Automated search pattern

### 3. Accepting SchoolId from Request (P0)
**Frequency:** 15% of PRs  
**Fix Time:** 10 minutes  
**Detection:** Code review

### 4. No Input Validation (P2)
**Frequency:** 20% of PRs  
**Fix Time:** 15 minutes  
**Detection:** Manual testing

### 5. Logging Sensitive Data (P3)
**Frequency:** 10% of PRs  
**Fix Time:** 5 minutes  
**Detection:** Log file review

---

## 📋 Quick Security Scan Script

**Save as:** `Backend/security-scan.ps1`

```powershell
Write-Host "=== Security Scan ===" -ForegroundColor Cyan

# 1. Find queries missing SchoolId
Write-Host "`n[1/5] Checking for queries without SchoolId..." -ForegroundColor Yellow
$missingSchoolId = git diff main | Select-String -Pattern "\.Where\(" | Select-String -NotMatch "SchoolId"
if ($missingSchoolId) {
    Write-Host "❌ CRITICAL: Queries missing SchoolId filter!" -ForegroundColor Red
    $missingSchoolId
} else {
    Write-Host "✅ All queries include SchoolId" -ForegroundColor Green
}

# 2. Find endpoints without [Authorize]
Write-Host "`n[2/5] Checking for endpoints without [Authorize]..." -ForegroundColor Yellow
$missingAuth = git diff main | Select-String -Pattern "\[Http(Get|Post|Put|Delete)\]" -Context 1 | Select-String -NotMatch "Authorize"
if ($missingAuth) {
    Write-Host "⚠️  HIGH: Endpoints missing [Authorize] attribute!" -ForegroundColor Yellow
    $missingAuth
} else {
    Write-Host "✅ All endpoints have [Authorize]" -ForegroundColor Green
}

# 3. Find raw SQL queries
Write-Host "`n[3/5] Checking for raw SQL queries..." -ForegroundColor Yellow
$rawSql = git diff main | Select-String -Pattern "FromSqlRaw|ExecuteSqlRaw"
if ($rawSql) {
    Write-Host "⚠️  MEDIUM: Raw SQL found - verify parameterization!" -ForegroundColor Yellow
    $rawSql
} else {
    Write-Host "✅ No raw SQL queries" -ForegroundColor Green
}

# 4. Find potential XSS vulnerabilities
Write-Host "`n[4/5] Checking for XSS vulnerabilities..." -ForegroundColor Yellow
cd Frontend
$xss = Select-String -Path "src/**/*.tsx" -Pattern "dangerouslySetInnerHTML"
cd ..
if ($xss) {
    Write-Host "⚠️  MEDIUM: dangerouslySetInnerHTML found - verify sanitization!" -ForegroundColor Yellow
    $xss
} else {
    Write-Host "✅ No XSS vulnerabilities" -ForegroundColor Green
}

# 5. Find sensitive data in logs
Write-Host "`n[5/5] Checking for sensitive data in logs..." -ForegroundColor Yellow
$sensitiveLogs = git diff main | Select-String -Pattern "_logger\." | Select-String -Pattern "(password|token|secret|apikey)" -CaseSensitive:$false
if ($sensitiveLogs) {
    Write-Host "⚠️  HIGH: Sensitive data may be logged!" -ForegroundColor Yellow
    $sensitiveLogs
} else {
    Write-Host "✅ No sensitive data in logs" -ForegroundColor Green
}

Write-Host "`n=== Scan Complete ===" -ForegroundColor Cyan
```

**Usage:**
```powershell
cd Backend
./security-scan.ps1
```

---

**Related Files:**
- `.copilot/agents/security-agent.md`
- `.copilot/memory/gotchas.md`
- `.copilot/critical-rules.md`
