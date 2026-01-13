# Performance Optimization
## Caching, Indexing & Query Optimization

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 15 minutes  
**Status:** 🟡 Partially Implemented

---

## 🎯 **Performance Goals**

| Metric | Target | Current | Status |
|--------|--------|---------|--------|
| **API Response (P95)** | < 200ms | ~300ms | 🟡 Needs work |
| **Dashboard Load** | < 1s | ~2s | 🟡 Needs work |
| **Database Query** | < 50ms | ~100ms | 🟡 Needs work |
| **Page Size** | < 2MB | ~3MB | 🟡 Optimize assets |
| **Concurrent Users** | 500+ | Untested | 🔴 Need load test |

---

## 💾 **Database Optimization**

### **1. Index Strategy**

**Critical Indexes (MUST HAVE):**

```sql
-- Multi-tenant isolation (MOST IMPORTANT)
CREATE INDEX IX_Students_SchoolId 
ON Students(SchoolId) 
INCLUDE (FirstName, LastName, Grade, ClassId)
WHERE IsDeleted = 0;

CREATE INDEX IX_Teachers_SchoolId 
ON Teachers(SchoolId) 
INCLUDE (FirstName, LastName, Email)
WHERE IsDeleted = 0;

CREATE INDEX IX_Classes_SchoolId 
ON Classes(SchoolId) 
INCLUDE (Name, Grade, Section)
WHERE IsDeleted = 0;

-- Attendance queries (date range scans)
CREATE INDEX IX_Attendance_SchoolId_Date 
ON Attendance(SchoolId, Date DESC)
INCLUDE (StudentId, Status);

-- Grades queries
CREATE INDEX IX_Grades_StudentId 
ON Grades(StudentId, AcademicYear, Term)
INCLUDE (SubjectId, MarksObtained, Grade);

-- Chat messages (recent messages first)
CREATE INDEX IX_ChatMessages_RoomId_SentAt 
ON ChatMessages(RoomId, SentAt DESC)
INCLUDE (SenderId, Message)
WHERE IsDeleted = 0;

-- Foreign key indexes
CREATE INDEX IX_Students_ClassId ON Students(ClassId);
CREATE INDEX IX_ClassSubjects_ClassId ON ClassSubjects(ClassId);
CREATE INDEX IX_ClassSubjects_TeacherId ON ClassSubjects(TeacherId);
```

**Query Optimization Example:**

```sql
-- ❌ BAD: Table scan (no index)
SELECT * FROM Students 
WHERE FirstName LIKE 'Raj%';

-- ✅ GOOD: Uses IX_Students_SchoolId (SchoolId filter first)
SELECT * FROM Students 
WHERE SchoolId = @schoolId 
  AND FirstName LIKE 'Raj%';

-- 🚀 BEST: Covering index (no table lookup)
CREATE INDEX IX_Students_SchoolId_FirstName 
ON Students(SchoolId, FirstName)
INCLUDE (LastName, Grade, ClassId);
```

### **2. Query Performance Analysis**

**Find Slow Queries:**

```sql
-- Top 10 slowest queries
SELECT TOP 10
    qs.execution_count,
    CAST(qs.total_elapsed_time / 1000000.0 AS DECIMAL(10,2)) AS total_elapsed_seconds,
    CAST(qs.total_elapsed_time / qs.execution_count / 1000000.0 AS DECIMAL(10,4)) AS avg_elapsed_seconds,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY qs.total_elapsed_time DESC;

-- Missing indexes
SELECT 
    OBJECT_NAME(d.object_id) AS TableName,
    d.equality_columns,
    d.inequality_columns,
    d.included_columns,
    s.user_seeks,
    s.avg_user_impact
FROM sys.dm_db_missing_index_details d
INNER JOIN sys.dm_db_missing_index_groups g ON d.index_handle = g.index_handle
INNER JOIN sys.dm_db_missing_index_group_stats s ON g.index_group_handle = s.group_handle
WHERE s.avg_user_impact > 50
ORDER BY s.avg_user_impact DESC;
```

### **3. Pagination Optimization**

```csharp
// ❌ BAD: Loads all data, then takes page (memory intensive)
var allStudents = await _context.Students.ToListAsync();
var page = allStudents.Skip((page - 1) * pageSize).Take(pageSize);

// ✅ GOOD: Database-level pagination
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
    .OrderBy(s => s.FirstName)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
    .ToListAsync();
```

---

## 🗄️ **Caching Strategy**

### **Redis Cache Implementation**

**1. Cache User Sessions:**

```csharp
public class CacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        // Try get from cache
        var cached = await _cache.GetStringAsync(key);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached);
        }
        
        // Cache miss - fetch from source
        var value = await factory();
        
        // Store in cache
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };
        
        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            options
        );
        
        return value;
    }
    
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
```

