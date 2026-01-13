# Testing Strategy
## Comprehensive Testing for Production Readiness

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 15 minutes  
**Status:** 🟡 In Progress

---

## 🎯 **Testing Philosophy**

**"Test early, test often, automate everything."**

Our testing pyramid prioritizes fast, isolated tests at the base and slower, integrated tests at the top.

```
               ┌─────────────┐
              ╱  E2E Tests   ╲     10% - Critical user flows
             ╱   (Slow)       ╲    Run before deployment
            ┌─────────────────┐
           ╱  Integration     ╲   30% - API + Database
          ╱    Tests           ╲  Run on every commit
         ┌─────────────────────┐
        ╱   Unit Tests          ╲  60% - Business logic
       ╱    (Fast)               ╲ Run on every save
      └───────────────────────────┘
```

---

## 📋 **Test Coverage Goals**

| Category | Target Coverage | Current | Status |
|----------|----------------|---------|--------|
| Unit Tests | 80% | 45% | 🟡 In Progress |
| Integration Tests | 70% | 30% | 🟡 In Progress |
| Security Tests | 100% | 0% | 🔴 Not Started |
| E2E Tests | Critical paths | 20% | 🟡 In Progress |
| Load Tests | All endpoints | 0% | 🔴 Not Started |

---

## 🧪 **Unit Tests (Business Logic)**

**Framework:** xUnit + Moq + FluentAssertions

### **Example: StudentService Tests**

**File:** `Backend/SMSServices.Tests/StudentServiceTests.cs`

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using SMSServices.Services;
using SMSRepository.RepositoryInterfaces;
using SMSDataModel.Model;

namespace SMSServices.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _mockRepo;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _mockRepo = new Mock<IStudentRepository>();
            _service = new StudentService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetStudentsBySchoolId_ReturnsOnlySchoolStudents()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var otherSchoolId = Guid.NewGuid();

            var students = new List<Student>
            {
                new Student { Id = Guid.NewGuid(), SchoolId = schoolId, FirstName = "John" },
                new Student { Id = Guid.NewGuid(), SchoolId = schoolId, FirstName = "Jane" },
                new Student { Id = Guid.NewGuid(), SchoolId = otherSchoolId, FirstName = "Bob" }
            };

            _mockRepo.Setup(r => r.GetBySchoolIdAsync(schoolId))
                     .ReturnsAsync(students.Where(s => s.SchoolId == schoolId));

            // Act
            var result = await _service.GetStudentsBySchoolIdAsync(schoolId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.SchoolId == schoolId);
            result.Should().NotContain(s => s.FirstName == "Bob");
        }

        [Fact]
        public async Task CreateStudent_WithoutSchoolId_ThrowsException()
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                FirstName = "John",
                LastName = "Doe",
                SchoolId = Guid.Empty  // Invalid
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CreateStudentAsync(dto));
        }

        [Fact]
        public async Task CreateStudent_WithValidData_ReturnsStudentWithSchoolId()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var dto = new CreateStudentDto
            {
                FirstName = "John",
                LastName = "Doe",
                SchoolId = schoolId,
                Email = "john@school.com"
            };

            var expectedStudent = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                SchoolId = schoolId
            };

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Student>()))
                     .ReturnsAsync(expectedStudent);

            // Act
            var result = await _service.CreateStudentAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.SchoolId.Should().Be(schoolId);
            result.FirstName.Should().Be("John");
            
            _mockRepo.Verify(r => r.AddAsync(It.Is<Student>(
                s => s.SchoolId == schoolId && s.FirstName == "John"
            )), Times.Once);
        }

        [Theory]
        [InlineData("", "Doe", "john@school.com")]  // Empty first name
        [InlineData("John", "", "john@school.com")]  // Empty last name
        [InlineData("John", "Doe", "invalid-email")] // Invalid email
        public async Task CreateStudent_WithInvalidData_ThrowsValidationException(
            string firstName, string lastName, string email)
        {
            // Arrange
            var dto = new CreateStudentDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                SchoolId = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await _service.CreateStudentAsync(dto));
        }
    }
}
```

**Run unit tests:**

```powershell
cd Backend
dotnet test SMSServices.Tests --logger "console;verbosity=detailed"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 🔗 **Integration Tests (API + Database)**

**Framework:** xUnit + WebApplicationFactory + Testcontainers

### **Example: Student API Integration Tests**

