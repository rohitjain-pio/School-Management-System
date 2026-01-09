# Security Implementation Guide - School Management System API

**Last Updated:** January 9, 2026  
**Version:** 1.0  
**Status:** Active

---

## Table of Contents

- [Overview](#overview)
- [Authentication Security](#authentication-security)
- [Authorization Design](#authorization-design)
- [Input Validation & Sanitization](#input-validation--sanitization)
- [Rate Limiting Strategy](#rate-limiting-strategy)
- [Audit Logging](#audit-logging)
- [Sensitive Data Protection](#sensitive-data-protection)
- [CORS Configuration](#cors-configuration)
- [Security Headers](#security-headers)
- [SignalR Security](#signalr-security)
- [Compliance Requirements](#compliance-requirements)

---

## Overview

### Security Objectives

1. **Confidentiality**: Protect student PII, grades, and private communications
2. **Integrity**: Prevent unauthorized data modification
3. **Availability**: Ensure system availability through rate limiting and DDoS protection
4. **Compliance**: Meet FERPA, COPPA, and GDPR requirements
5. **Accountability**: Comprehensive audit logging of all sensitive operations

### Threat Model

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| Unauthorized access to student records | Critical | JWT + RBAC + Resource-based authorization |
| Session hijacking | High | HttpOnly cookies + Secure flags + Token rotation |
| SQL injection | High | Parameterized queries (EF Core) |
| XSS attacks | High | Input sanitization + CSP headers |
| CSRF attacks | Medium | SameSite cookies + Anti-forgery tokens |
| Brute force attacks | Medium | Rate limiting + Account lockout |
| IDOR vulnerabilities | High | Resource-based authorization + GUID validation |
| Mass data enumeration | Medium | Pagination limits + Rate limiting |
| Token theft | High | Token revocation + Short expiration |
| Message encryption key exposure | Critical | Secure key storage + Key rotation |

---

## Authentication Security

### 1. JWT Token Management

#### Token Configuration

```json
{
  "Jwt": {
    "Key": "[256-bit secret key - NEVER commit to source control]",
    "Issuer": "https://api.sms.edu",
    "Audience": "https://sms.edu",
    "ExpirationMinutes": 180,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Security Requirements:**
- **Key Length**: Minimum 256 bits (32 bytes)
- **Key Storage**: Environment variables or Azure Key Vault (NEVER in appsettings.json)
- **Key Rotation**: Every 90 days
- **Algorithm**: HS256 (HMAC-SHA256) or RS256 (RSA with SHA-256)

#### Token Structure

**Access Token Claims:**
```json
{
  "sub": "user-guid",
  "email": "user@sms.edu",
  "role": "Student",
  "schoolId": "school-guid",
  "iat": 1704844800,
  "exp": 1704855600,
  "jti": "token-unique-id"
}
```

**Additional Claims for Authorization:**
- `role`: Primary role (Student, Teacher, Admin, etc.)
- `schoolId`: For multi-tenant authorization
- `classIds`: Array of class GUIDs for teachers
- `permissions`: Custom permissions array

#### Token Lifecycle

```
1. User Login
   ↓
2. Generate Access Token (3 hours) + Refresh Token (7 days)
   ↓
3. Store Access Token in HttpOnly cookie
   ↓
4. Store Refresh Token in database (hashed)
   ↓
5. Client uses Access Token for API requests
   ↓
6. Access Token expires → Use Refresh Token
   ↓
7. Validate Refresh Token → Issue new Access + Refresh Token
   ↓
8. Invalidate old Refresh Token (one-time use)
```

### 2. Refresh Token Strategy

#### Rotating Refresh Tokens

**Benefits:**
- Limits exposure window if refresh token is stolen
- Enables detection of token reuse attacks
- Allows graceful token revocation

**Implementation Flow:**
```
POST /api/Auth/refresh
Request: { refreshToken: "..." }

1. Validate refresh token signature
2. Check if token exists in database
3. Verify token hasn't been revoked
4. Check expiration (7 days)
5. Generate NEW access token + NEW refresh token
6. Revoke OLD refresh token
7. Store NEW refresh token (hashed)
8. Return tokens to client
```

**Database Schema:**
```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(256) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2 NULL,
    ReplacedByToken NVARCHAR(256) NULL,
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500)
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);
```

### 3. Token Revocation & Blacklist

#### Scenarios Requiring Revocation

1. **User Logout**: Revoke all tokens for user
2. **Password Change**: Revoke all existing tokens
3. **Role Change**: Revoke tokens to force re-authentication
4. **Security Breach**: Revoke specific token or all tokens
5. **Account Suspension**: Revoke all tokens immediately

#### Blacklist Strategy

**Option 1: Database Blacklist (Recommended)**
```sql
CREATE TABLE TokenBlacklist (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TokenJti NVARCHAR(100) NOT NULL, -- JWT ID claim
    UserId UNIQUEIDENTIFIER NOT NULL,
    RevokedAt DATETIME2 NOT NULL,
    ExpiresAt DATETIME2 NOT NULL, -- Token original expiration
    Reason NVARCHAR(200)
);

-- Auto-cleanup expired entries
CREATE INDEX IX_TokenBlacklist_ExpiresAt ON TokenBlacklist(ExpiresAt);
```

**Option 2: Redis Cache (High Performance)**
```
Key: "blacklist:{jti}"
Value: { userId, reason, revokedAt }
TTL: Token expiration time (3 hours)
```

**Validation Flow:**
```
1. Extract JWT from request
2. Decode token and get 'jti' claim
3. Check if 'jti' exists in blacklist
   - If YES → Return 401 Unauthorized
   - If NO → Continue validation
4. Validate signature, expiration, issuer, audience
5. Process request
```

### 4. Multi-Factor Authentication (MFA)

#### MFA for Admin Roles

**Required For:**
- Admin
- SuperAdmin
- Principal

**MFA Methods:**
1. **TOTP (Time-based One-Time Password)** - Google Authenticator, Authy
2. **Email OTP** - 6-digit code sent to registered email
3. **SMS OTP** - 6-digit code sent to phone (less secure, use as fallback)

#### MFA Implementation Flow

```
1. User enters username + password
   ↓
2. Validate credentials
   ↓
3. If user has Admin role:
   - Generate MFA challenge
   - Store challenge in cache (5 min expiration)
   - Return: { requiresMfa: true, challengeId: "..." }
   ↓
4. Client prompts for MFA code
   ↓
5. User enters MFA code
   ↓
6. POST /api/Auth/verify-mfa
   Request: { challengeId, mfaCode }
   ↓
7. Validate MFA code
   ↓
8. Issue JWT tokens
```

**TOTP Secret Storage:**
```sql
CREATE TABLE UserMfaSettings (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    TotpSecretEncrypted NVARCHAR(500), -- Encrypted with master key
    BackupCodesEncrypted NVARCHAR(MAX), -- JSON array of backup codes
    MfaEnabled BIT NOT NULL DEFAULT 0,
    EnabledAt DATETIME2,
    LastUsedAt DATETIME2
);
```

### 5. Password Reset Security

#### Secure Password Reset Flow

```
1. User requests password reset
   ↓
2. Generate cryptographically secure reset token
   - Use RandomNumberGenerator.GetBytes(32)
   - Hash token before storing (SHA256)
   ↓
3. Store token with expiration (15 minutes)
   ↓
4. Send email with reset link:
   https://sms.edu/reset-password?token={token}&email={email}
   ↓
5. User clicks link within 15 minutes
   ↓
6. Validate token:
   - Hash submitted token
   - Compare with stored hash
   - Check expiration
   - Verify email matches
   ↓
7. Allow password change
   ↓
8. Invalidate reset token immediately
   ↓
9. Revoke all existing JWT tokens
   ↓
10. Send confirmation email
```

**Reset Token Schema:**
```sql
CREATE TABLE PasswordResetTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(256) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL,
    IpAddress NVARCHAR(45)
);
```

**Security Considerations:**
- Token valid for 15 minutes only
- One-time use only
- Rate limit password reset requests (3 per hour per email)
- Log all password reset attempts
- Notify user via email when password is changed

### 6. Account Lockout

#### Lockout Policy

**Thresholds:**
- **Failed Login Attempts**: 5 within 15 minutes
- **Lockout Duration**: 30 minutes
- **Admin Notification**: After 3 lockouts within 24 hours

**Implementation:**
```sql
CREATE TABLE LoginAttempts (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL, -- NULL if user not found
    Email NVARCHAR(100) NOT NULL,
    IpAddress NVARCHAR(45),
    Success BIT NOT NULL,
    AttemptedAt DATETIME2 NOT NULL,
    FailureReason NVARCHAR(100)
);

CREATE TABLE AccountLockouts (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    LockedAt DATETIME2 NOT NULL,
    UnlocksAt DATETIME2 NOT NULL,
    Reason NVARCHAR(200),
    UnlockedAt DATETIME2 NULL,
    UnlockedBy UNIQUEIDENTIFIER NULL -- Admin user ID
);
```

**Lockout Flow:**
```
1. User attempts login
   ↓
2. Check if account is locked
   - If locked → Return 423 Locked
   ↓
3. Validate credentials
   ↓
4. If invalid:
   - Increment failed attempt counter
   - If failed attempts >= 5 in 15 min:
     → Lock account for 30 minutes
     → Send notification email
   - Return 401 Unauthorized
   ↓
5. If valid:
   - Clear failed attempts
   - Continue authentication
```

### 7. Cookie Configuration

#### HttpOnly Cookie Settings

**Production Configuration:**
```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,           // Prevents JavaScript access
    Secure = true,             // HTTPS only
    SameSite = SameSiteMode.Strict,  // CSRF protection
    MaxAge = TimeSpan.FromHours(3),
    Domain = ".sms.edu",       // Shared across subdomains
    Path = "/",
    IsEssential = true
};
```

**Cookie Names:**
- `auth_token`: JWT access token
- `refresh_token`: Refresh token (optional, can store server-side only)
- `mfa_challenge`: MFA challenge ID (5 min expiration)

**Security Headers for Cookies:**
```
Set-Cookie: auth_token=<JWT>; HttpOnly; Secure; SameSite=Strict; Max-Age=10800; Path=/
```

---

## Authorization Design

### 1. Role-Based Access Control (RBAC)

#### Role Hierarchy

```
SuperAdmin (Level 5)
    ├── Full system access
    ├── Security configuration
    ├── User role management
    └── System-wide settings

Principal (Level 4)
    ├── School-level management
    ├── View all school data
    ├── Approve major changes
    └── Generate reports

Admin (Level 3)
    ├── Manage users within school
    ├── Manage classes and schedules
    ├── View all records
    └── System configuration

SchoolIncharge (Level 3)
    ├── Department-level management
    ├── Oversee multiple classes
    └── Limited administrative tasks

Teacher (Level 2)
    ├── View assigned classes
    ├── Mark attendance
    ├── Manage announcements
    ├── Create chat rooms
    └── Conduct video calls

Student (Level 1)
    ├── View own records
    ├── Submit attendance
    ├── Access assigned chat rooms
    └── Join video calls
```

#### Permission Matrix

| Resource | Student | Teacher | Admin | Principal | SuperAdmin |
|----------|---------|---------|-------|-----------|------------|
| **Own Student Record** | Read | Read | Read/Write | Read/Write | Read/Write/Delete |
| **Other Student Records** | ❌ | Read (assigned classes) | Read/Write | Read/Write | Read/Write/Delete |
| **Teacher Records** | ❌ | Read (own) | Read/Write | Read/Write | Read/Write/Delete |
| **Class Management** | ❌ | Read (assigned) | Read/Write | Read/Write | Read/Write/Delete |
| **Attendance** | Submit (own) | Mark (assigned classes) | Read/Write (all) | Read/Write (all) | Read/Write/Delete |
| **Announcements** | Read | Create/Read (own school) | Create/Read/Write | Create/Read/Write/Delete | Full Access |
| **Chat Rooms** | Join (with access) | Create/Manage (own) | Full Access | Full Access | Full Access |
| **Video Calls** | Join (invited) | Create/Manage | Manage (all) | Full Access | Full Access |
| **User Management** | ❌ | ❌ | Create/Edit (limited) | Create/Edit (school) | Full Access |
| **System Settings** | ❌ | ❌ | Limited | Limited | Full Access |
| **Audit Logs** | ❌ | ❌ | Read | Read | Read/Write |

### 2. Resource-Based Authorization

#### Ownership Validation

**Students can only access their own data:**
```
Endpoint: GET /api/Student/{id}

Authorization Logic:
1. Extract user ID from JWT
2. If role == "Student":
   - Check if {id} == user's student record ID
   - If NO → Return 403 Forbidden
3. If role == "Teacher":
   - Check if student is in teacher's assigned classes
   - If NO → Return 403 Forbidden
4. If role >= "Admin":
   - Allow access (with school validation)
```

**Multi-Tenant Isolation (School-Based):**
```
All queries must include SchoolId validation:

1. Extract schoolId from user's JWT claims
2. Append to all database queries:
   - Students.Where(s => s.SchoolId == userSchoolId)
3. Prevent cross-school data access
4. Except for SuperAdmin (no school restriction)
```

#### Authorization Policies

**Policy Definitions:**

```csharp
// Custom Policies
"ViewOwnRecords"      // Students can view own data only
"ViewAssignedClasses" // Teachers can view assigned classes
"ManageUsers"         // Admin+ can manage users
"ManageSchool"        // Principal+ can manage school
"SystemAdmin"         // SuperAdmin only
"RequireMFA"          // Requires MFA verification
"SameSchool"          // User and resource in same school
```

**Policy Usage:**
```csharp
[Authorize(Policy = "ViewAssignedClasses")]
[Authorize(Policy = "SameSchool")]
public IActionResult GetClassStudents(Guid classId)
{
    // Additional resource-based check
    if (!IsClassAssignedToTeacher(classId, CurrentUserId))
        return Forbid();
    
    return Ok(students);
}
```

### 3. Claims-Based Authorization

#### Custom Claims

**Standard Claims:**
- `sub`: User ID
- `email`: Email address
- `role`: Primary role

**Custom Claims:**
- `schoolId`: School identifier
- `classIds`: JSON array of assigned class IDs
- `permissions`: JSON array of specific permissions
- `mfa_verified`: Boolean indicating MFA completion
- `department`: Department within school

**Fine-Grained Permissions:**
```json
{
  "permissions": [
    "student.read.own",
    "student.write.own",
    "attendance.submit",
    "chat.join",
    "videocall.join"
  ]
}
```

**Permission Validation:**
```csharp
[Authorize]
[RequirePermission("student.write.own")]
public IActionResult UpdateStudent(Guid id, StudentDto dto)
{
    // Validate ownership
    if (!HasPermission("student.write.own", id))
        return Forbid();
    
    return Ok();
}
```

---

## Input Validation & Sanitization

### 1. FluentValidation Rules

#### Student DTO Validation

**Create Student Request:**
```csharp
public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s'-]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-zA-Z\\s'-]+$");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100)
            .Must(email => email.EndsWith("@sms.edu")).WithMessage("Must use school email domain");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Now).WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Invalid date of birth");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => new[] { "Male", "Female", "Other" }.Contains(g))
            .WithMessage("Invalid gender value");

        RuleFor(x => x.PhoneNumber)
            .Matches("^[0-9\\-\\+\\s\\(\\)]+$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .MaximumLength(20);

        RuleFor(x => x.SchoolId)
            .NotEmpty()
            .Must(BeValidGuid).WithMessage("Invalid school ID format");

        RuleFor(x => x.ClassId)
            .Must(BeValidGuid).When(x => x.ClassId.HasValue)
            .WithMessage("Invalid class ID format");
    }

    private bool BeValidGuid(Guid? guid)
    {
        return guid.HasValue && guid.Value != Guid.Empty;
    }
}
```

#### Chat Message Validation

```csharp
public class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .Must(BeValidGuid);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message cannot be empty")
            .MaximumLength(1000).WithMessage("Message too long")
            .Must(NotContainMaliciousContent).WithMessage("Message contains prohibited content");

        RuleFor(x => x.ReplyToId)
            .Must(BeValidGuid).When(x => x.ReplyToId.HasValue);
    }

    private bool NotContainMaliciousContent(string content)
    {
        // Check for script tags, SQL keywords, etc.
        var prohibitedPatterns = new[]
        {
            "<script", "javascript:", "onerror=", "onclick=",
            "DROP TABLE", "DELETE FROM", "INSERT INTO", "UPDATE "
        };

        return !prohibitedPatterns.Any(p => 
            content.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
```

### 2. XSS Prevention

#### HTML Sanitization

**Library**: HtmlSanitizer (NuGet package)

**Sanitization Rules:**
```csharp
var sanitizer = new HtmlSanitizer();

// Allow only safe tags
sanitizer.AllowedTags.Clear();
sanitizer.AllowedTags.Add("b");
sanitizer.AllowedTags.Add("i");
sanitizer.AllowedTags.Add("u");
sanitizer.AllowedTags.Add("p");
sanitizer.AllowedTags.Add("br");
sanitizer.AllowedTags.Add("a");

// Allow only safe attributes
sanitizer.AllowedAttributes.Clear();
sanitizer.AllowedAttributes.Add("href"); // for <a> tags only

// Remove all scripts and event handlers
sanitizer.AllowedSchemes.Clear();
sanitizer.AllowedSchemes.Add("http");
sanitizer.AllowedSchemes.Add("https");
sanitizer.AllowedSchemes.Add("mailto");

var sanitizedContent = sanitizer.Sanitize(userInput);
```

**Apply To:**
- Chat messages
- Announcements
- Student/Teacher bios
- Any user-generated HTML content

### 3. SQL Injection Prevention

**Already Protected by EF Core (Parameterized Queries)**

**Safe:**
```csharp
// EF Core automatically parameterizes
var students = await _context.Students
    .Where(s => s.FirstName.Contains(searchTerm))
    .ToListAsync();
```

**Unsafe (NEVER USE):**
```csharp
// RAW SQL - AVOID
var students = await _context.Students
    .FromSqlRaw($"SELECT * FROM Students WHERE FirstName = '{searchTerm}'")
    .ToListAsync();
```

**If Raw SQL is Required:**
```csharp
// Use parameterized raw SQL
var students = await _context.Students
    .FromSqlRaw("SELECT * FROM Students WHERE FirstName = {0}", searchTerm)
    .ToListAsync();
```

### 4. File Upload Validation

#### Profile Picture Upload

**Validation Rules:**
- **File Size**: Max 5 MB
- **File Types**: JPEG, PNG, GIF only
- **Content Validation**: Verify actual file content (magic numbers)
- **Filename Sanitization**: Remove special characters
- **Virus Scanning**: Integrate with antivirus API

**Implementation:**
```csharp
public class FileUploadValidator
{
    private static readonly Dictionary<string, byte[]> FileSignatures = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } }
    };

    public bool ValidateFile(IFormFile file)
    {
        // Check file size
        if (file.Length > 5 * 1024 * 1024) // 5 MB
            return false;

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSignatures.ContainsKey(extension))
            return false;

        // Validate magic numbers (file signature)
        using var reader = new BinaryReader(file.OpenReadStream());
        var signature = FileSignatures[extension];
        var fileHeader = reader.ReadBytes(signature.Length);

        return fileHeader.SequenceEqual(signature);
    }

    public string SanitizeFileName(string fileName)
    {
        // Remove path traversal attempts
        fileName = Path.GetFileName(fileName);

        // Remove special characters
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9\._-]", "");

        // Generate unique name to prevent overwrites
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{extension}";

        return uniqueName;
    }
}
```

---

## Rate Limiting Strategy

### 1. Endpoint-Specific Limits

#### Rate Limit Configuration

| Endpoint Category | Limit | Window | Identifier |
|------------------|-------|--------|------------|
| **Login** | 5 requests | 1 minute | IP Address |
| **Password Reset** | 3 requests | 1 hour | Email + IP |
| **Registration** | 10 requests | 1 hour | IP Address |
| **API Endpoints (Authenticated)** | 100 requests | 1 minute | User ID |
| **Chat Messages** | 30 messages | 1 minute | User ID |
| **File Uploads** | 10 uploads | 1 hour | User ID |
| **Search/Filter** | 60 requests | 1 minute | User ID |
| **Video Call Creation** | 5 calls | 1 hour | User ID |
| **Bulk Operations** | 10 requests | 1 hour | User ID |

### 2. Rate Limit Headers

**Response Headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1704845460
Retry-After: 45
```

