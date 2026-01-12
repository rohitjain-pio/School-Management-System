# Authentication API Documentation

**Base URL:** `/api/Auth`  
**Module:** Authentication & Authorization  
**Status:** ✅ Fully Implemented (Backend + Frontend)

---

## Overview
The Authentication API provides comprehensive user authentication, authorization, password management, and session control functionality.

## Frontend Integration
- **Service File:** [Frontend/src/services/authService.ts](../../Frontend/src/services/authService.ts)
- **Status:** ✅ **WORKING** - Complete integration with all auth endpoints
- **Used By:** Login page, Register page, Profile pages, Password reset flows

---

## Endpoints

### 1. POST `/api/Auth/register`
**Register a new user**

#### Access
- ✅ **Public** (No authentication required)

#### Request Body
```json
{
  "userName": "string",
  "email": "string",
  "password": "string",      // Min 8 chars, uppercase, lowercase, digit, special char
  "role": "string",          // Options: "Admin", "SchoolAdmin", "Teacher", "Student"
  "schoolId": "guid | null"  // Required for non-Admin roles
}
```

#### Success Response (200 OK)
```json
{
  "isSuccess": true,
  "message": "Registration successful!"
}
```

#### Error Response (400 Bad Request)
```json
{
  "isSuccess": false,
  "errorMessage": "Password must be at least 8 characters..."
}
```

#### Password Requirements
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character
- At least 4 unique characters

#### Security Features
- Automatic password hashing using ASP.NET Identity
- Email uniqueness validation
- Role-based user creation
- Audit logging for registration attempts

---

### 2. POST `/api/Auth/login`
**Authenticate user and create session**

#### Access
- ✅ **Public** (No authentication required)

#### Request Body
```json
{
  "userName": "string",
  "password": "string"
}
```

#### Success Response (200 OK)
```json
{
  "message": "Login successful",
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "schoolId": "guid | null",
    "roles": ["string"]  // Array of role names
  }
}
```

#### HTTP-Only Cookies Set
```
auth_token (JWT):
  - HttpOnly: true
  - Secure: true  
  - SameSite: Lax
  - Expires: 3 hours

refresh_token:
  - HttpOnly: true
  - Secure: true
  - SameSite: Strict
  - Path: /api/Auth/refresh
  - Expires: 7 days
```

#### Error Responses
**401 Unauthorized - Invalid Credentials**
```json
{
  "message": "Invalid credentials. 5 attempts remaining."
}
```

**423 Locked - Account Locked**
```json
{
  "message": "Account locked due to too many failed attempts. Try again in 30 minutes."
}
```

#### Security Features
- Failed login attempt tracking
- Account lockout after 5 failed attempts (30-minute lockout)
- IP address logging
- Last login timestamp
- JWT + Refresh Token dual-token system
- Audit logging

---

### 3. GET `/api/Auth/me`
**Get current authenticated user information**

#### Access
- 🔒 **Requires Authentication**

#### Success Response (200 OK)
```json
{
  "id": "guid",
  "username": "string",
  "email": "string",
  "schoolId": "guid | null",
  "roles": ["string"]
}
```

#### Error Response (404 Not Found)
```json
{
  "message": "User not found"
}
```

#### Frontend Usage
- Used to verify authentication state
- Retrieve current user profile
- Check user roles and permissions

---

### 4. POST `/api/Auth/logout`
**Terminate user session**

#### Access
- 🔒 **Requires Authentication**

#### Success Response (200 OK)
```json
{
  "message": "Logout successful"
}
```

#### Actions Performed
1. Revokes all user refresh tokens
2. Blacklists current JWT token (until expiration)
3. Deletes auth_token cookie
4. Deletes refresh_token cookie
5. Signs out user from ASP.NET Identity
6. Logs audit event

---

### 5. POST `/api/Auth/refresh`
**Refresh expired access token**

#### Access
- ✅ **Public** (Uses refresh token from cookie)

#### Request
- Reads `refresh_token` from HTTP-Only cookie

#### Success Response (200 OK)
```json
{
  "message": "Token refreshed successfully"
}
```

#### New Cookies Set
- New `auth_token` with 3-hour expiration
- New `refresh_token` (rotated for security)

#### Error Response (401 Unauthorized)
```json
{
  "message": "Invalid or expired refresh token"
}
```

#### Security Features
- **Token Rotation**: Old refresh token is invalidated
- IP address validation
- Audit logging
- Automatic cleanup of expired tokens

---

### 6. POST `/api/Auth/request-password-reset`
**Request password reset token**

#### Access
- ✅ **Public** (No authentication required)

#### Request Body
```json
{
  "email": "string"
}
```

#### Success Response (200 OK)
```json
{
  "message": "If the email exists, a password reset link has been sent.",
  "resetToken": "string"  // ⚠️ Only in development mode
}
```