**File:** `Backend/SMSPrototype1.Tests/Integration/StudentControllerTests.cs`

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace SMSPrototype1.Tests.Integration
{
    public class StudentControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public StudentControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetStudents_WithValidToken_ReturnsOnlySchoolStudents()
        {
            // Arrange
            var token = await GetAuthTokenForSchool("School A");
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/students");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
            students.Should().NotBeEmpty();
            students.Should().OnlyContain(s => s.SchoolName == "School A");
        }

        [Fact]
        public async Task GetStudent_FromDifferentSchool_Returns403()
        {
            // Arrange: Create student in School A
            var schoolAToken = await GetAuthTokenForSchool("School A");
            var studentId = await CreateStudentInSchool("School A", schoolAToken);

            // Act: Try to access with School B token
            var schoolBToken = await GetAuthTokenForSchool("School B");
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", schoolBToken);

            var response = await _client.GetAsync($"/api/students/{studentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            error.Message.Should().Contain("different school");
        }

        [Fact]
        public async Task CreateStudent_WithoutSchoolId_AutoAssignsFromToken()
        {
            // Arrange
            var token = await GetAuthTokenForSchool("School A");
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            var createDto = new CreateStudentDto
            {
                FirstName = "Test",
                LastName = "Student",
                Email = "test@school.com",
                // SchoolId intentionally omitted - should be set from JWT
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/students", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var student = await response.Content.ReadFromJsonAsync<StudentDto>();
            student.SchoolName.Should().Be("School A");
        }

        private async Task<string> GetAuthTokenForSchool(string schoolName)
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = $"admin@{schoolName.ToLower().Replace(" ", "")}.com",
                password = "Admin@123"
            });

            var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            return result.Token;
        }

        private async Task<Guid> CreateStudentInSchool(string schoolName, string token)
        {
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/students", new
            {
                firstName = "Test",
                lastName = "Student",
                email = $"test@{schoolName.ToLower().Replace(" ", "")}.com"
            });

            var student = await response.Content.ReadFromJsonAsync<StudentDto>();
            return student.Id;
        }
    }
}
```

**Custom Test Factory:**

```csharp
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace database with test database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SMSDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<SMSDbContext>(options =>
            {
                options.UseSqlServer("Server=localhost;Database=SchoolManagementDB_Test;...");
            });

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SMSDbContext>();
            
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(SMSDbContext db)
    {
        // Create two test schools
        var schoolA = new School { Id = Guid.NewGuid(), Name = "School A", Code = "SCHOOL_A" };
        var schoolB = new School { Id = Guid.NewGuid(), Name = "School B", Code = "SCHOOL_B" };
        
        db.Schools.AddRange(schoolA, schoolB);

        // Create test users for each school
        // ... (add users, students, teachers, etc.)

        db.SaveChanges();
    }
}
```

---

## 🔒 **Security Tests (Multi-Tenant Isolation)**

**CRITICAL: These tests MUST pass before production deployment.**

**File:** `Backend/SMSPrototype1.Tests/Security/MultiTenantSecurityTests.cs`

```csharp
using Xunit;
using System.Net;

namespace SMSPrototype1.Tests.Security
{
    public class MultiTenantSecurityTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MultiTenantSecurityTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Scenario1_DirectStudentIdAccess_AcrossSchools_ShouldFail()
        {
            // Arrange: Get student from School A
            var schoolAToken = await LoginAsSchoolA();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolAToken);
            
            var schoolAStudents = await _client.GetFromJsonAsync<List<StudentDto>>("/api/students");
            var studentId = schoolAStudents.First().Id;