**429 Too Many Requests Response:**
```json
{
  "isSuccess": false,
  "data": null,
  "errorMessage": "Rate limit exceeded. Try again in 45 seconds.",
  "retryAfter": 45
}
```

### 3. Distributed Rate Limiting

**For Multi-Instance Deployments:**

**Option 1: Redis-Based (Recommended)**
```
Key Format: "ratelimit:{userId}:{endpoint}:{window}"
Value: Request count
TTL: Window duration

Example:
Key: "ratelimit:user-123:api:20260109-1430"
Value: 45
TTL: 60 seconds
```

**Option 2: SQL Server-Based**
```sql
CREATE TABLE RateLimitTracking (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Identifier NVARCHAR(100) NOT NULL, -- UserId or IP
    Endpoint NVARCHAR(200) NOT NULL,
    WindowStart DATETIME2 NOT NULL,
    RequestCount INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE INDEX IX_RateLimitTracking_Identifier_Endpoint_Window 
    ON RateLimitTracking(Identifier, Endpoint, WindowStart);
```

### 4. Rate Limit Bypass for Testing

**Whitelist IPs:**
```json
{
  "RateLimiting": {
    "WhitelistedIPs": [
      "127.0.0.1",
      "::1",
      "10.0.0.0/8"
    ],
    "WhitelistedUsers": [
      "monitoring-service-account"
    ]
  }
}
```

