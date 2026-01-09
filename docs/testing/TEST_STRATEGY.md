# API Testing Strategy - School Management System

## Overview

This document outlines the comprehensive testing strategy for the School Management System (SMS) API, covering unit tests, integration tests, end-to-end tests, contract tests, and performance tests.

**Last Updated:** January 9, 2026  
**Version:** 1.0  
**Status:** Active

---

## Table of Contents

- [Testing Objectives](#testing-objectives)
- [Test Pyramid](#test-pyramid)
- [Test Categories](#test-categories)
- [Testing Tools](#testing-tools)
- [Test Data Management](#test-data-management)
- [Environment Strategy](#environment-strategy)
- [CI/CD Integration](#cicd-integration)
- [Test Coverage Goals](#test-coverage-goals)
- [Reporting and Metrics](#reporting-and-metrics)

---

## Testing Objectives

### Primary Goals

1. **Functional Correctness**: Ensure all API endpoints behave as specified
2. **Security Validation**: Verify authentication, authorization, and data protection
3. **Performance Standards**: Meet response time and throughput requirements
4. **Reliability**: Achieve 99.9% uptime and handle edge cases gracefully
5. **Contract Compliance**: Maintain OpenAPI schema consistency

### Success Metrics

| Metric | Target | Current |
|--------|--------|---------|
| Unit Test Coverage | ≥ 80% | TBD |
| Integration Test Coverage | ≥ 70% | TBD |
| API Response Time (P95) | < 500ms | TBD |
| Defect Density | < 1 per 1000 LOC | TBD |
| Test Execution Time | < 10 minutes | TBD |

---

## Test Pyramid

```
           ┌─────────────┐
           │  E2E Tests  │  5% - Critical user flows
           │   (Slow)    │
           └─────────────┘
         ┌─────────────────┐
         │ Integration Tests│ 25% - API endpoints with DB
         │    (Medium)      │
         └─────────────────┘
       ┌───────────────────────┐
       │    Unit Tests         │ 70% - Services, Repositories, Utils
       │      (Fast)           │
       └───────────────────────┘
```

**Distribution:**
- **Unit Tests**: 70% - Fast, isolated, no dependencies
- **Integration Tests**: 25% - API + Database + Services
- **E2E Tests**: 5% - Complete user workflows
- **Performance Tests**: As needed, separate from pyramid

---

## Test Categories

### 1. Unit Tests (xUnit + Moq)

**Scope:** Business logic, services, repositories, utilities

**Framework:** xUnit.net with Moq for mocking

**Location:** `Backend/SMSServices.Tests/`, `Backend/SMSRepository.Tests/`

**Key Areas:**
- Service layer business logic
- Repository CRUD operations (mocked DbContext)
- Data validation and transformation
- Authentication service (token generation, validation)
- Encryption service (message encryption/decryption)
- Room access token service

**Example Structure:**
```
SMSServices.Tests/
├── Services/
│   ├── StudentServiceTests.cs
│   ├── TeacherServiceTests.cs
│   ├── AttendanceServiceTests.cs
│   ├── ChatRoomServiceTests.cs
│   └── AuthServiceTests.cs
├── Helpers/
│   └── DataSeedTests.cs
└── Utils/
    └── EncryptionTests.cs
```

**Sample Test:**
```csharp
[Fact]
public async Task CreateStudent_WithValidData_ReturnsStudent()
{
    // Arrange
    var mockRepo = new Mock<IStudentRepository>();
    var service = new StudentService(mockRepo.Object);
    var studentDto = new CreateStudentDto { /* ... */ };
    
    mockRepo.Setup(r => r.CreateAsync(It.IsAny<Student>()))
            .ReturnsAsync(new Student { Id = Guid.NewGuid() });
    
    // Act
    var result = await service.CreateStudentAsync(studentDto);
    
    // Assert
    Assert.NotNull(result);
    mockRepo.Verify(r => r.CreateAsync(It.IsAny<Student>()), Times.Once);
}
```

### 2. Integration Tests (xUnit + WebApplicationFactory)

**Scope:** API endpoints with real database

**Framework:** xUnit.net with WebApplicationFactory

**Location:** `Backend/SMSPrototype1.IntegrationTests/`

**Setup:**
- In-memory SQLite database OR separate test SQL Server database
- Real HTTP requests via `HttpClient`
- Seed test data before each test class
- Clean up after each test

**Key Areas:**
- All API controller endpoints
- Authentication flow (register → login → authorized request)
- CRUD operations with database persistence
- Role-based authorization
- SignalR hub connections

**Example Structure:**
```
SMSPrototype1.IntegrationTests/
├── Controllers/
│   ├── AuthControllerTests.cs
│   ├── StudentControllerTests.cs
│   ├── TeacherControllerTests.cs
│   ├── ClassControllerTests.cs
│   ├── AttendanceControllerTests.cs
│   └── ChatRoomsControllerTests.cs
├── Hubs/
│   ├── ChatHubTests.cs
│   └── VideoCallHubTests.cs
├── Fixtures/
│   └── WebAppFactory.cs
└── Helpers/
    └── TestDataSeeder.cs
```

**Sample Test:**
```csharp
public class StudentControllerTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    
    public StudentControllerTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAllStudents_WithAuth_ReturnsStudentList()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await _client.GetAsync("/api/Student");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResult<List<Student>>>(content);
        Assert.NotEmpty(result.Data);
    }
}
```

### 3. End-to-End Tests

**Scope:** Critical user workflows across multiple endpoints

**Tools:** Postman/Newman or Playwright for browser-based E2E

**Location:** `docs/testing/e2e-tests/`

**Key Workflows:**
1. **Student Enrollment Flow**
   - Admin creates school
   - Admin creates class
   - Admin registers student account
   - Admin creates student profile
   - Student logs in and views profile

2. **Teacher Onboarding Flow**
   - Admin registers teacher account
   - Admin creates teacher profile
   - Admin assigns teacher to classes
   - Teacher logs in and views assigned classes

3. **Attendance Tracking Flow**
   - Teacher logs in
   - Teacher views class roster
   - Teacher marks attendance for multiple students
   - Admin views attendance report

4. **Chat Room Flow**
   - Teacher creates chat room
   - Students join room with password
   - Multiple users send messages
   - Messages are encrypted and stored
   - Users receive real-time notifications

5. **Video Call Flow**
   - Teacher creates video room
   - Students join video call
   - WebRTC connections established
   - Audio/video toggled
   - Call recorded (if permitted)

**Postman Collection Example:**
```json
{
  "info": { "name": "E2E: Student Enrollment" },
  "item": [
    {
      "name": "1. Admin Login",
      "request": { "method": "POST", "url": "{{baseUrl}}/api/Auth/login" },
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": [
              "pm.test('Login successful', () => {",
              "  pm.response.to.have.status(200);",
              "});"
            ]
          }
        }
      ]
    },
    {
      "name": "2. Create School",
      "request": { "method": "POST", "url": "{{baseUrl}}/api/School" }
    },
    {
      "name": "3. Create Class",
      "request": { "method": "POST", "url": "{{baseUrl}}/api/Class" }
    },
    {
      "name": "4. Register Student",
      "request": { "method": "POST", "url": "{{baseUrl}}/api/Auth/register" }
    },
    {
      "name": "5. Create Student Profile",
      "request": { "method": "POST", "url": "{{baseUrl}}/api/Student" }
    }
  ]
}
```

### 4. Contract Tests (OpenAPI Validation)

**Scope:** Ensure API responses match OpenAPI schema

**Tools:** Postman schema validation, Schemathesis, or Dredd

**Location:** `docs/testing/contract-tests/`

**Validation Points:**
- Response structure matches schema
- Required fields are present
- Data types are correct
- Enum values are valid
- Status codes match documentation

**Postman Schema Validation:**
```javascript
pm.test("Response matches OpenAPI schema", function () {
    const schema = {
        "type": "object",
        "required": ["isSuccess", "data"],
        "properties": {
            "isSuccess": { "type": "boolean" },
            "data": {
                "type": "object",
                "required": ["id", "firstName", "lastName", "email"],
                "properties": {
                    "id": { "type": "string", "format": "uuid" },
                    "firstName": { "type": "string" },
                    "lastName": { "type": "string" },
                    "email": { "type": "string", "format": "email" }
                }
            },
            "errorMessage": { "type": ["string", "null"] }
        }
    };
    
    pm.response.to.have.jsonSchema(schema);
});
```

**TypeScript Type Validation:**
```typescript
// Ensure frontend types match backend DTOs
import { Student } from '@/types/api';

const response = await fetch('/api/Student/123');
const student: Student = await response.json();

// TypeScript compiler ensures type safety
console.log(student.firstName); // OK
console.log(student.invalidField); // Compile error
```

### 5. Performance Tests (k6)

**Scope:** Load testing, stress testing, scalability validation

**Tools:** k6 (Grafana k6)

**Location:** `Backend/performance-tests/`

**Test Scenarios:**

1. **Authentication Load Test**
   - 100 concurrent users logging in
   - Ramp-up time: 30 seconds
   - Duration: 5 minutes
   - Target: < 200ms P95 response time

2. **Student CRUD Stress Test**
   - 50 concurrent users creating students
   - 1000+ students in database
   - Pagination performance validation
   - Target: < 500ms P95 response time

3. **SignalR Chat Scalability**
   - 10 concurrent chat rooms
   - 20 users per room
   - 100 messages per minute
   - Target: Message delivery < 100ms

4. **Video Call Connection Test**
   - 5 concurrent video rooms
   - 10 participants per room
   - WebRTC signaling performance
   - Target: Connection established < 2 seconds

**Sample k6 Script:**
```javascript
// auth-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 100 }, // Ramp-up
    { duration: '5m', target: 100 },  // Sustained load
    { duration: '30s', target: 0 },   // Ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<200'], // 95% < 200ms
    http_req_failed: ['rate<0.01'],   // Error rate < 1%
  },
};

export default function () {
  const url = 'https://localhost:7266/api/Auth/login';
  const payload = JSON.stringify({
    userName: `testuser${__VU}`,
    password: 'Test123',
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const response = http.post(url, payload, params);

  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
  });

  sleep(1);
}
```

---

## Testing Tools

### Primary Tools

| Category | Tool | Purpose |
|----------|------|---------|
| **Unit Testing** | xUnit.net | .NET test framework |
| **Mocking** | Moq | Mock dependencies |
| **Integration Testing** | WebApplicationFactory | In-memory API testing |
| **API Testing** | Postman/Newman | Manual and automated API tests |
| **Performance Testing** | k6 | Load and stress testing |
| **Contract Testing** | Postman + OpenAPI | Schema validation |
| **Coverage** | Coverlet + ReportGenerator | Code coverage reports |
| **CI/CD** | GitHub Actions | Automated test execution |

### Tool Configuration

**xUnit Configuration (xunit.runner.json):**
```json
{
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4,
  "diagnosticMessages": false,
  "methodDisplay": "method"
}
```

**Coverlet Configuration (.runsettings):**
```xml
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura,json</Format>
          <Exclude>[*.Tests]*,[*]*.Migrations.*</Exclude>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

---

## Test Data Management

### Strategy

1. **Separate Test Database**
   - Database: `SMSTestDB`
   - Connection String: Environment variable `ConnectionStrings__TestConnection`
   - Reset before each test suite

2. **Seed Data**
   - Use `DataSeeder` project for consistent data
   - Known test users with predictable credentials
   - Reference data (schools, classes)
   - Isolation between tests

3. **Test Users**

| Username | Password | Role | Email | Purpose |
|----------|----------|------|-------|---------|
| `admin_test` | `Admin123!` | Admin | admin@test.sms.edu | Full access testing |
| `teacher_test` | `Teacher123!` | Teacher | teacher@test.sms.edu | Teacher workflow testing |
| `student_test` | `Student123!` | Student | student@test.sms.edu | Student workflow testing |
| `principal_test` | `Principal123!` | Principal | principal@test.sms.edu | Principal access testing |

4. **Test Data Cleanup**

**After Each Test:**
```csharp
public class DatabaseFixture : IDisposable
{
    public DataContext DbContext { get; private set; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SMSTestDB;")
            .Options;
        
        DbContext = new DataContext(options);
        DbContext.Database.EnsureCreated();
        SeedTestData();
    }
    
    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
    
    private void SeedTestData()
    {
        // Seed known test users, schools, classes
    }
}
```

### Data Generation

**Bogus Library for Fake Data:**
```csharp
using Bogus;

public static class TestDataGenerator
{
    public static List<Student> GenerateStudents(int count)
    {
        var studentFaker = new Faker<Student>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.DateOfBirth, f => f.Date.Past(18, DateTime.Now.AddYears(-6)))
            .RuleFor(s => s.Gender, f => f.PickRandom("Male", "Female"))
            .RuleFor(s => s.PhoneNumber, f => f.Phone.PhoneNumber());
        
        return studentFaker.Generate(count);
    }
}
```

---

## Environment Strategy

### Environment Configuration

| Environment | Base URL | Database | Purpose |
|-------------|----------|----------|---------|
| **Local Dev** | http://localhost:7266 | SMSDevDB | Developer testing |
| **Test** | http://localhost:7266 | SMSTestDB | Automated tests |
| **Staging** | https://staging-api.sms.edu | SMSStagingDB | Pre-production validation |
| **Production** | https://api.sms.edu | SMSProductionDB | Live system |

### Postman Environments

**Development Environment:**
```json
{
  "name": "SMS API - Test",
  "values": [
    { "key": "baseUrl", "value": "https://localhost:7266", "enabled": true },
    { "key": "authToken", "value": "", "enabled": true },
    { "key": "adminUsername", "value": "admin_test", "enabled": true },
    { "key": "adminPassword", "value": "Admin123!", "enabled": true },
    { "key": "schoolId", "value": "", "enabled": true },
    { "key": "studentId", "value": "", "enabled": true },
    { "key": "teacherId", "value": "", "enabled": true },
    { "key": "classId", "value": "", "enabled": true },
    { "key": "roomId", "value": "", "enabled": true }
  ]
}
```

**Pre-request Script (Auto Token Refresh):**
```javascript
// Check if token is expired or missing
const tokenExpiry = pm.environment.get("tokenExpiry");
const now = new Date().getTime();

if (!tokenExpiry || now > tokenExpiry) {
    // Login to get new token
    pm.sendRequest({
        url: pm.environment.get("baseUrl") + "/api/Auth/login",
        method: "POST",
        header: "Content-Type: application/json",
        body: {
            mode: "raw",
            raw: JSON.stringify({
                userName: pm.environment.get("adminUsername"),
                password: pm.environment.get("adminPassword")
            })
        }
    }, (err, response) => {
        if (!err && response.code === 200) {
            const jsonData = response.json();
            pm.environment.set("authToken", jsonData.token);
            pm.environment.set("tokenExpiry", now + (2.5 * 60 * 60 * 1000));
        }
    });
}
```

---

## CI/CD Integration

### GitHub Actions Workflow

**File:** `.github/workflows/api-tests.yml`

```yaml
name: API Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
      working-directory: ./Backend
    
    - name: Build
      run: dotnet build --no-restore
      working-directory: ./Backend
    
    - name: Run Unit Tests
      run: |
        dotnet test \
          --no-build \
          --verbosity normal \
          --collect:"XPlat Code Coverage" \
          --results-directory ./coverage
      working-directory: ./Backend
    
    - name: Generate Coverage Report
      uses: danielpalme/ReportGenerator-GitHub-Action@5
      with:
        reports: './Backend/coverage/**/coverage.cobertura.xml'
        targetdir: './coverage-report'
        reporttypes: 'HtmlInline;Cobertura'
    
    - name: Upload Coverage Report
      uses: actions/upload-artifact@v3
      with:
        name: coverage-report
        path: ./coverage-report
    
    - name: Check Coverage Threshold
      run: |
        # Extract coverage percentage
        COVERAGE=$(grep -oP 'line-rate="\K[0-9.]+' coverage-report/Cobertura.xml | head -1)
        COVERAGE_PCT=$(echo "$COVERAGE * 100" | bc)
        echo "Coverage: $COVERAGE_PCT%"
        
        # Fail if below 80%
        if (( $(echo "$COVERAGE_PCT < 80" | bc -l) )); then
          echo "Coverage below 80% threshold"
          exit 1
        fi

  integration-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: Test123!@#
        ports:
          - 1433:1433
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Run Integration Tests
      run: |
        dotnet test \
          --filter Category=Integration \
          --verbosity normal
      working-directory: ./Backend
      env:
        ConnectionStrings__TestConnection: Server=localhost,1433;Database=SMSTestDB;User Id=sa;Password=Test123!@#;TrustServerCertificate=True

  api-tests:
    runs-on: ubuntu-latest
    needs: integration-tests
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Start API Server
      run: |
        dotnet run --project Backend/SMSPrototype1/SMSPrototype1.csproj &
        sleep 30 # Wait for server to start
    
    - name: Install Newman
      run: npm install -g newman newman-reporter-htmlextra
    
    - name: Run Postman Collection
      run: |
        newman run docs/testing/postman/SMS_API_Tests.json \
          -e docs/testing/postman/Test_Environment.json \
          --reporters cli,htmlextra \
          --reporter-htmlextra-export ./newman-report.html
    
    - name: Upload Newman Report
      uses: actions/upload-artifact@v3
      with:
        name: newman-report
        path: ./newman-report.html

  performance-tests:
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup k6
      run: |
        sudo gpg -k
        sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
        echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
        sudo apt-get update
        sudo apt-get install k6
    
    - name: Run Performance Tests
      run: |
        k6 run Backend/performance-tests/auth-load-test.js --out json=performance-results.json
    
    - name: Upload Performance Report
      uses: actions/upload-artifact@v3
      with:
        name: performance-results
        path: ./performance-results.json
```

---

## Test Coverage Goals

### Coverage Targets

| Component | Target | Measurement |
|-----------|--------|-------------|
| **Services** | 90% | Line coverage |
| **Repositories** | 85% | Line coverage |
| **Controllers** | 80% | Line coverage |
| **Hubs** | 75% | Line coverage |
| **Overall** | 80% | Line coverage |

### Critical Path Coverage

**Must be 100% covered:**
- Authentication and authorization
- Password hashing and encryption
- JWT token generation and validation
- Payment processing (if applicable)
- Data encryption/decryption

### Coverage Exclusions

- Migrations
- Auto-generated code
- DTOs (Data Transfer Objects)
- Configuration classes
- Program.cs/Startup.cs

---

## Reporting and Metrics

### Test Reports

1. **Unit Test Results** (JUnit XML)
   - Test execution summary
   - Pass/fail counts
   - Execution time per test
   - Stack traces for failures

2. **Coverage Reports** (HTML + Cobertura)
   - Line coverage percentage
   - Branch coverage
   - Uncovered lines highlighted
   - Trends over time

3. **Newman API Test Reports** (HTML)
   - Request/response logs
   - Assertion results
   - Response times
   - Error details

4. **k6 Performance Reports** (JSON + HTML)
   - Request rate (RPS)
   - Response times (P50, P95, P99)
   - Error rate
   - Resource utilization

### Metrics Dashboard

**Key Metrics to Track:**
- Test pass rate (target: > 95%)
- Code coverage (target: > 80%)
- Average test execution time
- API response times (P95 < 500ms)
- Defect escape rate
- Test flakiness rate (< 2%)

### Continuous Monitoring

**Grafana Dashboard (with k6 integration):**
- Real-time performance metrics
- Historical trend analysis
- Alert on threshold violations
- Comparative analysis (current vs baseline)

---

## Next Steps

### Immediate Actions

1. ✅ Set up test projects in Backend solution
2. ✅ Create test database and seed scripts
3. ✅ Implement unit tests for core services
4. ✅ Configure GitHub Actions workflow
5. ✅ Create Postman collections for E2E tests

### Short-term (1-2 weeks)

- Complete integration tests for all controllers
- Set up k6 performance baseline
- Configure code coverage reporting
- Document test case templates

### Long-term (1-3 months)

- Implement contract testing automation
- Set up continuous performance monitoring
- Create test data generation utilities
- Establish regression test suite

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Owner:** QA Team  
**Related Docs:**
- [Postman Guide](./POSTMAN_GUIDE.md)
- [Test Cases](./test-cases/)
- [Performance Tests](../../Backend/performance-tests/)
