# Workflow: Add New API Controller

## Prerequisites
- [ ] Entity model exists in `SMSDataModel/Model/`
- [ ] Repository exists in `SMSRepository/Repository/`
- [ ] Service exists in `SMSServices/Services/`
- [ ] DTOs created in respective projects

---

## Step-by-Step Process

### Step 1: Create Controller File (5 min)

**Location:** `Backend/SMSPrototype1/Controllers/{EntityName}Controller.cs`

**Template:**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMSServices.ServicesInterfaces;
using SMSDataModel.Model.{EntityName}Dto;
using System.Security.Claims;

namespace SMSPrototype1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class {EntityName}Controller : BaseSchoolController
    {
        private readonly I{EntityName}Service _{entityName}Service;
        private readonly ILogger<{EntityName}Controller> _logger;

        public {EntityName}Controller(
            I{EntityName}Service {entityName}Service,
            ILogger<{EntityName}Controller> logger)
        {
            _{entityName}Service = {entityName}Service;
            _logger = logger;
        }

        // GET: api/{entityname}
        [HttpGet]
        [Authorize(Policy = "RequireSchoolStaff")]
        public async Task<IActionResult> GetAll()
        {
            var schoolId = GetSchoolIdFromClaims();
            
            if (schoolId == Guid.Empty)
            {
                return ForbidSchoolAccess("Invalid SchoolId");
            }

            var items = await _{entityName}Service.GetAllAsync(schoolId);
            
            _logger.LogInformation(
                "User {UserId} retrieved {Count} {EntityName} records from school {SchoolId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                items.Count(),
                "{EntityName}",
                schoolId
            );

            return Ok(items);
        }

        // GET: api/{entityname}/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireSchoolStaff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var schoolId = GetSchoolIdFromClaims();
            
            if (schoolId == Guid.Empty)
            {
                return ForbidSchoolAccess();
            }

            var item = await _{entityName}Service.GetByIdAsync(id, schoolId);
            
            if (item == null)
            {
                return NotFound($"{EntityName} {id} not found");
            }

            return Ok(item);
        }

        // POST: api/{entityname}
        [HttpPost]
        [Authorize(Policy = "RequireSchoolStaff")]
        public async Task<IActionResult> Create([FromBody] Create{EntityName}Dto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schoolId = GetSchoolIdFromClaims();
            
            if (schoolId == Guid.Empty)
            {
                return ForbidSchoolAccess();
            }

            var created = await _{entityName}Service.CreateAsync(dto, schoolId);
            
            _logger.LogInformation(
                "User {UserId} created {EntityName} {EntityId} in school {SchoolId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                "{EntityName}",
                created.Id,
                schoolId
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        // PUT: api/{entityname}/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireSchoolStaff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Update{EntityName}Dto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schoolId = GetSchoolIdFromClaims();
            
            if (schoolId == Guid.Empty)
            {
                return ForbidSchoolAccess();
            }

            var success = await _{entityName}Service.UpdateAsync(id, dto, schoolId);
            
            if (!success)
            {
                return NotFound($"{EntityName} {id} not found");
            }

            _logger.LogInformation(
                "User {UserId} updated {EntityName} {EntityId} in school {SchoolId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                "{EntityName}",
                id,
                schoolId
            );

            return NoContent();
        }

        // DELETE: api/{entityname}/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireSchoolManagement")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var schoolId = GetSchoolIdFromClaims();
            
            if (schoolId == Guid.Empty)
            {
                return ForbidSchoolAccess();
            }

            var success = await _{entityName}Service.DeleteAsync(id, schoolId);
            
            if (!success)
            {
                return NotFound($"{EntityName} {id} not found");
            }

            _logger.LogWarning(
                "User {UserId} deleted {EntityName} {EntityId} from school {SchoolId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                "{EntityName}",
                id,
                schoolId
            );

            return NoContent();
        }
    }
}
```

---

### Step 2: Add Swagger Documentation (3 min)

Add XML comments for Swagger UI:

```csharp
/// <summary>
/// Manages {EntityName} operations for a school
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class {EntityName}Controller : BaseSchoolController
{
    /// <summary>
    /// Gets all {entityname}s for the authenticated user's school
    /// </summary>
    /// <returns>List of {entityname}s</returns>
    /// <response code="200">Returns the list of {entityname}s</response>
    /// <response code="403">User does not have a valid SchoolId</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<{EntityName}Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        // Implementation
    }

    /// <summary>
    /// Gets a specific {entityname} by ID
    /// </summary>
    /// <param name="id">The {entityname} ID</param>
    /// <returns>The {entityname} details</returns>
    /// <response code="200">Returns the {entityname}</response>
    /// <response code="404">{EntityName} not found or belongs to different school</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof({EntityName}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Implementation
    }

    /// <summary>
    /// Creates a new {entityname}
    /// </summary>
    /// <param name="dto">The {entityname} data</param>
    /// <returns>The created {entityname}</returns>
    /// <response code="201">{EntityName} created successfully</response>
    /// <response code="400">Invalid input data</response>
    [HttpPost]
    [ProducesResponseType(typeof({EntityName}Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Create{EntityName}Dto dto)
    {
        // Implementation
    }
}
```

---

### Step 3: Register Service in DI Container (2 min)

**Location:** `Backend/SMSPrototype1/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ... existing services
        services.AddScoped<I{EntityName}Service, {EntityName}Service>();
        
        return services;
    }
}
```

**Or in `Program.cs` directly:**
```csharp
builder.Services.AddScoped<I{EntityName}Service, {EntityName}Service>();
```

---

### Step 4: Test Endpoints (15 min)

#### Test 1: Compile & Run
```bash
cd Backend/SMSPrototype1
dotnet build
# Should compile without errors

