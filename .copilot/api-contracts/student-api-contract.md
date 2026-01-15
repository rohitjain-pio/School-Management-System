# API Contract: Student Management
**Version:** 1.0  
**Base URL:** `/api/students`  
**Authentication:** JWT Bearer Token (Required)

---

## 📋 Overview

Complete API specification for Student CRUD operations with multi-tenant isolation.

**Resource:** Student  
**Endpoints:** 6 endpoints  
**Rate Limit:** 100 requests/minute per school

---

## 🔐 Authentication

All endpoints require JWT Bearer token with `SchoolId` claim:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Required Claims:**
- `NameIdentifier` (User ID)
- `SchoolId` (Multi-tenant discriminator)
- `Role` (Admin, Teacher, Student)

---

## 📊 Data Models

### Student (Response)

```typescript
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15T00:00:00Z",
  "enrollmentDate": "2024-09-01T00:00:00Z",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z"
}
```

**Field Specifications:**

| Field | Type | Required | Max Length | Notes |
|-------|------|----------|------------|-------|
| id | GUID | Yes (auto) | - | Primary key |
| schoolId | GUID | Yes (auto) | - | From JWT token |
| firstName | string | Yes | 100 | Letters and spaces only |
| lastName | string | Yes | 100 | Letters and spaces only |
| email | string | Yes | 200 | Must be unique within school |
| phoneNumber | string | No | 20 | E.164 format preferred |
| dateOfBirth | DateTime | Yes | - | Must be 5-100 years ago |
| enrollmentDate | DateTime | Yes | - | Cannot be future date |
| isActive | boolean | Yes (auto) | - | Default: true |
| createdAt | DateTime | Yes (auto) | - | UTC timestamp |
| updatedAt | DateTime | No | - | UTC timestamp |

---

### CreateStudentDto (Request)

```typescript
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15",
  "enrollmentDate": "2024-09-01"
}
```

**Validation Rules:**
- `firstName`: Required, 1-100 chars, `^[a-zA-Z\s]+$`
- `lastName`: Required, 1-100 chars, `^[a-zA-Z\s]+$`
- `email`: Required, valid email, 1-200 chars, unique within school
- `phoneNumber`: Optional, `^\+?[1-9]\d{1,14}$` (E.164)
- `dateOfBirth`: Required, 5-100 years ago
- `enrollmentDate`: Required, not in future

---

### UpdateStudentDto (Request)

```typescript
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15",
  "isActive": true
}
```

**All fields optional** (partial update supported)

---

### PagedResult<Student>

```typescript
{
  "items": [
    { /* Student object */ },
    { /* Student object */ }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 🔗 Endpoints

### 1. Get All Students (Paginated)

**Request:**
```http
GET /api/students?pageNumber=1&pageSize=50&searchTerm=John
Authorization: Bearer {token}
```

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| pageNumber | int | No | 1 | Page number (1-based) |
| pageSize | int | No | 50 | Items per page (max 100) |
| searchTerm | string | No | - | Search in firstName, lastName, email |

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "schoolId": "11111111-1111-1111-1111-111111111111",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@school.com",
      "phoneNumber": "+1234567890",
      "dateOfBirth": "2005-08-15T00:00:00Z",
      "enrollmentDate": "2024-09-01T00:00:00Z",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `400 Bad Request`: Invalid query parameters

**Performance:**
- Cache Duration: 5 minutes
- Expected Response Time: < 200ms
- Max Records Returned: 100 per page

---

### 2. Get Student by ID

**Request:**
```http
GET /api/students/{id}
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (GUID, required): Student ID

**Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15T00:00:00Z",
  "enrollmentDate": "2024-09-01T00:00:00Z",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z"
}
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Student not found or belongs to different school
- `400 Bad Request`: Invalid ID format

**Performance:**
- Expected Response Time: < 100ms

---

### 3. Create Student

**Request:**
```http
POST /api/students
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15",
  "enrollmentDate": "2024-09-01"
}
```

**Response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15T00:00:00Z",
  "enrollmentDate": "2024-09-01T00:00:00Z",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

