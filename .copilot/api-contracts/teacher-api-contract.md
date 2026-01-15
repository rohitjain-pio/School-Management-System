# API Contract: Teacher Management
**Version:** 1.0  
**Base URL:** `/api/teachers`  
**Authentication:** JWT Bearer Token (Required)

---

## 📋 Overview

Complete API specification for Teacher CRUD operations with role-based access control and multi-tenant isolation.

**Resource:** Teacher  
**Endpoints:** 7 endpoints  
**Rate Limit:** 100 requests/minute per school

---

## 🔐 Authentication

All endpoints require JWT Bearer token with `SchoolId` claim:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 Data Models

### Teacher (Response)

```typescript
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "userId": "7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f",
  "employeeId": "TCH-2024-001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "hireDate": "2024-01-15T00:00:00Z",
  "salary": 65000.00,
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
| userId | GUID | Yes | - | Associated user account |
| employeeId | string | Yes (auto) | 50 | Format: TCH-YYYY-XXX |
| firstName | string | Yes | 100 | Letters and spaces |
| lastName | string | Yes | 100 | Letters and spaces |
| email | string | Yes | 200 | Must be unique within school |
| phoneNumber | string | No | 20 | E.164 format |
| department | string | Yes | 100 | Math, Science, English, etc. |
| subject | string | Yes | 100 | Specific subject taught |
| hireDate | DateTime | Yes | - | Cannot be future date |
| salary | decimal | Yes | - | Admin only (hidden for Teacher role) |
| isActive | boolean | Yes (auto) | - | Default: true |
| createdAt | DateTime | Yes (auto) | - | UTC timestamp |
| updatedAt | DateTime | No | - | UTC timestamp |

---

### CreateTeacherDto (Request)

```typescript
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "hireDate": "2024-01-15",
  "salary": 65000.00
}
```

**Validation Rules:**
- `firstName`: Required, 1-100 chars, `^[a-zA-Z\s]+$`
- `lastName`: Required, 1-100 chars, `^[a-zA-Z\s]+$`
- `email`: Required, valid email, unique within school
- `phoneNumber`: Optional, E.164 format
- `department`: Required, 1-100 chars
- `subject`: Required, 1-100 chars
- `hireDate`: Required, not in future
- `salary`: Required, > 0, < 1,000,000

---

### UpdateTeacherDto (Request)

```typescript
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "salary": 70000.00,
  "isActive": true
}
```

**All fields optional** - Partial updates supported.

**Role-Based Restrictions:**
- **Admin:** Can update all fields including salary
- **Teacher:** Can only update email, phoneNumber (own record only)

---

### TeacherListDto (Simplified for Lists)

```typescript
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeId": "TCH-2024-001",
  "firstName": "Jane",
  "lastName": "Smith",
  "department": "Mathematics",
  "subject": "Algebra",
  "isActive": true
}
```

**Note:** Salary field excluded (performance + security).

---

## 🔗 Endpoints

### 1. Get All Teachers (Paginated)

**Request:**
```http
GET /api/teachers?pageNumber=1&pageSize=50&department=Mathematics
Authorization: Bearer {token}
```

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| pageNumber | int | No | 1 | Page number (1-based) |
| pageSize | int | No | 50 | Items per page (max 100) |
| department | string | No | - | Filter by department |
| searchTerm | string | No | - | Search firstName, lastName, email |
| activeOnly | bool | No | true | Show only active teachers |

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "employeeId": "TCH-2024-001",
      "firstName": "Jane",
      "lastName": "Smith",
      "department": "Mathematics",
      "subject": "Algebra",
      "isActive": true
    }
  ],
  "totalCount": 75,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 2
}
```

**Performance:**
- Expected Response Time: < 200ms
- Cache Duration: 5 minutes

---

### 2. Get Teacher by ID

**Request:**
```http
GET /api/teachers/{id}
Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "userId": "7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f",
  "employeeId": "TCH-2024-001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "hireDate": "2024-01-15T00:00:00Z",
  "salary": 65000.00,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

**Note:** `salary` field only returned for Admin role.

**Errors:**
- `401 Unauthorized`: Missing/invalid token
- `404 Not Found`: Teacher not found or different school
- `403 Forbidden`: Insufficient permissions

---

### 3. Create Teacher

**Request:**
```http
POST /api/teachers
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "hireDate": "2024-01-15",
  "salary": 65000.00
}
```

**Response: 201 Created**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "schoolId": "11111111-1111-1111-1111-111111111111",
  "userId": "7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f",
  "employeeId": "TCH-2024-001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@school.com",
  "phoneNumber": "+1234567890",
  "department": "Mathematics",
  "subject": "Algebra",
  "hireDate": "2024-01-15T00:00:00Z",
  "salary": 65000.00,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

**What Happens:**
1. Creates User account with "Teacher" role
2. Creates Teacher record linked to User
3. Generates employeeId (TCH-YYYY-XXX)
4. Sends welcome email with login credentials

**Authorization:**
- Only Admin can create teachers

**Errors:**
- `401 Unauthorized`: Missing/invalid token
- `403 Forbidden`: Non-admin user
- `400 Bad Request`: Validation errors

---

### 4. Update Teacher

**Request:**
```http
PUT /api/teachers/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "salary": 70000.00,
  "department": "Advanced Mathematics"
}
```

**Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "salary": 70000.00,
  "department": "Advanced Mathematics",
  "updatedAt": "2024-01-25T16:20:00Z"
}
```

