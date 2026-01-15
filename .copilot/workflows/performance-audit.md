# Workflow: Performance Audit
**Estimated Time:** 30-45 minutes  
**Frequency:** Weekly or before major releases  
**Impact:** High (89% improvement history)

---

## 📋 Overview

Systematic performance review to identify bottlenecks and optimization opportunities.

**Tools Required:**
- SQL Server Profiler
- Application Insights (Azure)
- k6 load testing tool
- Visual Studio Diagnostic Tools
- Chrome DevTools (Frontend)

---

## ⏱️ Time-Boxed Steps

### Step 1: Database Query Analysis (10 minutes)

**Tool:** SQL Server Profiler

**Start Profiler Session:**
```powershell
# Run typical user workflows while profiling
# Monitor for:
# - Queries > 200ms
# - N+1 query patterns
# - Missing indexes
# - Table scans
```

**Checklist:**
- [ ] All queries < 200ms response time
- [ ] No queries with > 10,000 rows without pagination
- [ ] No N+1 patterns (multiple queries in loop)
- [ ] No table scans on large tables (> 10,000 rows)
- [ ] All SchoolId queries use indexes

**Common Issues:**

**Issue 1: Missing Indexes**
```sql
-- SQL Profiler shows table scan
-- Check for missing index recommendation
SELECT 
    migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
    'CREATE INDEX IX_' + OBJECT_NAME(mid.object_id) + '_' + 
        REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns,''), ', ', '_'), '[', ''), ']', '') +
        '_' + REPLACE(REPLACE(REPLACE(ISNULL(mid.inequality_columns,''), ', ', '_'), '[', ''), ']', '') +
    ' ON ' + mid.statement + 
    ' (' + ISNULL(mid.equality_columns,'') + 
    CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END +
    ISNULL(mid.inequality_columns, '') + ')' +
    ISNULL(' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement
FROM sys.dm_db_missing_index_groups mig
INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) > 10
ORDER BY improvement_measure DESC;
```

**Fix Pattern:**
```csharp
// Add composite index
migrationBuilder.CreateIndex(
    name: "IX_Students_SchoolId_LastName_FirstName",
    table: "Students",
    columns: new[] { "SchoolId", "LastName", "FirstName" });
```

**Issue 2: N+1 Queries**
```csharp
// ❌ WRONG - N+1 problem
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync();

foreach (var student in students)
{
    // Each iteration = 1 query!
    var enrollments = await _context.Enrollments
        .Where(e => e.StudentId == student.Id)
        .ToListAsync();
}
// Result: 1 + N queries

// ✅ CORRECT - Eager loading
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .Include(s => s.Enrollments)
    .ToListAsync();
// Result: 1 query
```

**Issue 3: Fetching Too Much Data**
```csharp
// ❌ WRONG - Fetching entire entity
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync();
// Returns all columns

// ✅ CORRECT - Projection (Select)
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .Select(s => new StudentListDto 
    {
        Id = s.Id,
        Name = $"{s.FirstName} {s.LastName}",
        Email = s.Email
    })
    .ToListAsync();
// Returns only needed columns (70% less data)
```

---

### Step 2: API Endpoint Analysis (10 minutes)

**Tool:** Application Insights or k6 load testing

**Run Load Test:**
```javascript
// Save as: Backend/performance-tests/load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },  // Ramp up to 50 users
    { duration: '1m', target: 50 },   // Stay at 50 users
    { duration: '30s', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests < 500ms
  },
};

const BASE_URL = 'https://localhost:7001/api';

export default function () {
  // Login
  const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: 'admin@school-test.com',
    password: 'Test123!'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  const token = loginRes.json('token');

  // Get students list
  const studentsRes = http.get(`${BASE_URL}/students?pageNumber=1&pageSize=50`, {
    headers: { 'Authorization': `Bearer ${token}` },
  });

  check(studentsRes, {
    'students status 200': (r) => r.status === 200,
    'students < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

**Run Test:**
```powershell
cd Backend/performance-tests
k6 run load-test.js
```

**Analyze Results:**
```
✓ students status 200.............: 100%
✗ students < 500ms...............: 85%   (Target: 95%)

http_req_duration...............: avg=450ms min=120ms max=3.2s p(95)=820ms
http_reqs.......................: 1500 requests
```

**Checklist:**
- [ ] P95 response time < 500ms (95% of requests)
- [ ] No 500 errors under load
- [ ] No timeouts (all requests complete)
- [ ] Throughput > 100 requests/second

**Common Issues:**

**Issue 1: Slow Endpoint (> 500ms)**
```csharp
// Investigate with Visual Studio Profiler
// Look for:
// - Database queries (should be < 200ms)
// - Synchronous operations (use async/await)
// - Unnecessary data fetching
// - Missing AsNoTracking()
```

**Issue 2: High Memory Usage**
```csharp
// ❌ WRONG - Loads all data into memory
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .ToListAsync(); // 50,000 students = 100MB memory!

