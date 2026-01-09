# Authentication API Test Cases

**Module:** Authentication  
**Controller:** AuthController  
**Base Path:** `/api/Auth`

---

## Test Cases

### TC-AUTH-001: User Registration - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- API server is running
- Test database is clean

**Test Data:**
```json
{
  "userName": "newuser123",
  "password": "SecurePass123!",
  "email": "newuser@sms.edu",
  "role": "Student"
}
```

**Steps:**
1. Send POST request to `/api/Auth/register`
2. Include registration data in request body
3. Validate response

**Expected Result:**
- Status Code: `200 OK`
- Response Body:
  ```json
  {
    "isSuccess": true,
    "data": {
      "id": "<GUID>",
      "userName": "newuser123",
      "email": "newuser@sms.edu",
      "role": "Student"
    },
    "errorMessage": null
  }
  ```
- User is created in database
- Password is hashed (not stored as plain text)

**Postman Test:**
```javascript
pm.test("Status code is 200", () => {
    pm.response.to.have.status(200);
});

pm.test("Response contains user data", () => {
    const jsonData = pm.response.json();
    pm.expect(jsonData.isSuccess).to.be.true;
    pm.expect(jsonData.data).to.have.property('id');
    pm.expect(jsonData.data.userName).to.equal('newuser123');
});
```

---

### TC-AUTH-002: User Registration - Duplicate Username

**Priority:** High  
**Type:** Negative  
**Category:** Integration

**Prerequisites:**
- User with username "existinguser" already exists

**Test Data:**
```json
{
  "userName": "existinguser",
  "password": "SecurePass123!",
  "email": "duplicate@sms.edu",
  "role": "Student"
}
```

**Steps:**
1. Send POST request to `/api/Auth/register`
2. Use username that already exists

**Expected Result:**
- Status Code: `400 Bad Request`
- Response Body:
  ```json
  {
    "isSuccess": false,
    "data": null,
    "errorMessage": "Username already exists"
  }
  ```

---

### TC-AUTH-003: User Registration - Invalid Password

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Test Data:**
```json
{
  "userName": "testuser",
  "password": "weak",
  "email": "test@sms.edu",
  "role": "Student"
}
```

**Expected Result:**
- Status Code: `400 Bad Request`
- Error message indicates password requirements not met

---

### TC-AUTH-004: User Login - Success

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- User exists with credentials: `testuser` / `Test123!`

**Test Data:**
```json
{
  "userName": "testuser",
  "password": "Test123!"
}
```

**Steps:**
1. Send POST request to `/api/Auth/login`
2. Include valid credentials

**Expected Result:**
- Status Code: `200 OK`
- Response contains JWT token
- Cookie `auth_token` is set with:
  - HttpOnly flag
  - Secure flag
  - SameSite=Lax
  - Expiration: 3 hours
- Response Body:
  ```json
  {
    "isSuccess": true,
    "data": {
      "token": "<JWT_TOKEN>",
      "userId": "<GUID>",
      "userName": "testuser",
      "role": "Student",
      "expiresAt": "<ISO_DATETIME>"
    },
    "errorMessage": null
  }
  ```

**Postman Test:**
```javascript
pm.test("Login successful", () => {
    pm.response.to.have.status(200);
    const jsonData = pm.response.json();
    pm.expect(jsonData.data.token).to.be.a('string');
    pm.environment.set("authToken", jsonData.data.token);
});

pm.test("Cookie is set", () => {
    pm.expect(pm.cookies.has('auth_token')).to.be.true;
});
```

---

### TC-AUTH-005: User Login - Invalid Credentials

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Test Data:**
```json
{
  "userName": "testuser",
  "password": "WrongPassword123!"
}
```

**Expected Result:**
- Status Code: `401 Unauthorized`
- No token returned
- No cookie set
- Error message: "Invalid credentials"

---

### TC-AUTH-006: User Login - Non-existent User

**Priority:** Medium  
**Type:** Negative  
**Category:** Security

**Test Data:**
```json
{
  "userName": "nonexistentuser",
  "password": "Test123!"
}
```

**Expected Result:**
- Status Code: `401 Unauthorized`
- Error message should NOT reveal whether user exists (security best practice)