            // Act: Try to access with School B token
            var schoolBToken = await LoginAsSchoolB();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolBToken);
            
            var response = await _client.GetAsync($"/api/students/{studentId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Scenario2_UpdateStudentSchoolId_ShouldBeIgnored()
        {
            // Arrange
            var token = await LoginAsSchoolA();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var student = await CreateTestStudent();

            // Act: Try to change SchoolId
            var maliciousSchoolId = Guid.NewGuid();
            var updateDto = new UpdateStudentDto
            {
                FirstName = "Updated",
                SchoolId = maliciousSchoolId  // Attempt to change school
            };

            var response = await _client.PutAsJsonAsync($"/api/students/{student.Id}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var updatedStudent = await _client.GetFromJsonAsync<StudentDto>($"/api/students/{student.Id}");
            Assert.NotEqual(maliciousSchoolId, updatedStudent.SchoolId);  // Should remain original
        }

        [Fact]
        public async Task Scenario3_JWTTokenTampering_ShouldBeRejected()
        {
            // Arrange: Get valid token
            var validToken = await LoginAsSchoolA();
            
            // Decode, modify SchoolId claim, re-encode (without valid signature)
            var tamperedToken = TamperWithSchoolIdClaim(validToken, Guid.NewGuid());
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);

            // Act
            var response = await _client.GetAsync("/api/students");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);  // Invalid signature
        }

        [Fact]
        public async Task Scenario4_SQLInjection_InSchoolIdFilter_ShouldBeSanitized()
        {
            // Arrange
            var token = await LoginAsSchoolA();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act: Attempt SQL injection in query parameter
            var maliciousQuery = "'; DROP TABLE Students; --";
            var response = await _client.GetAsync($"/api/students?search={Uri.EscapeDataString(maliciousQuery)}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);  // Query handled safely
            
            // Verify Students table still exists
            var studentsResponse = await _client.GetAsync("/api/students");
            Assert.Equal(HttpStatusCode.OK, studentsResponse.StatusCode);
        }

        [Fact]
        public async Task Scenario5_SuperAdminAccess_AllSchools_WithAuditLog()
        {
            // Arrange: Login as SuperAdmin
            var superAdminToken = await LoginAsSuperAdmin();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", superAdminToken);

            // Act: Access School A students
            var schoolAId = await GetSchoolIdByName("School A");
            var response = await _client.GetAsync($"/api/superadmin/schools/{schoolAId}/students");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify audit log entry created
            var auditLogs = await _client.GetFromJsonAsync<List<AuditLogDto>>("/api/superadmin/audit-logs");
            Assert.Contains(auditLogs, log => 
                log.Action.Contains("SuperAdmin") && 
                log.SchoolId == schoolAId &&
                log.Severity == "Warning");
        }

        [Fact]
        public async Task Scenario6_BulkImport_ShouldNotAcceptForeignSchoolId()
        {
            // Arrange
            var token = await LoginAsSchoolA();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var schoolBId = await GetSchoolIdByName("School B");

            var csvContent = $@"
FirstName,LastName,Email,SchoolId
John,Doe,john@test.com,{schoolBId}
Jane,Smith,jane@test.com,{schoolBId}
";

            // Act
            var content = new StringContent(csvContent, Encoding.UTF8, "text/csv");
            var response = await _client.PostAsync("/api/students/bulk-import", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<BulkImportResult>();
            
            // Students should be created in School A, not School B
            var students = await _client.GetFromJsonAsync<List<StudentDto>>("/api/students");
            Assert.All(students.Where(s => s.FirstName == "John" || s.FirstName == "Jane"), 
                s => Assert.NotEqual(schoolBId, s.SchoolId));
        }

        [Fact]
        public async Task Scenario7_DatabaseDirectQuery_ShouldRespectSchoolFilter()
        {
            // This test verifies repository layer respects SchoolId

            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
            var schoolAId = await GetSchoolIdByName("School A");

            // Act
            var students = await repo.GetBySchoolIdAsync(schoolAId);

            // Assert
            Assert.NotEmpty(students);
            Assert.All(students, s => Assert.Equal(schoolAId, s.SchoolId));
        }

        [Fact]
        public async Task Scenario8_ChatRoom_CrossSchoolMessage_ShouldFail()
        {
            // Arrange: Create chat room in School A
            var schoolAToken = await LoginAsSchoolA();
            var chatRoomId = await CreateChatRoom(schoolAToken);

            // Act: Try to send message from School B user
            var schoolBToken = await LoginAsSchoolB();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolBToken);
            
            var response = await _client.PostAsJsonAsync($"/api/chat/rooms/{chatRoomId}/messages", new
            {
                message = "Hello from School B"
            });

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ... Continue with 12 more security test scenarios
    }
}
```

---

## 🚀 **Load Testing with k6**

**File:** `Backend/performance-tests/load-test-full.js`

```javascript
import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');
const BASE_URL = __ENV.BASE_URL || 'http://localhost:7266';