**Response Headers:**
```http
Location: /api/students/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `400 Bad Request`: Validation errors
  ```json
  {
    "errors": {
      "FirstName": ["First name is required"],
      "Email": ["Email is already in use"]
    }
  }
  ```
- `403 Forbidden`: Insufficient permissions (Student role cannot create)

**Validation Errors:**
- First name required
- Last name required
- Email required and must be unique within school
- Invalid email format
- Date of birth must be 5-100 years ago
- Enrollment date cannot be in future
- Phone number must match E.164 format (if provided)

**Performance:**
- Expected Response Time: < 150ms

---

### 4. Update Student

**Request:**
```http
PUT /api/students/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15",
  "isActive": true
}
```

**All fields optional** - Only provided fields will be updated.

**Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@school.com",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "2005-08-15T00:00:00Z",
  "enrollmentDate": "2024-09-01T00:00:00Z",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-25T16:20:00Z"
}
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Student not found or belongs to different school
- `400 Bad Request`: Validation errors
- `403 Forbidden`: Insufficient permissions

**Performance:**
- Expected Response Time: < 150ms

---

### 5. Delete Student (Soft Delete)

**Request:**
```http
DELETE /api/students/{id}
Authorization: Bearer {token}
```

**Response: 204 No Content**

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `404 Not Found`: Student not found or belongs to different school
- `403 Forbidden`: Insufficient permissions (only Admin can delete)
- `409 Conflict`: Cannot delete student with active enrollments

**Notes:**
- This is a **soft delete** (sets `isActive = false`)
- Student data is retained for audit purposes
- Student will no longer appear in default queries

**Performance:**
- Expected Response Time: < 100ms

---

### 6. Get Student Count

**Request:**
```http
GET /api/students/count?activeOnly=true
Authorization: Bearer {token}
```

**Query Parameters:**
- `activeOnly` (boolean, optional): Count only active students (default: true)

**Response: 200 OK**
```json
{
  "count": 1250
}
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token

**Performance:**
- Expected Response Time: < 50ms

---

## 🔒 Security

### Multi-Tenant Isolation

**CRITICAL:** All endpoints automatically filter by `SchoolId` from JWT token.

- School A cannot access School B's students
- `schoolId` is NEVER accepted from request body
- Returns `404 Not Found` (not `403`) when accessing other school's data

### Authorization Matrix

| Endpoint | Admin | Teacher | Student |
|----------|-------|---------|---------|
| GET /students | ✅ | ✅ | ❌ |
| GET /students/{id} | ✅ | ✅ | ✅ (own record) |
| POST /students | ✅ | ❌ | ❌ |
| PUT /students/{id} | ✅ | ✅ (limited fields) | ✅ (own record, limited) |
| DELETE /students/{id} | ✅ | ❌ | ❌ |
| GET /students/count | ✅ | ✅ | ❌ |

---

## ⚡ Performance

### Response Time Targets

| Endpoint | P50 | P95 | P99 |
|----------|-----|-----|-----|
| GET /students | 85ms | 120ms | 180ms |
| GET /students/{id} | 45ms | 75ms | 95ms |
| POST /students | 110ms | 160ms | 220ms |
| PUT /students/{id} | 95ms | 140ms | 190ms |
| DELETE /students/{id} | 80ms | 110ms | 150ms |

### Caching

- GET /students: 5 minutes (VaryByHeader: Authorization)
- GET /students/{id}: 5 minutes
- Cache invalidated on POST/PUT/DELETE

### Rate Limiting

- 100 requests/minute per school
- 429 Too Many Requests if exceeded

---

## 🧪 Example Usage

### TypeScript (Frontend)

```typescript
import { api } from '@/lib/api-client';

// Get all students
const response = await api.get<PagedResult<Student>>('/students', {
  params: { pageNumber: 1, pageSize: 50, searchTerm: 'John' }
});
const students = response.data.items;

// Get by ID
const student = await api.get<Student>(`/students/${id}`);

// Create
const newStudent = await api.post<Student>('/students', {
  firstName: 'John',
  lastName: 'Doe',
  email: 'john.doe@school.com',
  dateOfBirth: '2005-08-15',
  enrollmentDate: '2024-09-01'
});

// Update
const updated = await api.put<Student>(`/students/${id}`, {
  lastName: 'Smith'
});

// Delete
await api.delete(`/students/${id}`);
```

### C# (Backend Service)

```csharp
// Repository
public async Task<IEnumerable<Student>> GetAllAsync(Guid schoolId, int pageNumber, int pageSize)
{
    return await _context.Students
        .Where(s => s.SchoolId == schoolId && s.IsActive == true)
        .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();
}

// Controller
[Authorize]
[HttpGet]
public async Task<ActionResult<PagedResult<StudentDto>>> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? searchTerm = null)
{
    var schoolId = GetSchoolId(); // From JWT token
    var students = await _studentService.GetAllAsync(schoolId, pageNumber, pageSize, searchTerm);
    return Ok(students);
}
```

---

## 🐛 Error Response Format

All error responses follow this format:

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "FirstName": ["First name is required"],
    "Email": ["Email is already in use"]
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/students"
}
```

---

## 📝 Change Log

### Version 1.0 (2024-01-15)
- Initial release
- 6 endpoints (CRUD + Count)
- Multi-tenant isolation
- Pagination support
- Search functionality

---

**Related Files:**
- `.copilot/api-contracts/teacher-api-contract.md`
- `.copilot/api-contracts/authentication-contract.md`
- `.copilot/workflows/add-new-controller.md`