---

## Audit Logging

### 1. Audit Log Schema

**Database Model:**
```sql
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(100) NOT NULL,
    Resource NVARCHAR(200) NOT NULL,
    ResourceId NVARCHAR(100),
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX),
    RequestMethod NVARCHAR(10),
    RequestPath NVARCHAR(500),
    StatusCode INT,
    Duration INT, -- milliseconds
    AdditionalData NVARCHAR(MAX) -- JSON
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_Resource ON AuditLogs(Resource);
```

### 2. Events to Log

#### Authentication Events

| Event | Action | Resource | Additional Data |
|-------|--------|----------|-----------------|
| Login Success | `AUTH_LOGIN_SUCCESS` | `User` | Role, MFA used |
| Login Failure | `AUTH_LOGIN_FAILURE` | `User` | Failure reason |
| Logout | `AUTH_LOGOUT` | `User` | - |
| Password Reset Request | `AUTH_PASSWORD_RESET_REQUEST` | `User` | - |
| Password Reset Success | `AUTH_PASSWORD_RESET_SUCCESS` | `User` | - |
| MFA Enabled | `AUTH_MFA_ENABLED` | `User` | Method |
| MFA Disabled | `AUTH_MFA_DISABLED` | `User` | - |
| Token Refresh | `AUTH_TOKEN_REFRESH` | `User` | - |
| Account Lockout | `AUTH_ACCOUNT_LOCKOUT` | `User` | Reason |