dotnet run
# Should start on https://localhost:7266
```

#### Test 2: Swagger UI
1. Navigate to https://localhost:7266/swagger
2. Verify new controller appears
3. Check all endpoints are listed

#### Test 3: Postman Tests

**Setup:**
```http
# 1. Login to get token
POST https://localhost:7266/api/auth/login
Content-Type: application/json

{
  "email": "admin@testschool.com",
  "password": "Test@123"
}

Response:
{
  "token": "eyJhbGc...",
  "schoolId": "12345678-1234-1234-1234-123456789abc"
}
```

**Test CREATE:**
```http
POST https://localhost:7266/api/{entityname}
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "name": "Test {EntityName}",
  // ... other fields
}

Expected: 201 Created
```

**Test GET ALL:**
```http
GET https://localhost:7266/api/{entityname}
Authorization: Bearer eyJhbGc...

Expected: 200 OK with array of items
```

**Test GET BY ID:**
```http
GET https://localhost:7266/api/{entityname}/{id}
Authorization: Bearer eyJhbGc...

Expected: 200 OK with item details
```

**Test UPDATE:**
```http
PUT https://localhost:7266/api/{entityname}/{id}
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "name": "Updated Name",
  // ... other fields
}

Expected: 204 No Content
```

**Test DELETE:**
```http
DELETE https://localhost:7266/api/{entityname}/{id}
Authorization: Bearer eyJhbGc...

Expected: 204 No Content
```

---

### Step 5: Security Tests (10 min)

#### Test Cross-School Access Prevention

**Scenario:** User from School A tries to access School B's data

```http
# 1. Login as School A admin
POST https://localhost:7266/api/auth/login
{
  "email": "adminA@schoolA.com",
  "password": "Test@123"
}
# Save token as TOKEN_A

# 2. Create item in School A
POST https://localhost:7266/api/{entityname}
Authorization: Bearer TOKEN_A
{
  "name": "School A Item"
}
# Note the ID, e.g., ID_A

# 3. Login as School B admin
POST https://localhost:7266/api/auth/login
{
  "email": "adminB@schoolB.com",
  "password": "Test@123"
}
# Save token as TOKEN_B

# 4. Try to access School A's item with School B's token
GET https://localhost:7266/api/{entityname}/ID_A
Authorization: Bearer TOKEN_B

Expected: 404 Not Found (not 403, to avoid leaking existence)
```

#### Test Unauthorized Access

```http
# Try to access without token
GET https://localhost:7266/api/{entityname}

Expected: 401 Unauthorized
```

#### Test Insufficient Role

```http
# Login as Student (if endpoint requires SchoolStaff)
POST https://localhost:7266/api/auth/login
{
  "email": "student@school.com",
  "password": "Test@123"
}

GET https://localhost:7266/api/{entityname}
Authorization: Bearer {student_token}