**2. Cache Common Queries:**

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetSchool(Guid id)
{
    var school = await _cacheService.GetOrCreateAsync(
        $"school:{id}",
        async () => await _schoolService.GetByIdAsync(id),
        TimeSpan.FromMinutes(10)
    );
    
    return school == null ? NotFound() : Ok(school);
}
```

**3. Cache Invalidation:**

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateSchool(Guid id, UpdateSchoolDto dto)
{
    await _schoolService.UpdateAsync(id, dto);
    
    // Invalidate cache
    await _cacheService.RemoveAsync($"school:{id}");
    
    return NoContent();
}
```

### **What to Cache**

| Data | Cache Duration | Invalidation |
|------|----------------|--------------|
| **School Info** | 10 minutes | On update |
| **User Profile** | 5 minutes | On update |
| **Class List** | 30 minutes | On CRUD |
| **Subject List** | 1 hour | Rarely changes |
| **Chat Room List** | 5 minutes | On new room |
| **JWT Blacklist** | 3 hours | On logout |
| **Rate Limit Counters** | 1 minute | Rolling window |

### **What NOT to Cache**

- ❌ Attendance (changes frequently)
- ❌ Grades (privacy-sensitive)
- ❌ Chat messages (real-time)
- ❌ Audit logs (critical data)
- ❌ Payment transactions (financial)

---

## 🚀 **API Optimization**

### **1. Response Compression**

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

app.UseResponseCompression();
```

**Result:** 70-80% size reduction for JSON responses

### **2. Async All The Way**

```csharp
// ❌ BAD: Blocking I/O
public IActionResult GetStudents()
{
    var students = _context.Students.ToList(); // Blocks thread
    return Ok(students);
}

// ✅ GOOD: Async I/O
public async Task<IActionResult> GetStudents()
{
    var students = await _context.Students.ToListAsync();
    return Ok(students);
}
```

### **3. Projection (Select Only Needed Fields)**

```csharp
// ❌ BAD: Fetches all columns + navigation properties
var students = await _context.Students
    .Include(s => s.Class)
    .Include(s => s.School)
    .ToListAsync();

// ✅ GOOD: Fetches only needed columns
var students = await _context.Students
    .Where(s => s.SchoolId == schoolId)
    .Select(s => new StudentListDto
    {
        Id = s.Id,
        FullName = s.FirstName + " " + s.LastName,
        Grade = s.Grade,
        ClassName = s.Class.Name
    })
    .ToListAsync();
```

**Result:** 10x faster queries, 5x less network traffic

### **4. Batch Operations**

```csharp
// ❌ BAD: 100 database round trips
foreach (var student in students)
{
    _context.Students.Add(student);
    await _context.SaveChangesAsync();
}

// ✅ GOOD: 1 database round trip
_context.Students.AddRange(students);
await _context.SaveChangesAsync();
```

---

## 🌐 **Frontend Optimization**

### **1. Code Splitting**

```typescript
// Lazy load routes
import { lazy, Suspense } from 'react';

const StudentList = lazy(() => import('./pages/StudentList'));
const GradeEntry = lazy(() => import('./pages/GradeEntry'));

function App() {
  return (
    <Suspense fallback={<Loading />}>
      <Routes>
        <Route path="/students" element={<StudentList />} />
        <Route path="/grades" element={<GradeEntry />} />
      </Routes>
    </Suspense>
  );
}
```

**Result:** Initial bundle size reduced from 3MB → 500KB

### **2. Image Optimization**

```typescript
// Use WebP format with fallback
<picture>
  <source srcSet="/images/logo.webp" type="image/webp" />
  <img src="/images/logo.png" alt="School Logo" />
</picture>

// Lazy load images below fold
<img 
  src="/images/student-placeholder.jpg" 
  data-src="/images/student-photo.jpg"
  loading="lazy"
  alt="Student Photo"
/>
```

### **3. React Query Optimization**

```typescript
// Prefetch data on hover
const { data: student, isLoading } = useQuery({
  queryKey: ['student', studentId],
  queryFn: () => fetchStudent(studentId),
  staleTime: 5 * 60 * 1000, // 5 minutes
  cacheTime: 10 * 60 * 1000, // 10 minutes
});

// Optimistic updates
const mutation = useMutation({
  mutationFn: updateStudent,
  onMutate: async (newData) => {
    // Cancel outgoing queries
    await queryClient.cancelQueries(['student', studentId]);
    
    // Snapshot previous value
    const previous = queryClient.getQueryData(['student', studentId]);
    
    // Optimistically update
    queryClient.setQueryData(['student', studentId], newData);
    
    return { previous };
  },
  onError: (err, newData, context) => {
    // Rollback on error
    queryClient.setQueryData(['student', studentId], context.previous);
  },
});
```

### **4. Virtualization (Large Lists)**

```typescript
import { useVirtualizer } from '@tanstack/react-virtual';