#### Data Access Events

| Event | Action | Resource | Additional Data |
|-------|--------|----------|-----------------|
| View Student Record | `DATA_VIEW_STUDENT` | `Student` | Student ID |
| View Grade | `DATA_VIEW_GRADE` | `Grade` | Student ID, Subject |
| Export Student Data | `DATA_EXPORT_STUDENT` | `Student` | Student ID, Format |
| Bulk Data Export | `DATA_BULK_EXPORT` | `School` | Record count |
| Search Students | `DATA_SEARCH_STUDENT` | `Student` | Search criteria |

#### Administrative Events

| Event | Action | Resource | Additional Data |
|-------|--------|----------|-----------------|
| Create User | `ADMIN_USER_CREATE` | `User` | New user ID, Role |
| Update User | `ADMIN_USER_UPDATE` | `User` | Changed fields |
| Delete User | `ADMIN_USER_DELETE` | `User` | Deleted user ID |
| Change User Role | `ADMIN_USER_ROLE_CHANGE` | `User` | Old role → New role |
| Grant Permission | `ADMIN_PERMISSION_GRANT` | `User` | Permission granted |
| Revoke Permission | `ADMIN_PERMISSION_REVOKE` | `User` | Permission revoked |

#### Chat & Communication Events

| Event | Action | Resource | Additional Data |
|-------|--------|----------|-----------------|
| Create Chat Room | `CHAT_ROOM_CREATE` | `ChatRoom` | Room ID, Privacy level |
| Join Chat Room | `CHAT_JOIN` | `ChatRoom` | Room ID |
| Leave Chat Room | `CHAT_LEAVE` | `ChatRoom` | Room ID |
| Send Message | `CHAT_MESSAGE_SEND` | `ChatMessage` | Room ID, Encrypted |
| Delete Message | `CHAT_MESSAGE_DELETE` | `ChatMessage` | Message ID, Room ID |
| Download Recording | `VIDEO_RECORDING_DOWNLOAD` | `VideoRecording` | Recording ID |

