# API Contract: Authentication
**Version:** 1.0  
**Base URL:** `/api/auth`  
**Authentication:** None (public endpoints)

---

## 📋 Overview

Authentication API for user login, registration, and token management with multi-tenant support.

**Endpoints:** 5 endpoints  
**Token Type:** JWT Bearer  
**Token Expiration:** 60 minutes  
**Refresh Token:** 7 days

---

## 🔐 Security

### JWT Token Structure

```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@school.com",
  "SchoolId": "11111111-1111-1111-1111-111111111111",
  "role": "Admin",
  "exp": 1705324800,
  "iss": "SMS-API",
  "aud": "SMS-Client"
}
```

**Required Claims:**
- `sub` (NameIdentifier): User ID
- `email`: User email
- `SchoolId`: Tenant discriminator (CRITICAL)
- `role`: User role (Admin, Teacher, Student)
- `exp`: Expiration timestamp

---

## 📊 Data Models

### LoginDto (Request)

```typescript
{
  "email": "admin@school.com",
  "password": "Test123!"
}
```

**Validation:**
- `email`: Required, valid email format, max 200 chars
- `password`: Required, min 6 chars, max 100 chars

---

### RegisterDto (Request)

```typescript
{
  "email": "admin@school.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "firstName": "John",
  "lastName": "Doe",
  "schoolName": "Springfield High School",
  "phoneNumber": "+1234567890"
}
```

**Validation:**
- `email`: Required, valid email, max 200 chars, must be unique
- `password`: Required, min 8 chars, must contain uppercase, lowercase, digit, special char
- `confirmPassword`: Required, must match password
- `firstName`: Required, 1-100 chars, letters and spaces only
- `lastName`: Required, 1-100 chars, letters and spaces only
- `schoolName`: Required, 1-200 chars (creates new school)
- `phoneNumber`: Optional, E.164 format

---

### AuthResponse

```typescript
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresIn": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@school.com",
    "firstName": "John",
    "lastName": "Doe",
    "schoolId": "11111111-1111-1111-1111-111111111111",
    "schoolName": "Springfield High School",
    "role": "Admin"
  }
}
```

---

### RefreshTokenDto (Request)

```typescript
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```

---

### ChangePasswordDto (Request)

```typescript
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmNewPassword": "NewPass456!"
}
```

---

## 🔗 Endpoints

### 1. Register (Create New School)

**Request:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "admin@school.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "firstName": "John",
  "lastName": "Doe",
  "schoolName": "Springfield High School",
  "phoneNumber": "+1234567890"
}
```

**Response: 201 Created**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresIn": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@school.com",
    "firstName": "John",
    "lastName": "Doe",
    "schoolId": "11111111-1111-1111-1111-111111111111",
    "schoolName": "Springfield High School",
    "role": "Admin"
  }
}
```

**Errors:**
- `400 Bad Request`: Validation errors
  ```json
  {
    "errors": {
      "Email": ["Email is already in use"],
      "Password": ["Password must contain at least one uppercase letter"]
    }
  }
  ```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

**What Happens:**
1. Creates new School record
2. Creates Admin user associated with school
3. Generates JWT token with SchoolId claim
4. Returns token + user info

**Performance:**
- Expected Response Time: < 300ms (includes password hashing)

**Rate Limiting:**
- 5 registrations per IP per hour (prevent abuse)

---

### 2. Login

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@school.com",
  "password": "Test123!"
}
```

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresIn": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@school.com",
    "firstName": "John",
    "lastName": "Doe",
    "schoolId": "11111111-1111-1111-1111-111111111111",
    "schoolName": "Springfield High School",
    "role": "Admin"
  }
}
```

**Errors:**
- `401 Unauthorized`: Invalid credentials
  ```json
  {
    "message": "Invalid email or password"
  }
  ```
- `403 Forbidden`: Account locked (too many failed attempts)
  ```json
  {
    "message": "Account locked. Try again in 15 minutes."
  }
  ```

**Security Features:**
- Account lockout after 5 failed attempts (15-minute lockout)
- Rate limiting: 10 attempts per IP per minute
- Password not logged (ever)
- Generic error message (don't reveal if email exists)

**Performance:**
- Expected Response Time: < 200ms

---

### 3. Refresh Token

**Request:**
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "a1b2c3d4e5f6..."
}
```

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "g7h8i9j0k1l2...",
  "expiresIn": 3600
}
```

**Errors:**
- `401 Unauthorized`: Invalid or expired refresh token
  ```json
  {
    "message": "Invalid refresh token"
  }
  ```

**Notes:**
- Generates new JWT token + new refresh token
- Old refresh token is invalidated
- Refresh tokens expire after 7 days of inactivity

**Performance:**
- Expected Response Time: < 150ms

---

### 4. Change Password

**Request:**
```http
POST /api/auth/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmNewPassword": "NewPass456!"
}
```

**Response: 200 OK**
```json
{
  "message": "Password changed successfully"
}
```

**Errors:**
- `401 Unauthorized`: Missing or invalid token
- `400 Bad Request`: Validation errors
  ```json
  {
    "errors": {
      "CurrentPassword": ["Current password is incorrect"],
      "NewPassword": ["Password must contain at least one uppercase letter"]
    }
  }
  ```

