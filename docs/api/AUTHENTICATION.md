# Authentication & Authorization Guide

## Overview

The School Management System API uses **JWT (JSON Web Tokens)** for authentication and **role-based authorization** for access control. This guide covers authentication strategies, token management, SignalR hub authentication, and security best practices.

## Table of Contents

- [Authentication Overview](#authentication-overview)
- [User Registration](#user-registration)
- [Login & Token Acquisition](#login--token-acquisition)
- [Token Management](#token-management)
- [Authorization & Roles](#authorization--roles)
- [SignalR Hub Authentication](#signalr-hub-authentication)
- [Room Access Tokens](#room-access-tokens)
- [Security Best Practices](#security-best-practices)
- [Common Authentication Scenarios](#common-authentication-scenarios)

---

## Authentication Overview

### Authentication Flow

```
1. User Registration (POST /api/Auth/register)
   ↓
2. User Login (POST /api/Auth/login)
   ↓
3. JWT Token Generated & Set as HTTP-only Cookie
   ↓
4. Subsequent API Requests Include Token (Cookie or Header)
   ↓
5. Server Validates Token & Authorizes Request
   ↓
6. Token Expires → User Re-authenticates or Refreshes
```

### Token Storage Methods

The API supports two token storage strategies:

1. **HTTP-only Cookies** (Default for web clients)
   - Automatically sent with requests
   - Secure against XSS attacks
   - Best for browser-based applications

2. **Authorization Header** (For mobile/desktop apps)
   - Manual token management
   - Format: `Authorization: Bearer <token>`
   - Flexible for non-browser clients

---

## User Registration

### Endpoint

```
POST /api/Auth/register
```

### Request Body

```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "role": "string",
  "schoolId": "uuid | null"
}
```

### Password Requirements

- Minimum length: **6 characters**
- Must contain: **at least one digit**
- Must contain: **at least one lowercase letter**
- Must contain: **at least one uppercase letter**
- Non-alphanumeric characters: **optional**

### Available Roles

| Role | Description | Permissions |
|------|-------------|-------------|
| `Student` | Student users | View own data, join classes, participate in chat/video |
| `Teacher` | Teacher users | Manage classes, mark attendance, create chat rooms |
| `Admin` | School administrators | Full CRUD on students, teachers, classes |
| `SuperAdmin` | System administrators | Full system access, manage multiple schools |
| `Principal` | School principals | School-wide management, reports |
| `SchoolIncharge` | School managers | Similar to Admin |

### Example: Register a Student

```bash
curl -X POST https://localhost:7266/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "alice.johnson",
    "email": "alice.johnson@student.school.edu",
    "password": "Student123",
    "role": "Student",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  }'
```

**Success Response (200 OK):**

```json
{
  "isSuccess": true,
  "message": "Registration successful!"
}
```

**Error Response (400 Bad Request):**

```json
{
  "isSuccess": false,
  "errorMessage": "Password must be at least 6 characters; Password must contain at least one uppercase letter"
}
```

---

## Login & Token Acquisition

### Endpoint

```
POST /api/Auth/login
```

### Request Body

```json
{
  "userName": "string",
  "password": "string"
}
```

### Cookie-Based Authentication (Default)

When you login, the server sets an HTTP-only cookie named `auth_token`:

```bash
curl -X POST https://localhost:7266/api/Auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "userName": "alice.johnson",
    "password": "Student123"
  }'
```

**Response:**

```json
{
  "message": "Login successful"
}
```

**Cookie Details:**
- Name: `auth_token`
- HttpOnly: `true` (prevents JavaScript access)
- Secure: `true` (HTTPS only)
- SameSite: `Lax` (CSRF protection)
- Expires: **3 hours** from login
- Path: `/`

### Header-Based Authentication (Alternative)

For non-browser clients, you can modify the backend to return the token directly:

**Modified Response Example:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
  "expiration": "2024-01-09T13:00:00Z"
}
```

**Usage in Subsequent Requests:**

```bash
curl -X GET https://localhost:7266/api/Auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Token Management

### Token Structure

JWT tokens contain the following claims:

```json
{
  "sub": "user-id",
  "nameid": "user-id",
  "unique_name": "username",
  "jti": "unique-token-id",
  "SchoolId": "school-id",
  "role": ["Student"],
  "nbf": 1704801600,
  "exp": 1704812400,
  "iat": 1704801600,
  "iss": "YourIssuer",
  "aud": "YourAudience"
}
```

**Key Claims:**
- `nameid` / `sub`: User ID (GUID)
- `unique_name`: Username
- `SchoolId`: Associated school ID
- `role`: Array of user roles
- `exp`: Expiration timestamp (3 hours from issue)

### Token Expiration

- **Default Expiration**: 3 hours
- **Configurable**: Set in `appsettings.json` or code
- **Behavior on Expiration**: 401 Unauthorized response

### Token Refresh Strategy

Currently, the API does not implement refresh tokens. When a token expires:

1. User must re-authenticate via `/api/Auth/login`
2. New token is issued

**Best Practice**: Implement automatic re-authentication before expiration in your client.

**Future Enhancement**: Add refresh token endpoint:

```
POST /api/Auth/refresh
{
  "refreshToken": "string"
}
```

### Get Current User

Verify authentication and get user details:

```bash
curl -X GET https://localhost:7266/api/Auth/me \
  -b cookies.txt
```

**Response:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "alice.johnson",
  "email": "alice.johnson@student.school.edu",
  "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "roles": ["Student"]
}
```

### Logout

```bash
curl -X POST https://localhost:7266/api/Auth/logout \
  -b cookies.txt
```

**Response:**

```json
{
  "message": "Logout successful"
}
```

The `auth_token` cookie is deleted from the client.

---

## Authorization & Roles

### Role-Based Access Control

The API uses ASP.NET Core's `[Authorize]` attribute with role checks:

```csharp
[Authorize(Roles = "Admin,SuperAdmin")]
public async Task<IActionResult> DeleteStudent(Guid id)
```

### Permission Matrix

| Endpoint | Student | Teacher | Admin | SuperAdmin/Principal |
|----------|---------|---------|-------|---------------------|
| **Auth Endpoints** |
| POST /api/Auth/register | ✅ | ✅ | ✅ | ✅ |
| POST /api/Auth/login | ✅ | ✅ | ✅ | ✅ |
| GET /api/Auth/me | ✅ | ✅ | ✅ | ✅ |
| POST /api/Auth/logout | ✅ | ✅ | ✅ | ✅ |
| **Student Endpoints** |
| GET /api/Student | ✅ (own) | ✅ | ✅ | ✅ |
| POST /api/Student | ❌ | ❌ | ✅ | ✅ |
| PUT /api/Student/{id} | ✅ (own) | ❌ | ✅ | ✅ |
| DELETE /api/Student/{id} | ❌ | ❌ | ✅ | ✅ |
| **Teacher Endpoints** |
| GET /api/Teacher | ✅ | ✅ | ✅ | ✅ |
| POST /api/Teacher | ❌ | ❌ | ✅ | ✅ |
| PUT /api/Teacher/{id} | ❌ | ✅ (own) | ✅ | ✅ |
| DELETE /api/Teacher/{id} | ❌ | ❌ | ✅ | ✅ |
| **Attendance** |
| GET /api/Attendance | ✅ (own) | ✅ | ✅ | ✅ |
| POST /api/Attendance | ❌ | ✅ | ✅ | ✅ |
| **Chat Rooms** |
| GET /api/ChatRooms | ✅ | ✅ | ✅ | ✅ |
| POST /api/ChatRooms | ❌ | ✅ | ✅ | ✅ |
| POST /api/ChatRooms/join | ✅ | ✅ | ✅ | ✅ |
| DELETE /api/ChatRooms/{id} | ❌ | ✅ (owner) | ✅ | ✅ |

### Checking Permissions in Client Code

**JavaScript Example:**

```javascript
const response = await fetch('/api/Auth/me', {
  credentials: 'include' // Include cookies
});

const user = await response.json();
const isAdmin = user.roles.includes('Admin') || 
                user.roles.includes('SuperAdmin');

if (isAdmin) {
  // Show admin-only features
}
```

---

## SignalR Hub Authentication

SignalR hubs (`ChatHub`, `VideoCallHub`) require JWT authentication.

### Authentication Methods

#### 1. Cookie-Based (Recommended for Web)

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub", {
    // Cookies are automatically sent
  })
  .build();

await connection.start();
```

#### 2. Query String Token

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub?access_token=" + jwtToken)
  .build();

await connection.start();
```

#### 3. Access Token Factory (Best Practice)

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub", {
    accessTokenFactory: () => {
      return getTokenFromStorage(); // Your token retrieval logic
    }
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Server-Side Token Validation

The backend validates tokens in the `JwtBearerEvents.OnMessageReceived` event:

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        
        // Allow access token in query string for SignalR hubs
        if (!string.IsNullOrEmpty(accessToken) && 
            (path.StartsWithSegments("/chatHub") || 
             path.StartsWithSegments("/videoCallHub")))
        {
            context.Token = accessToken;
        }
        else if (context.Request.Cookies.ContainsKey("auth_token"))
        {
            context.Token = context.Request.Cookies["auth_token"];
        }
        
        return Task.CompletedTask;
    }
};
```

### Handling Authentication Failures

```javascript
connection.onclose(async (error) => {
  if (error && error.message.includes('Unauthorized')) {
    console.error('Authentication failed. Please login again.');
    // Redirect to login page
  }
});
```

---

## Room Access Tokens

In addition to user authentication, chat and video rooms use **room access tokens** for fine-grained access control.

### Purpose

- Verify user has permission to join a specific room
- Prevent unauthorized room access even with valid JWT
- Short-lived tokens tied to specific rooms

### Obtaining Room Access Token

When you join a room via the REST API:

```bash
curl -X POST https://localhost:7266/api/ChatRooms/join \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "roomId": "room-id-here",
    "password": "RoomPassword123"
  }'
```

**Response includes:**

```json
{
  "ok": true,
  "message": "Successfully joined room",
  "roomAccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roomDetails": { ... }
}
```

### Using Room Access Token

When joining SignalR hub:

```javascript
// Join chat room
await connection.invoke("JoinRoom", roomId, roomAccessToken);

// Join video room
await videoConnection.invoke("JoinVideoRoom", roomId, roomAccessToken);
```

### Token Validation

The server validates room access tokens to ensure:

1. Token is valid and not expired
2. Token's `roomId` claim matches the requested room
3. User is a participant in the room

**Server-Side Validation:**

```csharp
public async Task JoinRoom(string roomId, string roomAccessToken)
{
    // Validate room access token
    var tokenRoomId = _roomTokenService.GetRoomIdFromToken(roomAccessToken);
    if (tokenRoomId == null || tokenRoomId.ToString() != roomId)
    {
        throw new HubException("Unauthorized: Invalid room access token");
    }
    
    // Additional participant verification...
}
```

---

## Security Best Practices

### 1. Password Security

✅ **DO:**
- Enforce strong password requirements
- Use BCrypt for password hashing (chat room passwords)
- Implement account lockout after failed attempts (future enhancement)
- Require password changes periodically

❌ **DON'T:**
- Store passwords in plain text
- Log passwords or tokens
- Share credentials across users

### 2. Token Security

✅ **DO:**
- Use HTTPS for all API requests
- Set short token expiration times (3 hours)
- Store tokens securely (HTTP-only cookies, secure storage)
- Implement token refresh mechanism
- Validate tokens on every request

❌ **DON'T:**
- Store tokens in localStorage (XSS risk)
- Send tokens in URLs (except SignalR query string on HTTPS)
- Reuse tokens across devices without proper tracking

### 3. CORS Configuration

The API is configured for specific frontend origins:

```csharp
policy.WithOrigins(
    "https://localhost:5173", 
    "https://localhost:5174",
    "https://localhost:3000",
    "https://localhost:8080",
    "http://localhost:5173",
    "http://localhost:5174",
    "http://localhost:3000",
    "http://localhost:8080")
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials();
```

**Production:** Update to specific production URLs.

### 4. Rate Limiting

The API implements rate limiting middleware. Excessive requests result in:

```json
{
  "error": "Rate limit exceeded. Please try again later."
}
```

### 5. SignalR Security

- Enable `EnableDetailedErrors` only in development
- Validate all hub method parameters
- Implement flood protection (30 messages/minute in chat)
- Sanitize user input to prevent XSS
- Encrypt sensitive chat messages

---

## Common Authentication Scenarios

### Scenario 1: Web Application Login

```javascript
async function login(username, password) {
  const response = await fetch('/api/Auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // Important for cookies
    body: JSON.stringify({ userName: username, password: password })
  });

  if (response.ok) {
    // Cookie is automatically set
    const user = await fetchCurrentUser();
    redirectToDashboard(user);
  } else {
    showError('Invalid credentials');
  }
}

async function fetchCurrentUser() {
  const response = await fetch('/api/Auth/me', {
    credentials: 'include'
  });
  return await response.json();
}
```

### Scenario 2: Mobile App Login

```csharp
// C# Mobile Client
public async Task<string> LoginAsync(string username, string password)
{
    var loginDto = new { userName = username, password = password };
    var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);
    
    if (response.IsSuccessStatusCode)
    {
        // Extract token from response (if modified to return token)
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        // Store token securely
        await SecureStorage.SetAsync("auth_token", result.Token);
        
        return result.Token;
    }
    
    throw new UnauthorizedAccessException("Invalid credentials");
}

public async Task<HttpRequestMessage> CreateAuthenticatedRequest(string url)
{
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    var token = await SecureStorage.GetAsync("auth_token");
    
    request.Headers.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    return request;
}
```

### Scenario 3: Token Expiration Handling

```javascript
async function apiCall(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    credentials: 'include'
  });

  if (response.status === 401) {
    // Token expired or invalid
    console.log('Session expired. Please login again.');
    
    // Clear any cached data
    clearCache();
    
    // Redirect to login
    window.location.href = '/login';
    return null;
  }

  return await response.json();
}
```

### Scenario 4: SignalR Reconnection with Re-authentication

```javascript
connection.onclose(async (error) => {
  if (error && error.message.includes('Unauthorized')) {
    // Re-authenticate
    const success = await attemptReLogin();
    
    if (success) {
      // Restart connection with new token
      await connection.start();
      await connection.invoke("JoinRoom", roomId, newRoomAccessToken);
    }
  }
});
```

---

## Configuration

### JWT Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyWith256BitsMinimum!",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience"
  }
}
```

**Security Notes:**
- **Key**: Must be at least 256 bits (32 characters)
- **Production**: Use environment variables or Azure Key Vault
- **Never commit**: Add to `.gitignore`

### Identity Configuration (Program.cs)

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();
```

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Error Handling](./ERROR_HANDLING.md)
- [Security Best Practices](./SECURITY_BEST_PRACTICES.md)