export let options = {
  stages: [
    { duration: '2m', target: 50 },    // Ramp up to 50 users
    { duration: '5m', target: 100 },   // Ramp up to 100 users
    { duration: '5m', target: 100 },   // Stay at 100 users
    { duration: '2m', target: 200 },   // Spike to 200 users
    { duration: '3m', target: 200 },   // Stay at peak
    { duration: '2m', target: 0 },     // Ramp down
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'],  // 95% < 500ms, 99% < 1s
    'http_req_failed': ['rate<0.01'],  // Error rate < 1%
    'errors': ['rate<0.01'],
  },
};

export default function () {
  let token;

  // Login
  group('Authentication', function () {
    let loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
      email: 'admin@school.com',
      password: 'Admin@123'
    }), {
      headers: { 'Content-Type': 'application/json' },
    });

    check(loginRes, {
      'login status 200': (r) => r.status === 200,
      'login has token': (r) => r.json('token') !== undefined,
      'login time < 300ms': (r) => r.timings.duration < 300,
    }) || errorRate.add(1);

    token = loginRes.json('token');
  });

  sleep(1);

  // Get students
  group('Student Operations', function () {
    let headers = { 'Authorization': `Bearer ${token}` };

    let studentsRes = http.get(`${BASE_URL}/api/students`, { headers });
    check(studentsRes, {
      'get students status 200': (r) => r.status === 200,
      'get students time < 200ms': (r) => r.timings.duration < 200,
      'students array returned': (r) => Array.isArray(r.json()),
    }) || errorRate.add(1);

    // Get single student
    if (studentsRes.status === 200 && studentsRes.json().length > 0) {
      let studentId = studentsRes.json()[0].id;
      
      let studentRes = http.get(`${BASE_URL}/api/students/${studentId}`, { headers });
      check(studentRes, {
        'get student status 200': (r) => r.status === 200,
        'get student time < 150ms': (r) => r.timings.duration < 150,
      }) || errorRate.add(1);
    }
  });

  sleep(1);

  // Attendance operations
  group('Attendance Operations', function () {
    let headers = { 'Authorization': `Bearer ${token}` };

    let attendanceRes = http.get(`${BASE_URL}/api/attendance?date=${new Date().toISOString().split('T')[0]}`, { headers });
    check(attendanceRes, {
      'get attendance status 200': (r) => r.status === 200,
      'attendance time < 250ms': (r) => r.timings.duration < 250,
    }) || errorRate.add(1);
  });

  sleep(2);
}

// Smoke test (run after deployment)
export function handleSummary(data) {
  return {
    'load-test-results.json': JSON.stringify(data),
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
  };
}
```

**Run load tests:**

```bash
# Local testing
k6 run Backend/performance-tests/load-test-full.js

# With custom environment
k6 run --env BASE_URL=https://staging.schoolms.com Backend/performance-tests/load-test-full.js

# Generate HTML report
k6 run --out json=test-results.json Backend/performance-tests/load-test-full.js
k6-html-reporter --input test-results.json --output load-test-report.html
```

---

## 🎭 **End-to-End Tests with Playwright**

**Framework:** Playwright (TypeScript)

**File:** `Frontend/tests/e2e/student-crud.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Student CRUD Operations', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('http://localhost:3000/login');
    await page.fill('[name="email"]', 'admin@school.com');
    await page.fill('[name="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    
    await expect(page).toHaveURL('http://localhost:3000/dashboard');
  });

  test('should create a new student', async ({ page }) => {
    // Navigate to students page
    await page.click('text=Students');
    await expect(page).toHaveURL(/.*students/);

    // Click "Add Student" button
    await page.click('button:has-text("Add Student")');

    // Fill form
    await page.fill('[name="firstName"]', 'John');
    await page.fill('[name="lastName"]', 'Doe');
    await page.fill('[name="email"]', 'john.doe@test.com');
    await page.fill('[name="rollNumber"]', '12345');

    // Submit
    await page.click('button:has-text("Create")');

    // Verify success
    await expect(page.locator('text=Student created successfully')).toBeVisible();
    await expect(page.locator('text=John Doe')).toBeVisible();
  });

  test('should not show students from other schools', async ({ page }) => {
    // This test requires two test schools with known students

    // Navigate to students page
    await page.click('text=Students');

    // Get all student names
    const studentNames = await page.locator('[data-testid="student-name"]').allTextContents();

    // Verify none belong to "School B" (current user is from "School A")
    for (const name of studentNames) {
      const schoolCell = await page.locator(`tr:has-text("${name}") td[data-testid="school-name"]`);
      await expect(schoolCell).not.toContainText('School B');
    }
  });

  test('should handle cross-school access gracefully', async ({ page }) => {
    // Try to navigate directly to a student from another school
    const otherSchoolStudentId = 'abcd-1234-other-school-student';
    
    await page.goto(`http://localhost:3000/students/${otherSchoolStudentId}`);

    // Should show error message
    await expect(page.locator('text=You do not have permission')).toBeVisible();
    await expect(page.locator('text=different school')).toBeVisible();
  });
});
```

**Run E2E tests:**

```bash
cd Frontend
npx playwright test