// ✅ CORRECT - Pagination
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .OrderBy(s => s.LastName)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(); // 50 students = 100KB memory
```

---

### Step 3: Frontend Performance (10 minutes)

**Tool:** Chrome DevTools (Lighthouse)

**Run Lighthouse Audit:**
1. Open Chrome DevTools (F12)
2. Lighthouse tab
3. Run audit (Performance + Accessibility)

**Performance Targets:**
- [ ] Performance Score > 90
- [ ] First Contentful Paint (FCP) < 1.5s
- [ ] Largest Contentful Paint (LCP) < 2.5s
- [ ] Time to Interactive (TTI) < 3.5s
- [ ] Cumulative Layout Shift (CLS) < 0.1

**Common Issues:**

**Issue 1: Large Bundle Size**
```powershell
# Check bundle size
cd Frontend
npm run build

# Analyze bundle
npx vite-bundle-visualizer
```

**Fix:**
```typescript
// ✅ Code splitting (lazy loading)
import { lazy, Suspense } from 'react';

const StudentList = lazy(() => import('@/components/students/student-list'));

function App() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <StudentList />
    </Suspense>
  );
}
```

**Issue 2: Too Many API Calls**
```typescript
// ❌ WRONG - Separate API calls
const { data: student } = useQuery({
  queryKey: ['student', id],
  queryFn: () => studentService.getById(id)
});

const { data: enrollments } = useQuery({
  queryKey: ['enrollments', id],
  queryFn: () => enrollmentService.getByStudentId(id)
});
// Result: 2 API calls

// ✅ CORRECT - Single API call with includes
const { data } = useQuery({
  queryKey: ['student-detail', id],
  queryFn: () => studentService.getByIdWithEnrollments(id)
});
// Result: 1 API call
```

**Issue 3: Unnecessary Re-renders**
```typescript
// ❌ WRONG - Re-renders on every parent update
function StudentRow({ student }) {
  console.log('Rendering', student.name);
  return <tr>...</tr>;
}

// ✅ CORRECT - Memoized component
import { memo } from 'react';

const StudentRow = memo(function StudentRow({ student }) {
  return <tr>...</tr>;
});
// Only re-renders when student prop changes
```

---

### Step 4: Caching Strategy Review (5 minutes)

**Response Caching (Backend):**
```csharp
// Check for caching attributes
[HttpGet]
[ResponseCache(Duration = 300, VaryByHeader = "Authorization")] // ✅ Cache for 5 minutes
public async Task<IActionResult> GetAll()
{
    // Read-only data that doesn't change often
}
```

**Query Caching (TanStack Query):**
```typescript
// Frontend caching configuration
const { data } = useQuery({
  queryKey: ['students', schoolId],
  queryFn: studentService.getAll,
  staleTime: 5 * 60 * 1000, // ✅ 5 minutes
  cacheTime: 30 * 60 * 1000, // ✅ 30 minutes
});
```

**Checklist:**
- [ ] Read-only endpoints have response caching (5-30 minutes)
- [ ] Frequently accessed data cached (schools, teachers, courses)
- [ ] Cache invalidation on data changes
- [ ] Frontend queries have staleTime configured

---

### Step 5: Database Index Review (10 minutes)

**Check Existing Indexes:**
```sql
-- List all indexes with usage stats
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    ius.user_seeks,
    ius.user_scans,
    ius.user_lookups,
    ius.user_updates,
    ius.last_user_seek,
    ius.last_user_scan
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats ius 
    ON i.object_id = ius.object_id AND i.index_id = ius.index_id
WHERE OBJECT_NAME(i.object_id) LIKE '%Students%'
ORDER BY ius.user_seeks DESC;
```

**Analyze Results:**
- **High seeks/scans:** Good index, frequently used ✅
- **Zero seeks/scans:** Unused index, consider removing ❌
- **High updates, low seeks:** Index slowing down writes ⚠️

**Checklist:**
- [ ] Every table has index on SchoolId
- [ ] Composite indexes on SchoolId + frequently queried columns
- [ ] Foreign key columns indexed
- [ ] Covering indexes for common queries
- [ ] No duplicate indexes

**Common Index Patterns:**
```sql
-- ✅ Composite index for multi-tenant queries
CREATE INDEX IX_Students_SchoolId_LastName_FirstName
ON Students (SchoolId, LastName, FirstName);

-- ✅ Filtered index for active records
CREATE INDEX IX_Students_SchoolId_IsActive
ON Students (SchoolId, IsActive)
WHERE IsActive = 1;

-- ✅ Covering index (includes additional columns)
CREATE INDEX IX_Students_SchoolId_LastName
ON Students (SchoolId, LastName)
INCLUDE (FirstName, Email, PhoneNumber);
```

---

## 📊 Performance Metrics Dashboard

**Create Performance Baseline:**
```markdown
### Performance Baseline (2026-01-15)