### 3. Log Retention Policy

**Retention Periods:**
- **Authentication Logs**: 1 year
- **Data Access Logs**: 2 years (FERPA requirement)
- **Administrative Logs**: 3 years
- **Chat Logs**: 1 year (or per school policy)
- **Error Logs**: 90 days

**Archive Strategy:**
```sql
-- Archive old logs to separate table
INSERT INTO AuditLogsArchive
SELECT * FROM AuditLogs
WHERE Timestamp < DATEADD(YEAR, -1, GETUTCDATE());

DELETE FROM AuditLogs
WHERE Timestamp < DATEADD(YEAR, -1, GETUTCDATE());
```

### 4. Log Tampering Prevention

**Append-Only Storage:**
- Logs stored in separate database with read-only access
- Write operations through dedicated service account
- No DELETE or UPDATE permissions for application
- Regular backups to immutable storage (Azure Blob with WORM policy)

**Log Integrity:**
```csharp
// Add hash chain for tamper detection
public class AuditLog
{
    public Guid Id { get; set; }
    public string Action { get; set; }
    // ... other fields
    public string PreviousLogHash { get; set; }
    public string LogHash { get; set; } // SHA256 of current log + previous hash
}

// Compute hash
var logHash = ComputeSHA256(
    $"{log.Id}{log.Action}{log.Timestamp}{previousLogHash}"
);
```