Expected: 403 Forbidden
```

---

### Step 6: Write Unit Tests (20 min)

**Location:** `Backend/SMSPrototype1.Tests/Controllers/{EntityName}ControllerTests.cs`

```csharp
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class {EntityName}ControllerTests
{
    private readonly Mock<I{EntityName}Service> _mockService;
    private readonly Mock<ILogger<{EntityName}Controller>> _mockLogger;
    private readonly {EntityName}Controller _controller;
    private readonly Guid _schoolId = Guid.NewGuid();

    public {EntityName}ControllerTests()
    {
        _mockService = new Mock<I{EntityName}Service>();
        _mockLogger = new Mock<ILogger<{EntityName}Controller>>();
        _controller = new {EntityName}Controller(_mockService.Object, _mockLogger.Object);

        // Setup user with SchoolId claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("SchoolId", _schoolId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetAll_WithValidSchoolId_ReturnsOkResult()
    {
        // Arrange
        var items = new List<{EntityName}Dto>
        {
            new {EntityName}Dto { Id = Guid.NewGuid(), Name = "Item 1" },
            new {EntityName}Dto { Id = Guid.NewGuid(), Name = "Item 2" }
        };
        _mockService.Setup(s => s.GetAllAsync(_schoolId))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<{EntityName}Dto>>(okResult.Value);
        Assert.Equal(2, returnedItems.Count());
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item = new {EntityName}Dto { Id = id, Name = "Test Item" };
        _mockService.Setup(s => s.GetByIdAsync(id, _schoolId))
            .ReturnsAsync(item);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItem = Assert.IsType<{EntityName}Dto>(okResult.Value);
        Assert.Equal(id, returnedItem.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id, _schoolId))
            .ReturnsAsync(({EntityName}Dto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new Create{EntityName}Dto { Name = "New Item" };
        var created = new {EntityName}Dto { Id = Guid.NewGuid(), Name = "New Item" };
        _mockService.Setup(s => s.CreateAsync(dto, _schoolId))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
        var returnedItem = Assert.IsType<{EntityName}Dto>(createdResult.Value);
        Assert.Equal(created.Id, returnedItem.Id);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id, _schoolId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id, _schoolId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
```

**Run Tests:**
```bash
cd Backend/SMSPrototype1.Tests
dotnet test

# Should see:
# Passed: 6
# Failed: 0
```

---

### Step 7: Verification Checklist

Before marking as complete:

- [ ] Controller inherits from `BaseSchoolController`
- [ ] All endpoints require `[Authorize]`
- [ ] SchoolId extracted from JWT claims (never from request body)
- [ ] All repository/service calls include `schoolId` parameter
- [ ] Structured logging present on all CRUD operations
- [ ] Swagger documentation complete
- [ ] Service registered in DI container
- [ ] Application compiles without errors
- [ ] All Postman tests pass
- [ ] Cross-school access blocked
- [ ] Unit tests pass (6+ tests)
- [ ] No hardcoded values or secrets

---

## Common Mistakes to Avoid

### ❌ Accepting SchoolId from Request
```csharp
// WRONG
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] Guid schoolId)
{
    // Security vulnerability!
}
```

### ❌ Not Validating SchoolId
```csharp
// WRONG
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var schoolId = GetSchoolIdFromClaims();
    // Missing validation!
    var items = await _service.GetAllAsync(schoolId);
    return Ok(items);
}

// CORRECT
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var schoolId = GetSchoolIdFromClaims();
    if (schoolId == Guid.Empty)
    {
        return ForbidSchoolAccess();
    }
    var items = await _service.GetAllAsync(schoolId);
    return Ok(items);
}
```

### ❌ Wrong HTTP Status Code
```csharp
// WRONG - Reveals existence of cross-school data
if (item == null)
{
    return StatusCode(403); // Tells attacker "item exists but you can't access it"
}

// CORRECT - Generic not found
if (item == null)
{
    return NotFound(); // Could be non-existent OR cross-school
}
```

---

## Time Estimate

| Step | Time | Cumulative |
|------|------|------------|
| Create Controller | 5 min | 5 min |
| Add Swagger Docs | 3 min | 8 min |
| Register Service | 2 min | 10 min |
| Test Endpoints | 15 min | 25 min |
| Security Tests | 10 min | 35 min |
| Write Unit Tests | 20 min | 55 min |
| Verification | 5 min | **60 min** |

**Total:** ~1 hour per controller