function StudentList({ students }) {
  const parentRef = useRef();
  
  const virtualizer = useVirtualizer({
    count: students.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 50, // Row height
    overscan: 10, // Render 10 extra rows
  });
  
  return (
    <div ref={parentRef} style={{ height: '600px', overflow: 'auto' }}>
      <div style={{ height: virtualizer.getTotalSize() }}>
        {virtualizer.getVirtualItems().map((virtualRow) => (
          <div
            key={virtualRow.index}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              transform: `translateY(${virtualRow.start}px)`,
            }}
          >
            <StudentRow student={students[virtualRow.index]} />
          </div>
        ))}
      </div>
    </div>
  );
}
```

**Result:** Render 10,000 students without lag

---

## 📊 **N+1 Query Problem**

### **Problem Example:**

```csharp
// ❌ BAD: N+1 queries (1 + 100 queries for 100 students)
var students = await _context.Students.ToListAsync(); // 1 query

foreach (var student in students)
{
    var className = student.Class.Name; // 1 query per student = 100 queries!
}
```

**Total:** 101 database queries 🐌

### **Solution: Eager Loading**

```csharp
// ✅ GOOD: 1 query with JOIN
var students = await _context.Students
    .Include(s => s.Class)
    .ToListAsync(); // 1 query with INNER JOIN

foreach (var student in students)
{
    var className = student.Class.Name; // No query, already loaded
}
```

**Total:** 1 database query 🚀

### **Better: Projection**

```csharp
// 🚀 BEST: 1 query, minimal data
var students = await _context.Students
    .Select(s => new 
    {
        StudentName = s.FirstName + " " + s.LastName,
        ClassName = s.Class.Name
    })
    .ToListAsync();
```

---

## 🔥 **Load Testing**

### **k6 Script (Performance Test)**

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Spike to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<200'], // 95% requests < 200ms
    http_req_failed: ['rate<0.01'],   // < 1% errors
  },
};

export default function () {
  // Login
  let loginRes = http.post('https://api.schoolms.com/api/auth/login', {
    email: 'teacher@school.com',
    password: 'TestPass123!',
  });
  
  check(loginRes, {
    'login successful': (r) => r.status === 200,
  });
  
  let token = loginRes.json('accessToken');
  
  // Get students
  let studentsRes = http.get('https://api.schoolms.com/api/students', {
    headers: { Authorization: `Bearer ${token}` },
  });
  
  check(studentsRes, {
    'students loaded': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
  });
  
  sleep(1);
}
```

**Run Test:**
```bash
k6 run load-test.js

# Output:
# ✓ login successful........: 100%
# ✓ students loaded.........: 100%
# ✓ response time < 200ms...: 95.3%
# http_req_duration.........: avg=156ms p(95)=189ms
# http_reqs.................: 12000
```

---

## 🎯 **Performance Monitoring**

### **Application Insights Queries**

```kusto
// Average response time by endpoint
requests
| where timestamp > ago(1h)
| summarize avg(duration), count() by name
| order by avg_duration desc

// 95th percentile response time
requests
| where timestamp > ago(24h)
| summarize percentile(duration, 95) by bin(timestamp, 1h)
| render timechart

// Failed requests
requests
| where success == false
| where timestamp > ago(1h)
| project timestamp, name, resultCode, duration
| order by timestamp desc
```

### **Real User Monitoring (RUM)**

```typescript
// Track page load time
window.addEventListener('load', () => {
  const loadTime = performance.timing.loadEventEnd - performance.timing.navigationStart;
  
  analytics.track('PageLoad', {
    page: window.location.pathname,
    loadTime: loadTime,
    user: currentUser.id,
  });
  
  // Alert if > 3 seconds
  if (loadTime > 3000) {
    console.warn(`Slow page load: ${loadTime}ms`);
  }
});
```

---

## ✅ **Performance Checklist**

**Database:**
- [ ] All multi-tenant tables have SchoolId index
- [ ] Foreign key indexes created
- [ ] Query execution plans reviewed
- [ ] No N+1 queries
- [ ] Pagination implemented
- [ ] Connection pooling enabled

**Caching:**
- [ ] Redis cache configured
- [ ] Common queries cached
- [ ] Cache invalidation on updates
- [ ] Rate limiting uses cache

**API:**
- [ ] Response compression enabled
- [ ] Async/await throughout
- [ ] Projection used (not full entities)
- [ ] Batch operations where possible

**Frontend:**
- [ ] Code splitting implemented
- [ ] Images optimized (WebP, lazy load)
- [ ] React Query caching
- [ ] Virtualization for long lists
- [ ] Bundle size < 500KB (gzipped)

**Monitoring:**
- [ ] Application Insights configured
- [ ] Performance alerts set up
- [ ] Load testing completed
- [ ] Real user monitoring enabled

---

## 📚 **Next Steps**

1. **Disaster Recovery:** [14_DISASTER_RECOVERY.md](./14_DISASTER_RECOVERY.md)
2. **Monitoring:** [17_MONITORING_LOGGING.md](./17_MONITORING_LOGGING.md)
3. **Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)

---

**Document Status:** ✅ Complete  
**Implementation:** 🟡 Partially Complete  
**Target:** 95% requests < 200ms by Feb 13, 2026