---

## Sensitive Data Protection

### 1. Encryption at Rest

#### Database Encryption

**SQL Server Transparent Data Encryption (TDE):**
```sql
-- Enable TDE on database
USE master;
GO

CREATE MASTER KEY ENCRYPTION BY PASSWORD = '<strong_password>';
GO

CREATE CERTIFICATE TDE_Cert WITH SUBJECT = 'TDE Certificate';
GO

USE SMSDB;
GO

CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE TDE_Cert;
GO

ALTER DATABASE SMSDB
SET ENCRYPTION ON;
GO
```

#### Column-Level Encryption

**For Highly Sensitive Data:**
```csharp
// Encrypt student SSN, DOB, addresses
public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [Encrypted] // Custom attribute
    public string SocialSecurityNumber { get; set; }
    
    [Encrypted]
    public DateTime DateOfBirth { get; set; }
    
    [Encrypted]
    public string HomeAddress { get; set; }
}
```

**Encryption Service:**
```csharp
public interface IDataEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}

// Implementation using AES-256-GCM
public class DataEncryptionService : IDataEncryptionService
{
    private readonly byte[] _masterKey; // From Azure Key Vault

    public string Encrypt(string plaintext)
    {
        using var aes = new AesGcm(_masterKey);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var ciphertext = new byte[plaintext.Length];

        RandomNumberGenerator.Fill(nonce);
        
        aes.Encrypt(
            nonce,
            Encoding.UTF8.GetBytes(plaintext),
            ciphertext,
            tag
        );

        // Return Base64(nonce + tag + ciphertext)
        var combined = nonce.Concat(tag).Concat(ciphertext).ToArray();
        return Convert.ToBase64String(combined);
    }
}
```

### 2. Encryption in Transit

**TLS Configuration:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:7266",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "<certificate_password>"
        }
      }
    }
  }
}
```

**Enforce HTTPS:**
```csharp
// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// HSTS header
app.UseHsts();
```

**Minimum TLS Version:**
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});
```

### 3. Chat Message Encryption

**End-to-End Encryption Flow:**
```
1. User sends message
   ↓
2. Client generates symmetric key (AES-256)
   ↓
3. Encrypt message with symmetric key
   ↓
4. Encrypt symmetric key with recipient's public key (RSA)
   ↓
5. Send to server: { encryptedMessage, encryptedKey }
   ↓
6. Server stores encrypted message (no decryption on server)
   ↓
7. Broadcast to room members
   ↓
8. Each recipient decrypts symmetric key with their private key
   ↓
9. Decrypt message with symmetric key
```

**Server-Side Encryption (Simpler Alternative):**
```csharp
public interface IMessageEncryptionService
{
    string EncryptMessage(string message, Guid roomId);
    string DecryptMessage(string encryptedMessage, Guid roomId);
}

// Use room-specific encryption key
public class MessageEncryptionService : IMessageEncryptionService
{
    public string EncryptMessage(string message, Guid roomId)
    {
        var roomKey = GetRoomKey(roomId); // Retrieve from secure storage
        return AesEncrypt(message, roomKey);
    }
}
```

### 4. Data Masking

#### Mask Sensitive Data in Responses

**Student SSN:**
```csharp
public class StudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Mask SSN: 123-45-6789 → ***-**-6789
    public string SocialSecurityNumber => MaskSSN(_socialSecurityNumber);
    
    private string MaskSSN(string ssn)
    {
        if (string.IsNullOrEmpty(ssn) || ssn.Length < 4)
            return "***-**-****";
        
        return $"***-**-{ssn.Substring(ssn.Length - 4)}";
    }
}
```

**Email Addresses:**
```csharp
// john.doe@sms.edu → j***@sms.edu
public string MaskEmail(string email)
{
    var parts = email.Split('@');
    if (parts.Length != 2 || parts[0].Length < 2)
        return "***@***";
    
    return $"{parts[0][0]}***@{parts[1]}";
}
```

### 5. Secret Management

#### Azure Key Vault Integration

**Configuration:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

**Secrets to Store in Key Vault:**
- JWT signing key
- Database connection strings
- Encryption master keys
- Third-party API keys
- SMTP credentials
- Storage account keys

**Local Development (User Secrets):**
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-256-bit-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

---

## CORS Configuration

### 1. Environment-Specific CORS

**Development:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:5174",
      "http://localhost:3000",
      "http://localhost:8080"
    ]
  }
}
```

**Production:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://sms.edu",
      "https://www.sms.edu",
      "https://app.sms.edu"
    ]
  }
}
```