| Endpoint | P50 | P95 | P99 | Throughput |
|----------|-----|-----|-----|------------|
| GET /api/students | 85ms | 120ms | 180ms | 450 req/s |
| POST /api/students | 110ms | 160ms | 220ms | 300 req/s |
| GET /api/students/{id} | 45ms | 75ms | 95ms | 800 req/s |
| PUT /api/students/{id} | 95ms | 140ms | 190ms | 350 req/s |

### Database Query Performance

| Query | Avg Time | Max Time | Executions/min |
|-------|----------|----------|----------------|
| Student List | 38ms | 120ms | 1,200 |
| Student Detail | 22ms | 65ms | 800 |
| Enrollment List | 45ms | 140ms | 600 |

### Frontend Performance

| Metric | Value | Target |
|--------|-------|--------|
| Performance Score | 94 | > 90 |
| FCP | 1.2s | < 1.5s |
| LCP | 2.1s | < 2.5s |
| TTI | 2.8s | < 3.5s |
| Bundle Size | 285KB | < 500KB |
```

**Save as:** `docs/performance-baseline.md`

---

## ✅ Performance Approval Checklist

**Backend:**
- [ ] All endpoints < 500ms (P95)
- [ ] No N+1 query patterns
- [ ] Pagination implemented for lists
- [ ] AsNoTracking used for read-only queries
- [ ] Indexes on SchoolId + frequently queried columns
- [ ] Response caching on read-only endpoints

**Database:**
- [ ] All queries < 200ms
- [ ] No table scans on large tables
- [ ] Missing index recommendations addressed
- [ ] Composite indexes for common queries
- [ ] Unused indexes removed

**Frontend:**
- [ ] Lighthouse Performance Score > 90
- [ ] Bundle size < 500KB
- [ ] Code splitting implemented
- [ ] TanStack Query caching configured
- [ ] Unnecessary re-renders eliminated

**Load Testing:**
- [ ] 100+ requests/second throughput
- [ ] No errors under 50 concurrent users
- [ ] No timeouts under load
- [ ] Memory usage stable over time

---

## 🚀 Quick Wins (Implement First)

### 1. Add AsNoTracking to Read-Only Queries
**Impact:** 30-40% faster, 40% less memory  
**Effort:** 5 minutes  
**ROI:** High

```csharp
// Find all read-only queries
git grep "\.ToListAsync()" | grep -v "AsNoTracking"

// Add AsNoTracking
.AsNoTracking()
.ToListAsync()
```

### 2. Add Pagination to List Endpoints
**Impact:** 95% faster, 90% less data transfer  
**Effort:** 15 minutes  
**ROI:** Very High

```csharp
// Add pagination parameters
.Skip((pageNumber - 1) * pageSize)
.Take(pageSize)
```

### 3. Add Response Caching
**Impact:** 96% faster, 98% less database load  
**Effort:** 2 minutes  
**ROI:** Very High

```csharp
[ResponseCache(Duration = 300, VaryByHeader = "Authorization")]
```

### 4. Add Composite Indexes
**Impact:** 85-95% faster queries  
**Effort:** 10 minutes  
**ROI:** Very High

```sql
CREATE INDEX IX_Students_SchoolId_LastName
ON Students (SchoolId, LastName);
```

---

## 📋 Performance Review Report Template

**Save as:** `docs/performance-review-{date}.md`

```markdown
# Performance Review - {Date}

## Summary
- Overall Status: ✅ Good / ⚠️ Needs Improvement / ❌ Critical Issues
- Performance Score: X/100
- Issues Found: X critical, X high, X medium

## Database Performance
- Slow Queries: X queries > 200ms
- Missing Indexes: X recommendations
- N+1 Patterns: X occurrences

### Action Items
1. [ ] Add index on Students (SchoolId, LastName) - P1
2. [ ] Fix N+1 in EnrollmentController.GetAll() - P1
3. [ ] Add AsNoTracking to StudentRepository.GetAll() - P2

## API Performance
- Slow Endpoints: X endpoints > 500ms
- Throughput: X req/s (Target: 100 req/s)
- Error Rate: X% (Target: < 1%)

### Action Items
1. [ ] Add pagination to GET /api/enrollments - P0
2. [ ] Add response caching to GET /api/schools - P2

## Frontend Performance
- Lighthouse Score: X/100
- Bundle Size: XKB (Target: < 500KB)
- FCP: Xs (Target: < 1.5s)

### Action Items
1. [ ] Implement code splitting for admin pages - P2
2. [ ] Optimize images (use WebP format) - P3

## Next Review Date
{Date + 1 week}
```

---

**Related Files:**
- `.copilot/memory/optimization-wins.md`
- `.copilot/agents/database-agent.md`
- `.copilot/workflows/add-new-table.md`
