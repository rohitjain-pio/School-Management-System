# AI Decision Authority Matrix

## What AI Can Decide Autonomously ✅

### Code Generation & Implementation
- **Create new controllers** following BaseSchoolController pattern
- **Create new services** following repository pattern
- **Create new repositories** with SchoolId filtering
- **Add new DTOs** with validation rules
- **Write unit tests** for new features
- **Add FluentValidation validators** for DTOs
- **Create database migrations** (after reviewing schema changes)
- **Add logging statements** with structured logging
- **Refactor code** for better readability (without changing logic)
- **Add XML documentation comments** to public methods

### Bug Fixes
- **Fix compilation errors** (syntax, missing imports)
- **Fix null reference exceptions** (add null checks)
- **Fix async/await issues** (add await, Task return types)
- **Fix LINQ query issues** (correct syntax)
- **Add missing SchoolId filters** to queries (CRITICAL security fix)

### Documentation
- **Update code comments** for clarity
- **Add README sections** for new features
- **Document API endpoints** in Swagger comments
- **Add usage examples** in comments

### Testing
- **Run existing tests** to verify changes
- **Add test cases** for new methods
- **Fix failing tests** (if cause is obvious)

---

## What AI Should Ask First ⚠️

### Architecture Changes
- **Changing database schema** (adding/removing tables or columns)
  - Ask: "I need to add a new column 'X' to table 'Y'. Should I proceed?"
- **Adding new third-party packages** (NuGet or npm)
  - Ask: "I recommend adding package 'X' for feature 'Y'. Approve?"
- **Changing authentication flow**
  - Ask: "This requires modifying JWT token structure. Confirm?"
- **Modifying API routes** (breaking changes)
  - Ask: "This will change the endpoint URL. Frontend needs update too. Proceed?"

### Business Logic
- **Changing fee calculation logic**
  - Ask: "Current logic does X. Should I change to Y?"
- **Modifying grade calculation**
  - Ask: "New formula would be X. Correct?"
- **Changing user permissions** (role access)
  - Ask: "Should 'Teacher' role have access to X?"

### Data Operations
- **Database seeding** with production-like data
  - Ask: "Ready to seed database with test schools. Confirm?"
- **Running migrations on production**
  - Ask: "Migration ready. Backup database first?"
- **Bulk data updates/deletes**
  - Ask: "This will affect X records. Verify first?"

### Performance Impact
- **Adding expensive operations** (N+1 queries, large loops)
  - Ask: "This might impact performance. Should I optimize differently?"
- **Caching strategy changes**
  - Ask: "Should I cache this data? For how long?"

---

## What AI Should NEVER Do ❌

### Security Compromises
- **Remove SchoolId filters** from queries
- **Bypass authentication** checks
- **Hardcode passwords or secrets** in code
- **Disable HTTPS** in production
- **Allow cross-school data access** without audit logging

### Data Loss Risks
- **Drop tables** without explicit instruction
- **Delete production data** without backup confirmation
- **Truncate tables** without approval
- **Disable foreign key constraints**

### Breaking Changes
- **Change primary keys** (Guid → Int)
- **Rename database tables** without migration plan
- **Remove API endpoints** that frontend uses
- **Change DTO property names** (breaks API contracts)

### Deployment Actions
- **Deploy to production** without approval
- **Modify Azure resources** (scaling, pricing tiers)
- **Change connection strings** in production
- **Restart production services**

### Financial Operations
- **Modify payment amounts** or fee structures
- **Refund payments** without approval
- **Change Razorpay configuration**

---

## Decision Framework

### ✅ Low Risk = Proceed Autonomously
- Change affects only development environment
- Code is reversible via Git
- No data loss risk
- No security implications
- Follows established patterns

**Examples:**
- Adding a new API endpoint (following BaseSchoolController)
- Writing unit tests
- Refactoring for readability
- Adding logging statements

---

### ⚠️ Medium Risk = Ask First
- Change affects architecture
- Impacts multiple components
- Requires frontend coordination
- Performance implications
- Business logic changes

**Examples:**
- Adding a new database table
- Changing API response format
- Modifying authentication flow
- Adding a new third-party service

---

### ❌ High Risk = NEVER Without Explicit Approval
- Production environment changes
- Data deletion/modification
- Security-related changes
- Breaking changes to public APIs
- Financial calculations

**Examples:**
- Running migrations on production
- Deleting records
- Changing fee calculation
- Deploying to Azure

---

## Communication Protocol

### When Asking for Approval

**Format:**
```
🤔 DECISION REQUIRED

**Action:** [What you want to do]
**Reason:** [Why it's needed]
**Impact:** [What will change]
**Risk:** [Potential issues]
**Alternative:** [Other options, if any]

**Recommendation:** [Your suggestion]

Please confirm to proceed.
```

