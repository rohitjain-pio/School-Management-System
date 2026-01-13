# Migration Strategy
## Safe Migration to Production Multi-Tenancy

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 12 minutes  
**Status:** 🟡 Ready for Execution

---

## 🎯 **Migration Goals**

1. **Zero downtime** for existing users
2. **Data integrity** maintained throughout process
3. **Rollback capability** if issues arise
4. **Audit trail** of all changes
5. **Testing at each step** before proceeding

---

## 📋 **Pre-Migration Checklist**

```
Environment Preparation:
[ ] Backup current production database
[ ] Verify backup restoration works
[ ] Set up staging environment matching production
[ ] Install SQL Server Management Studio (SSMS)
[ ] Document all existing users and their roles

Code Preparation:
[ ] Complete security implementation (Step 1-4 from 10_SECURITY_IMPLEMENTATION.md)
[ ] All controllers inherit BaseSchoolController
[ ] JWT generation includes SchoolId claim
[ ] Middleware registered in correct order

Testing:
[ ] All unit tests pass
[ ] Security tests written (20+ test cases)
[ ] Integration tests pass on staging
[ ] Load tests show < 200ms response time

Team Preparation:
[ ] Schedule migration during low-traffic period (e.g., Sunday 2 AM)
[ ] Notify all admins 72 hours in advance
[ ] Prepare support team for post-migration issues
[ ] Create rollback plan with time estimates
```

---

## 🗺️ **Migration Phases**

### **Phase 1: Preparation (1 hour)**
- Backup database
- Deploy new code to staging
- Run migration script on staging
- Validate staging environment

### **Phase 2: Maintenance Window (30 minutes)**
- Enable maintenance mode
- Backup production database
- Run migration script
- Deploy new backend code

### **Phase 3: Validation (30 minutes)**
- Smoke tests
- Security tests
- User login tests
- Performance checks

### **Phase 4: Monitoring (24 hours)**
- Watch error logs
- Monitor API response times
- Track user login success rate
- Check audit logs for anomalies

---

## 🔄 **Step-by-Step Migration Process**

### **Step 1: Backup Current Database**

```sql
-- Full backup before migration
BACKUP DATABASE [SchoolManagementDB]
TO DISK = 'C:\Backups\SchoolManagementDB_PreMigration_20260113.bak'
WITH FORMAT,
     NAME = 'Pre-Migration Full Backup',
     DESCRIPTION = 'Full backup before multi-tenant migration',
     COMPRESSION,
     STATS = 10;

-- Verify backup
RESTORE VERIFYONLY
FROM DISK = 'C:\Backups\SchoolManagementDB_PreMigration_20260113.bak';
```

**Export to Azure Blob Storage (recommended):**

```powershell
# Upload backup to cloud storage
az storage blob upload `
    --account-name schoolmsstorage `
    --container-name backups `
    --file "C:\Backups\SchoolManagementDB_PreMigration_20260113.bak" `
    --name "SchoolManagementDB_PreMigration_20260113.bak"
```

---

### **Step 2: Document Current State**

```sql
-- Count existing records
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM AspNetUsers
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'Teachers', COUNT(*) FROM Teachers
UNION ALL
SELECT 'Classes', COUNT(*) FROM Classes
UNION ALL
SELECT 'Subjects', COUNT(*) FROM Subjects
UNION ALL
SELECT 'Attendance', COUNT(*) FROM Attendance
UNION ALL
SELECT 'Grades', COUNT(*) FROM Grades;

-- Export to CSV for reference
bcp "SELECT * FROM AspNetUsers" queryout "C:\Migration\users_before.csv" -c -t"," -S localhost -d SchoolManagementDB -T

-- Check for orphaned records
SELECT 'Students without UserId' as Issue, COUNT(*) as Count
FROM Students WHERE UserId NOT IN (SELECT Id FROM AspNetUsers)
UNION ALL
SELECT 'Teachers without UserId', COUNT(*)
FROM Teachers WHERE UserId NOT IN (SELECT Id FROM AspNetUsers);
```

---

### **Step 3: Enable Maintenance Mode**

**Create maintenance page:**

```html
<!-- Frontend/public/maintenance.html -->
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Maintenance - School Management System</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }
        .container {
            text-align: center;
            padding: 2rem;
        }
        h1 { font-size: 3rem; margin-bottom: 1rem; }
        p { font-size: 1.2rem; margin-bottom: 2rem; }
        .eta { font-size: 2rem; font-weight: bold; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🔧 System Maintenance</h1>
        <p>We're upgrading to make your experience even better!</p>
        <p class="eta">Estimated completion: <span id="eta">2:30 AM</span></p>
        <p>Thank you for your patience.</p>
    </div>
</body>
</html>
```