### 2. CORS Policy Configuration

**Implementation:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(24));
    });

    // Separate policy for admin endpoints
    options.AddPolicy("AdminOnly", policy =>
    {
        policy
            .WithOrigins("https://admin.sms.edu")
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowCredentials();
    });
});
```

### 3. SignalR CORS Configuration

**SignalR-Specific CORS:**
```csharp
app.MapHub<ChatHub>("/chatHub", options =>
{
    options.AllowedOrigins.Add("https://sms.edu");
    options.AllowedOrigins.Add("https://app.sms.edu");
});
```

---

## Security Headers

### 1. Security Headers Configuration

**Headers to Implement:**

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self' wss://sms.edu; font-src 'self'; frame-ancestors 'none'
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: camera=(self "https://sms.edu"), microphone=(self "https://sms.edu"), geolocation=(), payment=()
```

### 2. Content Security Policy (CSP)

**CSP Directives:**

```
Content-Security-Policy:
  default-src 'self';
  script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net;
  style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
  img-src 'self' data: https: blob:;
  font-src 'self' https://fonts.gstatic.com;
  connect-src 'self' wss://sms.edu https://sms.edu;
  media-src 'self' blob:;
  object-src 'none';
  frame-ancestors 'none';
  base-uri 'self';
  form-action 'self';
  upgrade-insecure-requests;
```

**For Video Call Page:**
```
Permissions-Policy:
  camera=(self "https://sms.edu"),
  microphone=(self "https://sms.edu"),
  display-capture=(self "https://sms.edu")
```

---

## SignalR Security

### 1. Hub Authorization

**Authentication Check:**
```csharp
[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Validate JWT token
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort();
            return;
        }

        // Log connection
        await _auditService.LogAsync("CHAT_HUB_CONNECT", "ChatHub", userId);
        
        await base.OnConnectedAsync();
    }
}
```

**Query String Token Support:**
```csharp
// For SignalR clients that can't set headers
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatHub") || path.StartsWithSegments("/videoCallHub")))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });
```

### 2. Room Access Control

**Validate Room Access:**
```csharp
public async Task JoinRoom(Guid roomId, string accessToken)
{
    var userId = GetUserId();
    
    // Validate access token
    if (!await _roomAccessTokenService.ValidateTokenAsync(roomId, userId, accessToken))
    {
        throw new HubException("Invalid room access token");
    }
    
    // Check room privacy
    var room = await _chatRoomService.GetRoomAsync(roomId);
    if (room.PrivacyLevel == "Private" && !await HasRoomAccess(roomId, userId))
    {
        throw new HubException("Access denied to private room");
    }
    
    // Add to SignalR group
    await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
    
    // Log access
    await _auditService.LogAsync("CHAT_ROOM_JOIN", "ChatRoom", roomId.ToString(), userId);
}
```

### 3. Message Rate Limiting

**Flood Protection:**
```csharp
private readonly Dictionary<string, Queue<DateTime>> _userMessageTimes = new();
private const int MaxMessagesPerMinute = 30;

public async Task SendMessage(Guid roomId, string content)
{
    var userId = GetUserId();
    
    // Rate limit check
    if (!_userMessageTimes.ContainsKey(userId))
        _userMessageTimes[userId] = new Queue<DateTime>();
    
    var userMessages = _userMessageTimes[userId];
    var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
    
    // Remove old messages
    while (userMessages.Count > 0 && userMessages.Peek() < oneMinuteAgo)
        userMessages.Dequeue();
    
    if (userMessages.Count >= MaxMessagesPerMinute)
    {
        throw new HubException("Rate limit exceeded. Maximum 30 messages per minute.");
    }
    
    userMessages.Enqueue(DateTime.UtcNow);
    
    // Send message
    // ...
}
```

---

## Compliance Requirements

### 1. FERPA (Family Educational Rights and Privacy Act)

**Requirements:**
- **Consent**: Obtain written consent before disclosing student PII
- **Access Rights**: Students/parents can review and request corrections
- **Access Logs**: Maintain logs of who accessed student records
- **Retention**: Retain records as required by state law
- **Destruction**: Secure deletion when no longer needed

**Implementation:**
```csharp
// Log all student record access
[Authorize]
[AuditLog(Action = "DATA_VIEW_STUDENT")]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _studentService.GetByIdAsync(id);
    
    // FERPA compliance check
    if (!await HasFERPAAccess(id, CurrentUserId))
        return Forbid();
    
    return Ok(student);
}

// Data export with consent verification
public async Task<IActionResult> ExportStudentData(Guid studentId)
{
    // Verify consent
    var consent = await _consentService.GetConsentAsync(studentId, "DATA_EXPORT");
    if (consent == null || consent.ExpiresAt < DateTime.UtcNow)
        return Forbid("Consent required for data export");
    
    var data = await _studentService.ExportDataAsync(studentId);
    return File(data, "application/json", $"student-{studentId}.json");
}
```

### 2. COPPA (Children's Online Privacy Protection Act)

**Requirements for Students Under 13:**
- **Parental Consent**: Obtain verifiable parental consent
- **Data Minimization**: Collect only necessary data
- **No Marketing**: No targeted advertising to children
- **Secure Deletion**: Delete data upon parent request