**Example:**
```
🤔 DECISION REQUIRED

**Action:** Add "MiddleName" column to Students table
**Reason:** Many students have middle names that aren't being captured
**Impact:** 
  - Database schema change (migration required)
  - DTO updates needed
  - Frontend form needs new field
**Risk:** 
  - Migration might fail on production if Students table is locked
  - Existing frontend will ignore this field (backward compatible)
**Alternative:** 
  - Use "FirstName" field to include middle names (FirstName = "John Michael")

**Recommendation:** Add the column. It's backward compatible and improves data accuracy.

Please confirm to proceed.
```

---

## Error Recovery Decision Tree

### If Compilation Fails
1. **Check for missing usings** → Add automatically ✅
2. **Check for typos** → Fix automatically ✅
3. **Check for breaking API changes** → Ask user ⚠️

### If Tests Fail
1. **If test is outdated** (code changed) → Update test ✅
2. **If test reveals bug** → Fix bug ✅
3. **If test logic is unclear** → Ask user ⚠️

### If Migration Fails
1. **If syntax error** → Fix and retry ✅
2. **If foreign key constraint violation** → Ask user ⚠️
3. **If data loss risk** → STOP and ask ❌

### If Runtime Error
1. **If null reference** → Add null check ✅
2. **If wrong data type** → Fix conversion ✅
3. **If SchoolId missing** → CRITICAL - Ask user ❌

---

## Trust But Verify

### AI Should Self-Validate
After making changes, AI should automatically:

1. **Compile the code** → Ensure no errors
2. **Run affected tests** → Verify functionality
3. **Check for SchoolId filters** → Security audit
4. **Review logs** → Ensure structured logging used
5. **Scan for hardcoded values** → No secrets in code

If validation fails, AI should:
- Fix automatically (if low risk)
- Report issue and ask for guidance (if uncertain)
- Roll back changes (if high risk)

---

## Example Scenarios

### Scenario 1: Adding a New Property
**User Request:** "Add phone number to Student model"

**AI Decision:** ✅ Proceed autonomously
**Reasoning:** Low risk, follows existing patterns

**Actions:**
1. Add PhoneNumber property to Student entity
2. Create migration
3. Update StudentDto
4. Add validation rule (phone format)
5. Update create/update methods
6. Write unit tests

**Report Back:**
```
✅ COMPLETED: Added PhoneNumber to Student model

Changes:
- Added PhoneNumber (string, max 15 chars) to Student entity
- Created migration: Add_PhoneNumber_To_Students
- Updated DTOs and validation
- Added 3 unit tests

Ready to run migration: `dotnet ef database update`
```

---

### Scenario 2: Changing Fee Calculation
**User Request:** "Change late fee from 5% to 10%"

**AI Decision:** ⚠️ Ask first
**Reasoning:** Business logic change, financial impact

**Response:**
```
🤔 DECISION REQUIRED

**Action:** Increase late fee from 5% to 10%
**Impact:** Students paying late will be charged double the current penalty
**Location:** FeeCalculationService.cs, line 45
**Affected:** All schools using the platform

**Questions:**
1. Should this apply to all schools or configurable per school?
2. Should existing unpaid invoices use old rate or new rate?
3. Effective date: immediate or next billing cycle?

Please clarify before I proceed.
```

---

### Scenario 3: Database Migration Error
**AI Action:** Running migration

**Error:** Foreign key constraint violation

**AI Decision:** ❌ STOP and report

**Response:**
```
❌ MIGRATION FAILED

**Error:** Foreign key constraint violation
**Details:** Cannot add SchoolId to ChatMessages table because 45 existing messages have NULL SchoolId

**Options:**
1. Delete orphaned messages (45 records) - DATA LOSS
2. Assign orphaned messages to a default school
3. Keep messages but mark as system-level (SchoolId = Guid.Empty)

I cannot proceed without your decision. Which option?
```

---

## Learning from User Feedback

When user corrects AI decisions:
- **Log the correction** in `.copilot/memory/learned-patterns.md`
- **Update decision matrix** if needed
- **Adjust autonomy level** for similar future scenarios

**Example Entry:**
```markdown
## 2026-01-15: Late Fee Calculation Change

**What I did:** Asked user before changing from 5% to 10%
**User feedback:** "Good call to ask, but in future, business logic changes like this should ALWAYS be configurable per school, not hardcoded"
**Lesson learned:** Never hardcode business calculations. Always use database configuration or settings table
**Updated rule:** Add to "Never Do" list - Hardcoding business calculations
```
