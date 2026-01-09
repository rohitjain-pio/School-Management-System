# Student API Test Cases

**Module:** Student Management  
**Controller:** StudentController  
**Base Path:** `/api/Student`

---

## Test Cases

### TC-STU-001: Get All Students - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Logged in as Admin/Teacher
- At least 3 students exist in database

**Steps:**
1. Send GET request to `/api/Student`
2. Include auth token

**Expected Result:**
- Status Code: `200 OK`
- Response Body:
  ```json
  {
    "isSuccess": true,
    "data": [
      {
        "id": "<GUID>",
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@sms.edu",
        "dateOfBirth": "2010-05-15",
        "gender": "Male",
        "phoneNumber": "555-0101",
        "classId": "<GUID>",
        "schoolId": "<GUID>"
      }
    ],
    "errorMessage": null
  }
  ```

---

### TC-STU-002: Get All Students - Unauthorized

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Prerequisites:**
- No auth token provided

**Steps:**
1. Send GET request to `/api/Student` without Authorization header

**Expected Result:**
- Status Code: `401 Unauthorized`

---

### TC-STU-003: Get Student by ID - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Student exists with known ID

**Steps:**
1. Send GET request to `/api/Student/{id}`
2. Replace `{id}` with valid student GUID

**Expected Result:**
- Status Code: `200 OK`
- Response contains single student object

---

### TC-STU-004: Get Student by ID - Not Found

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Test Data:**
- Student ID: `00000000-0000-0000-0000-000000000000` (non-existent)

**Expected Result:**
- Status Code: `404 Not Found`
- Error message: "Student not found"

---

### TC-STU-005: Create Student - Success

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Logged in as Admin
- School and Class exist