#### Security Features
- **Generic response** (doesn't reveal if email exists)
- One-time use reset tokens
- Time-limited tokens (typically 1 hour)
- IP address logging
- Audit trail

---

### 7. POST `/api/Auth/reset-password`
**Reset password using reset token**

#### Access
- ✅ **Public** (Uses reset token)

#### Request Body
```json
{
  "email": "string",
  "token": "string",      // From request-password-reset
  "newPassword": "string" // Must meet password requirements
}
```

#### Success Response (200 OK)
```json
{
  "message": "Password reset successfully. Please login with your new password."
}
```

#### Error Response (400 Bad Request)
```json
{
  "message": "Invalid or expired reset token"
}
```

#### Actions Performed
1. Validates reset token
2. Changes password
3. Marks token as used
4. Revokes all existing refresh tokens
5. Logs audit event

---

### 8. POST `/api/Auth/change-password`
**Change password for authenticated user**

#### Access
- 🔒 **Requires Authentication**

#### Request Body
```json
{
  "currentPassword": "string",
  "newPassword": "string"  // Must meet password requirements
}
```

#### Success Response (200 OK)
```json
{
  "message": "Password changed successfully. Please login again."
}
```

#### Error Response (400 Bad Request)
```json
{
  "message": "Current password is incorrect"
}
```

#### Actions Performed
1. Validates current password
2. Changes to new password
3. Revokes all refresh tokens (force re-login)
4. Blacklists current JWT
5. Deletes authentication cookies
6. Logs audit event

---

## Frontend Implementation Status

### ✅ Implemented & Working
- **Login Flow** - Complete with error handling
- **Registration Flow** - Multi-step with role selection
- **Logout** - Proper cleanup and redirect
- **Get Current User** - Used in protected routes
- **Password Reset Request** - Email-based flow
- **Reset Password** - Token validation and password update
- **Change Password** - Authenticated password change

### 📁 Frontend Files
```
Frontend/src/
├── services/
│   └── authService.ts          # All auth API calls
├── context/
│   └── AuthContext.tsx          # Auth state management (if exists)
├── pages/
│   ├── Login.tsx
│   └── Register.tsx
└── hooks/
    └── useAuth.tsx              # Auth custom hooks (if exists)
```

---

## Security Features

### ✅ Implemented
1. **JWT Authentication** - Bearer token in HTTP-Only cookies
2. **Refresh Token Rotation** - Enhanced security
3. **Token Blacklisting** - Prevent reuse of revoked tokens
4. **Account Lockout** - 5 failed attempts = 30-minute lockout
5. **Password Requirements** - Strong password enforcement
6. **Audit Logging** - All auth events logged
7. **IP Address Tracking** - Login source tracking
8. **CORS Protection** - Restricted origins
9. **HTTPS Required** - Secure cookie flag
10. **SameSite Cookies** - CSRF protection

### 🔒 Cookie Security
```
auth_token:
  HttpOnly: ✅  // Prevents XSS attacks
  Secure: ✅    // HTTPS only
  SameSite: Lax // CSRF protection

refresh_token:
  HttpOnly: ✅  // Prevents XSS attacks
  Secure: ✅    // HTTPS only
  SameSite: Strict  // Maximum CSRF protection
  Path: /api/Auth/refresh  // Limited scope
```

---

## Testing Recommendations

### Manual Testing Checklist
- [ ] Register new user with each role (Admin, SchoolAdmin, Teacher, Student)
- [ ] Login with valid credentials
- [ ] Login with invalid credentials (check lockout)
- [ ] Access /me endpoint while authenticated
- [ ] Logout and verify token is invalidated
- [ ] Refresh token before expiration
- [ ] Request password reset
- [ ] Reset password with valid token
- [ ] Change password while authenticated
- [ ] Verify lockout after 5 failed login attempts

### Automated Testing
- Unit tests for password validation
- Integration tests for auth flows
- Security tests for token validation

---

## Known Issues & Considerations

### ⚠️ Development vs Production
- **Reset Token Exposure**: In development, reset token is returned in response (should be email-only in production)
- **CORS Configuration**: Development allows multiple local origins; production should restrict to actual domain

### 🔄 Potential Improvements
1. **Email Verification**: Currently disabled (`RequireConfirmedEmail = false`)
2. **2FA Support**: Not implemented
3. **OAuth Integration**: No social login options
4. **Rate Limiting**: General rate limiting exists, but specific auth endpoint limits could be stricter
5. **Password History**: No prevention of password reuse

---

## Error Codes Summary

| Status Code | Scenario |
|-------------|----------|
| 200 | Success |
| 400 | Bad request (validation errors, invalid data) |
| 401 | Unauthorized (invalid credentials, expired token) |
| 404 | User not found |
| 423 | Account locked (too many failed attempts) |

---

## Authorization Policies (Used by other APIs)

The auth system establishes these policies used throughout the application:

| Policy | Required Roles | Usage |
|--------|---------------|-------|
| `AdminOnly` | Admin | School management |
| `SchoolAdminOnly` | SchoolAdmin | School-level administration |
| `AdminOrSchoolAdmin` | Admin, SchoolAdmin | Most management functions |
| `TeacherOrAbove` | Teacher, SchoolAdmin, Admin | Teaching features |
| `StudentOrAbove` | Student, Teacher, SchoolAdmin, Admin | General features |
| `SameSchool` | Any authenticated + same school | School-scoped data |
| `ResourceOwner` | Any authenticated + owns resource | Personal data |

---

**Last Updated:** January 12, 2026  
**API Version:** v1  
**Backend Status:** ✅ Fully Implemented  
**Frontend Status:** ✅ Fully Integrated