**Security:**
- Requires authentication (valid JWT token)
- Must provide correct current password
- New password must meet requirements
- All active tokens invalidated (forces re-login)

**Performance:**
- Expected Response Time: < 300ms

---

### 5. Logout

**Request:**
```http
POST /api/auth/logout
Authorization: Bearer {token}
```

**Response: 204 No Content**

**Errors:**
- `401 Unauthorized`: Missing or invalid token

**What Happens:**
1. Invalidates current JWT token (adds to blacklist)
2. Invalidates all refresh tokens for user
3. Client should clear token from storage

**Performance:**
- Expected Response Time: < 100ms

---

## 🔒 Security Best Practices

### Token Storage (Frontend)

```typescript
// ✅ BEST - httpOnly cookie (set by backend)
// Cannot be accessed by JavaScript (XSS protection)

// ✅ ACCEPTABLE - sessionStorage (cleared on tab close)
sessionStorage.setItem('token', authResponse.token);

// ⚠️ CAUTION - localStorage (persists across sessions)
localStorage.setItem('token', authResponse.token);

// ❌ NEVER - Plain variable (lost on refresh)
let token = authResponse.token;
```

### API Interceptor (Axios)

```typescript
// Auto-attach token to all requests
axios.interceptors.request.use(config => {
  const token = sessionStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 (token expired)
axios.interceptors.response.use(
  response => response,
  async error => {
    if (error.response?.status === 401) {
      // Try refresh token
      const refreshToken = sessionStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const response = await authService.refresh(refreshToken);
          sessionStorage.setItem('token', response.token);
          sessionStorage.setItem('refreshToken', response.refreshToken);
          
          // Retry original request
          error.config.headers.Authorization = `Bearer ${response.token}`;
          return axios.request(error.config);
        } catch {
          // Refresh failed, redirect to login
          authService.logout();
          window.location.href = '/login';
        }
      } else {
        authService.logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
```

---

## ⚡ Performance

### Response Time Targets

| Endpoint | P50 | P95 | P99 |
|----------|-----|-----|-----|
| POST /auth/register | 250ms | 300ms | 400ms |
| POST /auth/login | 150ms | 200ms | 280ms |
| POST /auth/refresh | 100ms | 150ms | 200ms |
| POST /auth/change-password | 250ms | 300ms | 400ms |
| POST /auth/logout | 50ms | 100ms | 150ms |

### Rate Limiting

| Endpoint | Limit | Window |
|----------|-------|--------|
| /auth/register | 5 requests | Per IP per hour |
| /auth/login | 10 requests | Per IP per minute |
| /auth/refresh | 30 requests | Per user per hour |
| /auth/change-password | 5 requests | Per user per hour |

429 Too Many Requests returned if exceeded.

---

## 🧪 Example Usage

### TypeScript (Frontend)

```typescript
import { api } from '@/lib/api-client';

// Register
const registerResponse = await api.post<AuthResponse>('/auth/register', {
  email: 'admin@school.com',
  password: 'Test123!',
  confirmPassword: 'Test123!',
  firstName: 'John',
  lastName: 'Doe',
  schoolName: 'Springfield High School'
});

// Store tokens
sessionStorage.setItem('token', registerResponse.data.token);
sessionStorage.setItem('refreshToken', registerResponse.data.refreshToken);

// Login
const loginResponse = await api.post<AuthResponse>('/auth/login', {
  email: 'admin@school.com',
  password: 'Test123!'
});

// Refresh token
const refreshResponse = await api.post<AuthResponse>('/auth/refresh', {
  refreshToken: sessionStorage.getItem('refreshToken')
});

// Change password
await api.post('/auth/change-password', {
  currentPassword: 'OldPass123!',
  newPassword: 'NewPass456!',
  confirmNewPassword: 'NewPass456!'
});

// Logout
await api.post('/auth/logout');
sessionStorage.clear();
```

### C# (Backend)

```csharp
// JWT Token Generation
private string GenerateJwtToken(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("SchoolId", user.SchoolId.ToString()), // ✅ CRITICAL
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// Password Hashing
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, 12); // Cost factor 12
}

public bool VerifyPassword(string password, string passwordHash)
{
    return BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
```

---

## 🐛 Common Error Responses

### 400 Bad Request - Validation Errors
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "Email": ["Email is already in use"],
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter"
    ]
  }
}
```

### 401 Unauthorized - Invalid Credentials
```json
{
  "statusCode": 401,
  "message": "Invalid email or password"
}
```

### 403 Forbidden - Account Locked
```json
{
  "statusCode": 403,
  "message": "Account locked due to too many failed login attempts. Try again in 15 minutes."
}
```

### 429 Too Many Requests
```json
{
  "statusCode": 429,
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

---

## 📝 Change Log

### Version 1.0 (2024-01-15)
- Initial release
- JWT authentication
- Refresh token support
- Account lockout protection
- Rate limiting

---

**Related Files:**
- `.copilot/api-contracts/student-api-contract.md`
- `.copilot/api-contracts/teacher-api-contract.md`
- `.copilot/workflows/security-review-checklist.md`
