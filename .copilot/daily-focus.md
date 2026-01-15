# Daily Focus - January 15, 2026
**Week 1, Day 3 - Database Migration & Security Foundation**

## 🎯 Today's Mission
**Implement SchoolId isolation across the entire database and codebase**

This is the MOST CRITICAL security feature. Without it, schools can access each other's data.

---

## ⏰ Time-Boxed Tasks (9 hours total)

### Morning Session (3 hours) - Database Migration

#### Task 1: Pre-Migration Checks (30 min)
```sql
-- Verify current database state
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check existing SchoolId columns
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'SchoolId';
```

#### Task 2: Database Backup (15 min)
```bash
# Create backup before migration
sqlcmd -S localhost -d SMSDatabase -Q "BACKUP DATABASE SMSDatabase TO DISK = 'C:\Backups\SMSDatabase_PreMigration_Jan15.bak'"
```

#### Task 3: Execute Migration Script (1 hour)
```bash
# Run migration script
cd Backend
sqlcmd -S localhost -U sa -P YourPassword -d SMSDatabase -i DatabaseMigration_SchoolIsolation.sql

# Verify SchoolId columns added
# Expected: 30+ tables should have SchoolId column
```

#### Task 4: Create Test School (30 min)
```sql
-- Insert test school
INSERT INTO Schools (Id, SchoolName, ContactEmail, IsActive)
VALUES (NEWID(), 'Test School Alpha', 'admin@testschool.com', 1);

-- Note the SchoolId for testing
-- Example: 12345678-1234-1234-1234-123456789abc
```

#### Task 5: Verify Indexes (45 min)
```sql
-- Check indexes on SchoolId columns
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE '%SchoolId%'
ORDER BY t.name;
```

---

### Afternoon Session (4 hours) - Code Implementation

#### Task 6: Create SchoolIsolationMiddleware (1 hour)
**File:** `Backend/SMSPrototype1/Middleware/SchoolIsolationMiddleware.cs`

**Reference:** `docs/production-architecture/10_SECURITY_IMPLEMENTATION.md` (Step 1)

**Key Points:**
- Extract SchoolId claim from JWT token
- Validate SchoolId exists and is valid Guid
- Allow SuperAdmin bypass (with logging)
- Reject requests with missing/invalid SchoolId
- Exempt auth endpoints from validation

**Test Criteria:**
- [ ] Request with valid SchoolId → Pass through
- [ ] Request without SchoolId → Return 403 Forbidden
- [ ] SuperAdmin request → Pass through + log warning
- [ ] Login endpoint → Exempt from check

#### Task 7: Create BaseSchoolController (1 hour)
**File:** `Backend/SMSPrototype1/Controllers/BaseSchoolController.cs`

**Key Methods:**
```csharp
protected Guid GetSchoolIdFromClaims()
protected async Task<bool> ValidateSchoolOwnership<T>(Guid entityId)
protected IActionResult ForbidSchoolAccess()
```

**Test Criteria:**
- [ ] GetSchoolIdFromClaims() returns correct Guid
- [ ] ValidateSchoolOwnership() blocks cross-school access
- [ ] ForbidSchoolAccess() returns 403 with clear message

#### Task 8: Register Middleware (30 min)
**File:** `Backend/SMSPrototype1/Program.cs`

**Code:**
```csharp
// After app.UseAuthentication()
app.UseMiddleware<SchoolIsolationMiddleware>();
```

**Test Criteria:**
- [ ] Middleware runs after authentication
- [ ] Middleware runs before authorization
- [ ] Application compiles successfully

#### Task 9: Update AuthService JWT Claims (1.5 hours)
**File:** `Backend/SMSServices/Services/AuthService.cs`

**Add to JWT:**
```csharp
authClaims.Add(new Claim("SchoolId", user.SchoolId.ToString()));
```

**Test Criteria:**
- [ ] Login response includes SchoolId in token
- [ ] Token decoded shows SchoolId claim
- [ ] Refresh token preserves SchoolId claim

---

### Evening Session (2 hours) - Testing

#### Task 10: Manual API Testing (1 hour)
**Use Postman:**

1. **Login as SchoolAdmin:**
```http
POST /api/auth/login
{
  "email": "admin@testschool.com",
  "password": "Test@123"
}

Response: {
  "token": "eyJhbGc...",
  "schoolId": "12345678-1234-1234-1234-123456789abc"
}
```

2. **Get Students (Should Work):**
```http
GET /api/student
Authorization: Bearer eyJhbGc...

Expected: List of students from Test School Alpha only
```

3. **Try Cross-School Access (Should Fail):**
```http
GET /api/student/{studentId-from-different-school}
Authorization: Bearer eyJhbGc...

Expected: 403 Forbidden with message "Access denied: Resource belongs to different school"
```

#### Task 11: Write Unit Tests (1 hour)
**File:** `Backend/SMSPrototype1.Tests/Middleware/SchoolIsolationMiddlewareTests.cs`

```csharp
[Fact]
public async Task InvokeAsync_WithValidSchoolId_PassesThrough()

[Fact]
public async Task InvokeAsync_WithoutSchoolId_Returns403()

[Fact]
public async Task InvokeAsync_SuperAdmin_AllowsWithLogging()

[Fact]
public async Task InvokeAsync_AuthEndpoint_Exempted()
```

---

## ✅ Definition of Done

Today is complete when ALL of these are true:

- [ ] Database migration script executed successfully
- [ ] All 30+ tables have SchoolId column
- [ ] Indexes created on all SchoolId columns
- [ ] Test school created in database
- [ ] SchoolIsolationMiddleware.cs created and registered
- [ ] BaseSchoolController.cs created
- [ ] JWT token includes SchoolId claim
- [ ] Postman tests pass (valid access works, cross-school blocked)
- [ ] Unit tests written (4+ tests)
- [ ] All tests passing
- [ ] Application compiles without errors
- [ ] No console errors in logs

---

## 🚨 Blockers / Questions

### Known Issues
1. **Issue:** Some controllers might already have partial SchoolId logic
   **Resolution:** Review each controller individually, consolidate in BaseSchoolController

2. **Issue:** SuperAdmin needs different access pattern
   **Resolution:** Check role claim before SchoolId validation in middleware

### Questions to Resolve
- [ ] Should SuperAdmin bypass ALL school isolation checks?
  - Answer: Yes, but with mandatory audit logging
  
- [ ] What happens if user belongs to multiple schools (future feature)?
  - Answer: MVP assumes one school per user (except Parents)

---

## 📊 Progress Tracking

**Start Time:** 9:00 AM  
**Target End Time:** 6:00 PM  
**Actual End Time:** _____  

**Morning Session:**
- Task 1: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 2: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 3: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 4: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 5: ⏱️ Start: _____ | End: _____ | ✅/❌

**Afternoon Session:**
- Task 6: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 7: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 8: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 9: ⏱️ Start: _____ | End: _____ | ✅/❌

**Evening Session:**
- Task 10: ⏱️ Start: _____ | End: _____ | ✅/❌
- Task 11: ⏱️ Start: _____ | End: _____ | ✅/❌

---

## 🔗 Resources Needed

**Documentation:**
- `docs/production-architecture/10_SECURITY_IMPLEMENTATION.md`
- `docs/production-architecture/02_MULTI_TENANCY_DESIGN.md`

**Code References:**
- Existing middleware: `Backend/SMSPrototype1/Middleware`
- Existing controllers: `Backend/SMSPrototype1/Controllers`