---

### TC-AUTH-007: User Logout - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- User is logged in with valid token

**Steps:**
1. Send POST request to `/api/Auth/logout`
2. Include auth token in cookie or Authorization header

**Expected Result:**
- Status Code: `200 OK`
- Cookie `auth_token` is cleared (expires immediately)
- Response:
  ```json
  {
    "isSuccess": true,
    "data": "Logged out successfully",
    "errorMessage": null
  }
  ```

---

### TC-AUTH-008: JWT Token Validation - Valid Token

**Priority:** Critical  
**Type:** Positive  
**Category:** Security

**Prerequisites:**
- Valid JWT token obtained from login

**Steps:**
1. Send GET request to protected endpoint (e.g., `/api/Student`)
2. Include token in Authorization header: `Bearer <token>`

**Expected Result:**
- Status Code: `200 OK`
- Request is authorized
- User data is accessible

**Postman Pre-request Script:**
```javascript
pm.request.headers.add({
    key: 'Authorization',
    value: 'Bearer ' + pm.environment.get('authToken')
});
```

---

### TC-AUTH-009: JWT Token Validation - Expired Token

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Prerequisites:**
- JWT token that has expired (> 3 hours old)

**Steps:**
1. Send request with expired token

**Expected Result:**
- Status Code: `401 Unauthorized`
- Error message: "Token expired"

---

### TC-AUTH-010: JWT Token Validation - Malformed Token

**Priority:** Medium  
**Type:** Negative  
**Category:** Security

**Test Data:**
- Token: `invalid.token.format`

**Expected Result:**
- Status Code: `401 Unauthorized`
- Error message indicates invalid token

---

### TC-AUTH-011: Role-Based Authorization - Admin Access

**Priority:** Critical  
**Type:** Positive  
**Category:** Authorization

**Prerequisites:**
- Logged in as Admin user

**Steps:**
1. Send request to admin-only endpoint (e.g., `POST /api/School`)
2. Include admin JWT token

**Expected Result:**
- Status Code: `200 OK` or `201 Created`
- Request is successful

---

### TC-AUTH-012: Role-Based Authorization - Student Access Denied

**Priority:** High  
**Type:** Negative  
**Category:** Authorization

**Prerequisites:**
- Logged in as Student user

**Steps:**
1. Send request to admin-only endpoint (e.g., `POST /api/School`)
2. Include student JWT token

**Expected Result:**
- Status Code: `403 Forbidden`
- Error message: "Insufficient permissions"

---

### TC-AUTH-013: Password Security - Hashing Verification

**Priority:** Critical  
**Type:** Security  
**Category:** Unit Test

**Steps:**
1. Register new user
2. Query database directly
3. Verify password is hashed (not plain text)

**Expected Result:**
- Password field in database contains hashed value
- Hashed password does NOT match original password
- Hash algorithm is secure (bcrypt, PBKDF2, or Argon2)

---

### TC-AUTH-014: Concurrent Login Sessions

**Priority:** Medium  
**Type:** Functional  
**Category:** Integration

**Steps:**
1. Login from Device/Browser A
2. Login from Device/Browser B with same credentials
3. Verify both sessions are valid

**Expected Result:**
- Both logins successful
- Both tokens are valid
- Both sessions can make authorized requests

**Note:** If single-session enforcement is required, second login should invalidate first token.

---

### TC-AUTH-015: Registration with All Roles

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Test Data:**
Test registration for each role:
- Student
- Teacher
- Admin
- Principal
- SchoolIncharge
- SuperAdmin

**Expected Result:**
- All roles can be registered successfully
- User object contains correct role
- JWT token contains correct role claim

---

## Test Execution Checklist

- [ ] All test cases pass in local environment
- [ ] All test cases pass in CI/CD pipeline
- [ ] Security tests validated by security team
- [ ] Performance benchmarks met (login < 200ms)
- [ ] Test coverage > 80% for AuthController
- [ ] Edge cases documented and tested

---

**Total Test Cases:** 15  
**Critical:** 4  
**High:** 7  
**Medium:** 4  
**Coverage:** Authentication, Authorization, Security, Validation