**Update nginx configuration:**

```nginx
# Frontend/nginx-maintenance.conf
server {
    listen 80;
    server_name schoolms.com;

    location / {
        root /usr/share/nginx/html;
        try_files /maintenance.html =503;
    }

    # Return 503 for API requests
    location /api {
        return 503 '{"error": "System under maintenance", "message": "Please try again in 30 minutes"}';
        add_header Content-Type application/json;
    }
}
```

**Deploy maintenance mode:**

```bash
# Replace nginx config
docker exec frontend cp /etc/nginx/nginx-maintenance.conf /etc/nginx/conf.d/default.conf
docker exec frontend nginx -s reload
```

---

### **Step 4: Run Migration Script**

**Execute with transaction safety:**

```powershell
# Run migration script from Backend directory
cd Backend

# Execute with error handling
sqlcmd -S localhost -d SchoolManagementDB -i DatabaseMigration_SchoolIsolation.sql -o migration_log.txt

# Check for errors in log
Get-Content migration_log.txt | Select-String -Pattern "ERROR|FAILED"

# If no errors, verify results
sqlcmd -S localhost -d SchoolManagementDB -Q "
SELECT 
    (SELECT COUNT(*) FROM Schools) as Schools,
    (SELECT COUNT(*) FROM AspNetUsers WHERE SchoolId IS NOT NULL) as UsersWithSchool,
    (SELECT COUNT(*) FROM Students WHERE SchoolId IS NOT NULL) as StudentsWithSchool,
    (SELECT COUNT(*) FROM Teachers WHERE SchoolId IS NOT NULL) as TeachersWithSchool
"
```

**Expected Output:**
```
Schools | UsersWithSchool | StudentsWithSchool | TeachersWithSchool
--------|-----------------|-------------------|-------------------
1       | 45              | 150               | 12
```

---

### **Step 5: Deploy New Backend Code**

```bash
# Build new Docker image with SchoolIsolationMiddleware
cd Backend
docker build -t schoolms-backend:v2.0.0 .

# Stop old container
docker stop backend

# Start new container
docker run -d \
  --name backend \
  -p 7266:80 \
  -e ConnectionStrings__DefaultConnection="Server=db;Database=SchoolManagementDB;..." \
  -e JWT__Secret="your-secret-key" \
  --network schoolms-network \
  schoolms-backend:v2.0.0

# Check logs for startup errors
docker logs backend --tail 50
```

---

### **Step 6: Smoke Tests**

**Test 1: Health Check**

```bash
curl http://localhost:7266/api/health
# Expected: 200 OK
```

**Test 2: Login (Existing User)**

```bash
curl -X POST http://localhost:7266/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@school.com",
    "password": "Admin@123"
  }'

# Expected: 200 OK with JWT token containing SchoolId claim
```

**Test 3: Get Students (With SchoolId)**

```bash
# Decode JWT to get SchoolId
TOKEN="eyJhbGc..."

curl http://localhost:7266/api/students \
  -H "Authorization: Bearer $TOKEN"

# Expected: 200 OK with students from user's school only
```

**Test 4: Cross-School Access (Should Fail)**

```bash
# Try to access student from different school
curl http://localhost:7266/api/students/{student-id-from-different-school} \
  -H "Authorization: Bearer $TOKEN"

# Expected: 403 Forbidden with "different school" message
```

---

### **Step 7: Data Validation**

```sql
-- Verify no data loss
SELECT 'Users' as TableName, COUNT(*) as AfterCount, 
       (SELECT COUNT(*) FROM AspNetUsers_Backup) as BeforeCount
FROM AspNetUsers
UNION ALL
SELECT 'Students', COUNT(*), (SELECT COUNT(*) FROM Students_Backup)
FROM Students;

-- Check SchoolId distribution
SELECT 
    s.Name as SchoolName,
    COUNT(DISTINCT u.Id) as UserCount,
    COUNT(DISTINCT st.Id) as StudentCount,
    COUNT(DISTINCT t.Id) as TeacherCount
FROM Schools s
LEFT JOIN AspNetUsers u ON s.Id = u.SchoolId
LEFT JOIN Students st ON s.Id = st.SchoolId
LEFT JOIN Teachers t ON s.Id = t.SchoolId
GROUP BY s.Name;

-- Check for orphaned records (should be 0)
SELECT 'Students without School' as Issue, COUNT(*) as Count
FROM Students WHERE SchoolId NOT IN (SELECT Id FROM Schools)
UNION ALL
SELECT 'Users without School', COUNT(*)
FROM AspNetUsers WHERE SchoolId NOT IN (SELECT Id FROM Schools);
```