**Test Data:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@sms.edu",
  "dateOfBirth": "2011-08-20",
  "gender": "Female",
  "phoneNumber": "555-0202",
  "address": "123 Main St",
  "classId": "<VALID_CLASS_GUID>",
  "schoolId": "<VALID_SCHOOL_GUID>",
  "guardianName": "Mary Smith",
  "guardianPhone": "555-0203"
}
```

**Steps:**
1. Send POST request to `/api/Student`
2. Include student data in body

**Expected Result:**
- Status Code: `201 Created`
- Location header contains URL of new student
- Response contains created student with ID

**Postman Test:**
```javascript
pm.test("Student created", () => {
    pm.response.to.have.status(201);
    const jsonData = pm.response.json();
    pm.environment.set("studentId", jsonData.data.id);
});
```

---

### TC-STU-006: Create Student - Duplicate Email

**Priority:** High  
**Type:** Negative  
**Category:** Validation

**Prerequisites:**
- Student with email `existing@sms.edu` already exists

**Test Data:**
```json
{
  "email": "existing@sms.edu",
  "firstName": "Duplicate",
  "lastName": "User"
}
```

**Expected Result:**
- Status Code: `400 Bad Request`
- Error message: "Email already in use"

---

### TC-STU-007: Create Student - Invalid Email Format

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Test Data:**
```json
{
  "email": "invalid-email-format",
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Result:**
- Status Code: `400 Bad Request`
- Error message indicates invalid email format

---

### TC-STU-008: Create Student - Missing Required Fields

**Priority:** High  
**Type:** Negative  
**Category:** Validation

**Test Data:**
```json
{
  "firstName": "Test"
  // Missing lastName, email, etc.
}
```

**Expected Result:**
- Status Code: `400 Bad Request`
- Error messages list all missing required fields

---

### TC-STU-009: Create Student - Invalid Date of Birth

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Test Data:**
```json
{
  "dateOfBirth": "2030-01-01",  // Future date
  "firstName": "Test",
  "lastName": "User"
}
```

**Expected Result:**
- Status Code: `400 Bad Request`
- Error message: "Date of birth cannot be in the future"

---

### TC-STU-010: Update Student - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Student exists with known ID
- Logged in as Admin

**Test Data:**
```json
{
  "id": "<EXISTING_STUDENT_GUID>",
  "firstName": "Updated",
  "lastName": "Name",
  "email": "updated@sms.edu",
  "phoneNumber": "555-9999"
}
```

**Steps:**
1. Send PUT request to `/api/Student/{id}`
2. Include updated student data

**Expected Result:**
- Status Code: `200 OK`
- Response contains updated student data
- Database reflects changes

---

### TC-STU-011: Update Student - Partial Update

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Test Data:**
```json
{
  "id": "<EXISTING_STUDENT_GUID>",
  "phoneNumber": "555-8888"
  // Only updating phone number
}
```

**Expected Result:**
- Status Code: `200 OK`
- Phone number updated
- Other fields remain unchanged

---

### TC-STU-012: Delete Student - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Student exists
- Logged in as Admin

**Steps:**
1. Send DELETE request to `/api/Student/{id}`

**Expected Result:**
- Status Code: `204 No Content` or `200 OK`
- Student removed from database
- Associated records handled appropriately (cascade or prevent delete)

---

### TC-STU-013: Delete Student - Non-existent

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Test Data:**
- Student ID: Non-existent GUID

**Expected Result:**
- Status Code: `404 Not Found`

---

### TC-STU-014: Get Students with Pagination

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Database has 50+ students

**Steps:**
1. Send GET request to `/api/Student?pageNumber=1&pageSize=10`

**Expected Result:**
- Status Code: `200 OK`
- Response contains exactly 10 students
- Pagination metadata included:
  ```json
  {
    "isSuccess": true,
    "data": [...],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalCount": 50,
      "pageSize": 10
    }
  }
  ```

---

### TC-STU-015: Search Students by Name

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Send GET request to `/api/Student?search=John`

**Expected Result:**
- Status Code: `200 OK`
- Response contains only students with "John" in first or last name

---

### TC-STU-016: Filter Students by Class

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Send GET request to `/api/Student?classId={CLASS_GUID}`

**Expected Result:**
- Status Code: `200 OK`
- All returned students belong to specified class

---

### TC-STU-017: Filter Students by Gender

**Priority:** Low  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Send GET request to `/api/Student?gender=Male`

**Expected Result:**
- Status Code: `200 OK`
- All returned students have gender="Male"

---

### TC-STU-018: Get Students - Performance Test

**Priority:** High  
**Type:** Performance  
**Category:** Load Test

**Prerequisites:**
- Database has 1000+ students

**Steps:**
1. Send GET request to `/api/Student`
2. Measure response time

**Expected Result:**
- Response time < 500ms (P95)
- All students returned or paginated appropriately

---

### TC-STU-019: Create Student - Role Authorization

**Priority:** Critical  
**Type:** Negative  
**Category:** Authorization

**Prerequisites:**
- Logged in as Student (not Admin)

**Steps:**
1. Attempt to create new student as Student user

**Expected Result:**
- Status Code: `403 Forbidden`
- Error message: "Insufficient permissions"

---

### TC-STU-020: Bulk Student Creation

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Test Data:**
```json
{
  "students": [
    { "firstName": "Student1", "lastName": "Test", "email": "s1@sms.edu" },
    { "firstName": "Student2", "lastName": "Test", "email": "s2@sms.edu" },
    { "firstName": "Student3", "lastName": "Test", "email": "s3@sms.edu" }
  ]
}
```

**Steps:**
1. Send POST request to `/api/Student/bulk` (if endpoint exists)

**Expected Result:**
- Status Code: `201 Created`
- All students created successfully
- Response contains array of created students

---

## Test Execution Checklist

- [ ] All CRUD operations tested
- [ ] Validation rules enforced
- [ ] Authorization checks pass
- [ ] Pagination works correctly
- [ ] Search/filter functionality validated
- [ ] Performance benchmarks met
- [ ] Test coverage > 80%

---

**Total Test Cases:** 20  
**Critical:** 2  
**High:** 10  
**Medium:** 7  
**Low:** 1  
**Coverage:** CRUD, Validation, Authorization, Pagination, Search