**Authorization Matrix:**

| Field | Admin | Teacher (Own Record) |
|-------|-------|----------------------|
| firstName | ✅ | ❌ |
| lastName | ✅ | ❌ |
| email | ✅ | ✅ |
| phoneNumber | ✅ | ✅ |
| department | ✅ | ❌ |
| subject | ✅ | ❌ |
| salary | ✅ | ❌ |
| isActive | ✅ | ❌ |

---

### 5. Delete Teacher (Soft Delete)

**Request:**
```http
DELETE /api/teachers/{id}
Authorization: Bearer {token}
```

**Response: 204 No Content**

**Authorization:**
- Only Admin can delete teachers

**Errors:**
- `401 Unauthorized`: Missing/invalid token
- `403 Forbidden`: Non-admin user
- `404 Not Found`: Teacher not found or different school
- `409 Conflict`: Cannot delete teacher with active classes

---

### 6. Get Teachers by Department

**Request:**
```http
GET /api/teachers/department/{department}
Authorization: Bearer {token}
```

**Response: 200 OK**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "employeeId": "TCH-2024-001",
    "firstName": "Jane",
    "lastName": "Smith",
    "subject": "Algebra",
    "isActive": true
  }
]
```

**Common Departments:**
- Mathematics
- Science
- English
- History
- Physical Education
- Arts
- Computer Science

---

### 7. Get Teacher Statistics

**Request:**
```http
GET /api/teachers/statistics
Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "totalTeachers": 75,
  "activeTeachers": 68,
  "inactiveTeachers": 7,
  "byDepartment": {
    "Mathematics": 15,
    "Science": 12,
    "English": 10,
    "History": 8,
    "Physical Education": 6,
    "Arts": 5,
    "Computer Science": 4,
    "Other": 15
  },
  "averageSalary": 58500.00,
  "averageTenure": 4.2
}
```

**Authorization:**
- Only Admin can view statistics

---

## 🔒 Security

### Multi-Tenant Isolation

All endpoints filter by `SchoolId` from JWT token automatically.

### Authorization Matrix

| Endpoint | Admin | Teacher | Student |
|----------|-------|---------|---------|
| GET /teachers | ✅ | ✅ | ❌ |
| GET /teachers/{id} | ✅ | ✅ (own) | ❌ |
| POST /teachers | ✅ | ❌ | ❌ |
| PUT /teachers/{id} | ✅ | ✅ (own, limited) | ❌ |
| DELETE /teachers/{id} | ✅ | ❌ | ❌ |
| GET /teachers/department/{dept} | ✅ | ✅ | ❌ |
| GET /teachers/statistics | ✅ | ❌ | ❌ |

### Salary Field Protection

- **Admin:** Sees salary in all responses
- **Teacher:** Never sees salary (even own record)
- **Student:** No access to teachers endpoint

---

## ⚡ Performance

### Response Time Targets

| Endpoint | P50 | P95 | P99 |
|----------|-----|-----|-----|
| GET /teachers | 95ms | 140ms | 200ms |
| GET /teachers/{id} | 50ms | 80ms | 110ms |
| POST /teachers | 180ms | 250ms | 320ms |
| PUT /teachers/{id} | 110ms | 170ms | 230ms |
| DELETE /teachers/{id} | 85ms | 120ms | 160ms |

---

## 🧪 Example Usage

### TypeScript (Frontend)

```typescript
import { api } from '@/lib/api-client';

// Get all teachers
const response = await api.get<PagedResult<TeacherListDto>>('/teachers', {
  params: { 
    pageNumber: 1, 
    pageSize: 50, 
    department: 'Mathematics' 
  }
});

// Create teacher (Admin only)
const newTeacher = await api.post<Teacher>('/teachers', {
  firstName: 'Jane',
  lastName: 'Smith',
  email: 'jane.smith@school.com',
  department: 'Mathematics',
  subject: 'Algebra',
  hireDate: '2024-01-15',
  salary: 65000.00
});

// Update teacher
const updated = await api.put<Teacher>(`/teachers/${id}`, {
  salary: 70000.00
});

// Get statistics (Admin only)
const stats = await api.get<TeacherStatistics>('/teachers/statistics');
```

---

**Related Files:**
- `.copilot/api-contracts/student-api-contract.md`
- `.copilot/api-contracts/authentication-contract.md`
- `.copilot/workflows/add-new-controller.md`