---

### **Step 8: Performance Testing**

**Run k6 load test:**

```javascript
// migration-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '1m', target: 50 },   // Ramp up to 50 users
    { duration: '3m', target: 50 },   // Stay at 50 users
    { duration: '1m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% of requests < 500ms
  },
};

export default function () {
  // Login
  let loginRes = http.post('http://localhost:7266/api/auth/login', JSON.stringify({
    email: 'admin@school.com',
    password: 'Admin@123'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  check(loginRes, {
    'login successful': (r) => r.status === 200,
    'has token': (r) => r.json('token') !== undefined,
  });

  let token = loginRes.json('token');

  // Get students
  let studentsRes = http.get('http://localhost:7266/api/students', {
    headers: { 'Authorization': `Bearer ${token}` },
  });

  check(studentsRes, {
    'students retrieved': (r) => r.status === 200,
    'response time OK': (r) => r.timings.duration < 200,
  });

  sleep(1);
}
```

**Execute test:**

```bash
k6 run migration-load-test.js

# Expected output:
# http_req_duration..........: avg=145ms  min=50ms  med=120ms  max=450ms  p(95)=320ms
# ✓ All checks passed
```

---

### **Step 9: Disable Maintenance Mode**

```bash
# Restore normal nginx config
docker exec frontend cp /etc/nginx/nginx.conf.backup /etc/nginx/conf.d/default.conf
docker exec frontend nginx -s reload

# Verify frontend is accessible
curl http://localhost:3000
# Expected: React app HTML
```

---

### **Step 10: Post-Migration Monitoring**

**Monitor for 24 hours:**

```sql
-- Check login success rate (should be > 95%)
SELECT 
    COUNT(*) as TotalLogins,
    SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as SuccessfulLogins,
    (SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) as SuccessRate
FROM LoginAttempts
WHERE Timestamp > DATEADD(HOUR, -24, GETUTCDATE());

-- Check for security violations
SELECT COUNT(*) as CrossSchoolAttempts
FROM AuditLogs
WHERE Action LIKE '%cross-school%'
  AND Timestamp > DATEADD(HOUR, -24, GETUTCDATE());
-- Expected: 0

-- Check API error rate (should be < 1%)
SELECT 
    COUNT(*) as TotalRequests,
    SUM(CASE WHEN StatusCode >= 500 THEN 1 ELSE 0 END) as ServerErrors,
    (SUM(CASE WHEN StatusCode >= 500 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) as ErrorRate
FROM RequestLogs
WHERE Timestamp > DATEADD(HOUR, -24, GETUTCDATE());
```

**Application Insights query:**

```kusto
requests
| where timestamp > ago(24h)
| summarize 
    TotalRequests = count(),
    AvgDuration = avg(duration),
    P95Duration = percentile(duration, 95),
    ErrorRate = countif(success == false) * 100.0 / count()
| project TotalRequests, AvgDuration, P95Duration, ErrorRate
```

---

## 🔙 **Rollback Plan**

**If critical issues arise within 1 hour of migration:**

### **Quick Rollback (10 minutes)**

```bash
# 1. Stop new backend
docker stop backend

# 2. Start old backend
docker start backend-v1.9.0

# 3. Restore database from backup
sqlcmd -S localhost -Q "
USE master;
ALTER DATABASE SchoolManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE SchoolManagementDB 
FROM DISK = 'C:\Backups\SchoolManagementDB_PreMigration_20260113.bak'
WITH REPLACE, RECOVERY;
ALTER DATABASE SchoolManagementDB SET MULTI_USER;
"

# 4. Verify old version works
curl http://localhost:7266/api/health
curl -X POST http://localhost:7266/api/auth/login -d '{"email":"admin@school.com","password":"Admin@123"}'

# 5. Notify users
echo "System has been restored to previous version. Migration will be rescheduled."
```

**If issues discovered after 1 hour (data migration needed):**

### **Data-Only Rollback (30 minutes)**

```sql
-- Remove SchoolId constraints
BEGIN TRANSACTION;

-- Drop foreign keys
ALTER TABLE Students DROP CONSTRAINT FK_Students_Schools;
ALTER TABLE Teachers DROP CONSTRAINT FK_Teachers_Schools;
ALTER TABLE AspNetUsers DROP CONSTRAINT FK_AspNetUsers_Schools;
-- ... (all other tables)

-- Make SchoolId nullable
ALTER TABLE Students ALTER COLUMN SchoolId UNIQUEIDENTIFIER NULL;
ALTER TABLE Teachers ALTER COLUMN SchoolId UNIQUEIDENTIFIER NULL;
-- ... (all other tables)

COMMIT TRANSACTION;

-- Redeploy old backend code
docker start backend-v1.9.0
```