**Implementation:**
```csharp
public class CreateStudentDto
{
    public DateTime DateOfBirth { get; set; }
    
    // If under 13, require parental consent
    public Guid? ParentalConsentId { get; set; }
}

public class StudentService
{
    public async Task<Student> CreateStudentAsync(CreateStudentDto dto)
    {
        var age = DateTime.Now.Year - dto.DateOfBirth.Year;
        
        if (age < 13 && !dto.ParentalConsentId.HasValue)
        {
            throw new ValidationException("Parental consent required for students under 13");
        }
        
        if (dto.ParentalConsentId.HasValue)
        {
            var consent = await _consentService.ValidateConsentAsync(dto.ParentalConsentId.Value);
            if (!consent.IsValid)
                throw new ValidationException("Invalid parental consent");
        }
        
        // Create student
        // ...
    }
}
```

### 3. GDPR (General Data Protection Regulation)

**Requirements:**
- **Right to Access**: Users can request their data
- **Right to Rectification**: Users can correct their data
- **Right to Erasure**: "Right to be forgotten"
- **Data Portability**: Export data in machine-readable format
- **Breach Notification**: Notify within 72 hours
- **Data Protection Officer**: Designate DPO
- **Privacy by Design**: Implement privacy from the start

**Implementation:**
```csharp
// Right to Access (GDPR Article 15)
[HttpGet("gdpr/my-data")]
public async Task<IActionResult> GetMyData()
{
    var userId = GetUserId();
    var data = await _gdprService.ExportUserDataAsync(userId);
    
    return Ok(new
    {
        user = data.User,
        students = data.Students,
        attendance = data.Attendance,
        messages = data.Messages,
        recordings = data.Recordings
    });
}

// Right to Erasure (GDPR Article 17)
[HttpDelete("gdpr/delete-my-data")]
public async Task<IActionResult> DeleteMyData()
{
    var userId = GetUserId();
    
    // Verify identity (require password re-entry)
    if (!await VerifyIdentity())
        return Unauthorized();
    
    // Anonymize or delete data
    await _gdprService.DeleteUserDataAsync(userId);
    
    // Log deletion
    await _auditService.LogAsync("GDPR_DATA_DELETION", "User", userId);
    
    return NoContent();
}

// Data Portability (GDPR Article 20)
[HttpGet("gdpr/export")]
public async Task<IActionResult> ExportData()
{
    var userId = GetUserId();
    var jsonData = await _gdprService.ExportToJsonAsync(userId);
    
    return File(
        Encoding.UTF8.GetBytes(jsonData),
        "application/json",
        $"gdpr-export-{userId}-{DateTime.UtcNow:yyyyMMdd}.json"
    );
}
```

### 4. Data Retention Policy

**Retention Periods:**
| Data Type | Retention Period | Deletion Method |
|-----------|------------------|-----------------|
| Student Records | 5 years after graduation | Secure deletion |
| Attendance Records | 3 years | Secure deletion |
| Chat Messages | 1 year | Secure deletion |
| Video Recordings | 30 days (or per policy) | Secure deletion + overwrite |
| Audit Logs | 2 years | Archive then delete |
| Authentication Logs | 1 year | Archive then delete |

**Auto-Deletion Implementation:**
```csharp
// Background job to delete old data
public class DataRetentionJob : IHostedService
{
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delete old chat messages (1 year)
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        await _context.ChatMessages
            .Where(m => m.CreatedAt < oneYearAgo)
            .ExecuteDeleteAsync();
        
        // Delete old video recordings (30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var oldRecordings = await _context.VideoRecordings
            .Where(r => r.CreatedAt < thirtyDaysAgo)
            .ToListAsync();
        
        foreach (var recording in oldRecordings)
        {
            // Delete file
            await _videoService.DeleteRecordingAsync(recording.FilePath);
            
            // Delete database record
            _context.VideoRecordings.Remove(recording);
        }
        
        await _context.SaveChangesAsync();
    }
}
```

---

## Summary

This security implementation guide provides comprehensive documentation for securing the School Management System API. All security measures should be implemented following industry best practices and compliance requirements.

**Key Takeaways:**
1. **Defense in Depth**: Multiple layers of security (authentication, authorization, validation, encryption)
2. **Compliance First**: FERPA, COPPA, and GDPR requirements built into design
3. **Audit Everything**: Comprehensive logging of all sensitive operations
4. **Data Protection**: Encryption at rest and in transit for all PII
5. **Zero Trust**: Validate and authorize every request
6. **Incident Response**: Logging and monitoring for security incidents

**Next Steps:**
- Implement security controls as documented
- Conduct security testing (see [SECURITY_TESTING_CHECKLIST.md](SECURITY_TESTING_CHECKLIST.md))
- Perform penetration testing (see [PENETRATION_TESTING.md](PENETRATION_TESTING.md))
- Regular security audits and updates

---

**Related Documentation:**
- [Security Testing Checklist](SECURITY_TESTING_CHECKLIST.md)
- [Penetration Testing Guide](PENETRATION_TESTING.md)
- [Authentication Security](AUTHENTICATION_SECURITY.md)
- [Data Protection Guide](DATA_PROTECTION_GUIDE.md)