# With UI
npx playwright test --ui

# Specific test
npx playwright test student-crud.spec.ts
```

---

## 🤖 **AI-Assisted Testing**

**Use GitHub Copilot Chat to generate test cases:**

```
Prompt: "Generate xUnit test cases for TeacherService.GetTeachersBySchoolIdAsync() 
covering: 
1. Returns only teachers from specified school
2. Empty list when no teachers exist
3. Exception when SchoolId is Guid.Empty
4. Pagination works correctly
5. Includes related user data"
```

**Use Copilot to write security tests:**

```
Prompt: "Write integration test that verifies a user from School A cannot 
access, update, or delete a student from School B through the API"
```

---

## ✅ **Testing Checklist (Before Production)**

```
Unit Tests (60% coverage minimum):
[ ] StudentService - All CRUD methods
[ ] TeacherService - All CRUD methods  
[ ] AuthService - Login, register, token generation
[ ] AttendanceService - Mark attendance, get reports
[ ] GradeService - Enter grades, calculate averages

Integration Tests (30% of critical paths):
[ ] Student API - CRUD with SchoolId validation
[ ] Teacher API - CRUD with SchoolId validation
[ ] Auth API - Login returns valid JWT with SchoolId
[ ] Attendance API - Only shows school's attendance
[ ] Cross-school access denied in all endpoints

Security Tests (100% of scenarios):
[ ] ✅ Scenario 1: Direct ID access across schools
[ ] ✅ Scenario 2: Update SchoolId attempt
[ ] ✅ Scenario 3: JWT tampering
[ ] ✅ Scenario 4: SQL injection
[ ] ✅ Scenario 5: SuperAdmin access with audit
[ ] ✅ Scenario 6: Bulk import foreign SchoolId
[ ] ✅ Scenario 7: Repository layer filtering
[ ] ✅ Scenario 8: Chat room cross-school message
[ ] ... (12 more scenarios)

Load Tests:
[ ] 100 concurrent users - < 500ms (p95)
[ ] 200 concurrent users (spike) - < 1s (p99)
[ ] Error rate < 1%
[ ] Database CPU < 80%

E2E Tests (Critical user flows):
[ ] Admin creates student → Student appears in list
[ ] Teacher marks attendance → Attendance saved
[ ] Student views grades → Only their grades shown
[ ] Parent views multiple children → All children visible
[ ] Cross-school access → Error message displayed
```

---

## 📊 **Continuous Testing in CI/CD**

**GitHub Actions workflow:**

```yaml
# .github/workflows/test.yml
name: Test Suite

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Run unit tests
        run: |
          cd Backend
          dotnet test --logger "trx;LogFileName=test-results.trx" /p:CollectCoverage=true
      
      - name: Upload test results
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: '**/test-results.trx'

  integration-tests:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Run integration tests
        run: |
          cd Backend
          dotnet test SMSPrototype1.Tests --filter Category=Integration

  security-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Run security tests
        run: |
          cd Backend
          dotnet test SMSPrototype1.Tests --filter Category=Security
      
      - name: Security test must pass
        run: exit 1
        if: failure()

  load-tests:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup k6
        run: |
          sudo gpg -k
          sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      
      - name: Run load tests
        run: |
          k6 run Backend/performance-tests/load-test-full.js
```

---

## 📚 **Next Steps**

1. **Code Standards:** [19_CODE_STANDARDS.md](./19_CODE_STANDARDS.md)
2. **Security Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)
3. **Deployment:** [09_DEPLOYMENT_ARCHITECTURE.md](./09_DEPLOYMENT_ARCHITECTURE.md)

---

**Document Status:** ✅ Complete  
**Tests Written:** 🟡 40% Complete  
**Priority:** Achieve 80% unit test coverage before production