---

## 📊 **Migration Timeline**

```
Sunday, January 19, 2026

1:00 AM - Pre-checks
  ├─ Verify backup exists
  ├─ Check staging environment
  └─ Notify on-call team

1:30 AM - Enable maintenance mode
  ├─ Switch nginx to maintenance.html
  ├─ Send email to admins
  └─ Post notice on status page

2:00 AM - Database migration
  ├─ Backup production DB (5 min)
  ├─ Run migration script (10 min)
  └─ Verify data integrity (5 min)

2:20 AM - Deploy new code
  ├─ Build Docker image (3 min)
  ├─ Deploy container (2 min)
  └─ Check startup logs (2 min)

2:30 AM - Testing
  ├─ Smoke tests (5 min)
  ├─ Security tests (5 min)
  ├─ Performance tests (10 min)
  └─ User acceptance test (10 min)

3:00 AM - Go live
  ├─ Disable maintenance mode
  ├─ Monitor error logs
  └─ Watch dashboard metrics

3:00 AM - 6:00 AM - Close monitoring
  └─ 15-minute intervals checking logs

6:00 AM onwards - 24-hour monitoring
  └─ Hourly checks for anomalies
```

---

## 🚨 **Common Migration Issues**

### **Issue 1: Login fails with "Invalid SchoolId"**

**Cause:** User has Guid.Empty SchoolId

**Solution:**
```sql
-- Find affected users
SELECT Id, Email, FirstName, LastName, SchoolId
FROM AspNetUsers
WHERE SchoolId = '00000000-0000-0000-0000-000000000000'
   OR SchoolId IS NULL;

-- Assign to default school
UPDATE AspNetUsers
SET SchoolId = (SELECT Id FROM Schools WHERE Code = 'DEFAULT_MIGRATION')
WHERE SchoolId = '00000000-0000-0000-0000-000000000000'
   OR SchoolId IS NULL;
```

### **Issue 2: Existing students/teachers not visible**

**Cause:** SchoolId mismatch between user and their data

**Solution:**
```sql
-- Sync student SchoolId with their user
UPDATE s
SET s.SchoolId = u.SchoolId
FROM Students s
INNER JOIN AspNetUsers u ON s.UserId = u.Id
WHERE s.SchoolId != u.SchoolId OR s.SchoolId IS NULL;

-- Same for teachers
UPDATE t
SET t.SchoolId = u.SchoolId
FROM Teachers t
INNER JOIN AspNetUsers u ON t.UserId = u.Id
WHERE t.SchoolId != u.SchoolId OR t.SchoolId IS NULL;
```

### **Issue 3: Performance degradation**

**Cause:** Missing indexes on SchoolId columns

**Solution:**
```sql
-- Rebuild indexes
ALTER INDEX IX_Students_SchoolId ON Students REBUILD;
ALTER INDEX IX_Teachers_SchoolId ON Teachers REBUILD;
ALTER INDEX IX_Attendance_SchoolId_Date ON Attendance REBUILD;

-- Update statistics
UPDATE STATISTICS Students;
UPDATE STATISTICS Teachers;
UPDATE STATISTICS Attendance;
```

---

## ✅ **Post-Migration Checklist**

```
Day 1 (Migration Day):
[ ] All smoke tests passed
[ ] No critical errors in logs
[ ] Users can log in successfully
[ ] Response times < 200ms
[ ] No cross-school access attempts logged

Day 2-7 (First Week):
[ ] Monitor daily login success rate (should be > 98%)
[ ] Check audit logs for anomalies
[ ] Review Application Insights dashboard
[ ] Collect user feedback
[ ] Performance metrics stable

Week 2-4 (First Month):
[ ] Verify all schools are properly isolated
[ ] Test SuperAdmin workflows
[ ] Run security penetration tests
[ ] Optimize slow queries
[ ] Document lessons learned
```

---

## 📚 **Next Steps**

1. **Testing Strategy:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)
2. **Monitoring Setup:** [17_MONITORING_LOGGING.md](./17_MONITORING_LOGGING.md)
3. **Security Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)

---

**Document Status:** ✅ Complete  
**Tested:** 🟡 Staging Only  
**Priority:** Execute before production launch