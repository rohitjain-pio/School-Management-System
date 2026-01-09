# Error Handling Reference Guide

## Overview

The School Management System API provides comprehensive error handling with standardized HTTP status codes, detailed error messages, and validation feedback. This guide covers all error responses, troubleshooting steps, and best practices for error handling in client applications.

## Table of Contents

- [HTTP Status Codes](#http-status-codes)
- [Response Structures](#response-structures)
- [Common Errors by Category](#common-errors-by-category)
- [Validation Errors](#validation-errors)
- [SignalR Connection Errors](#signalr-connection-errors)
- [Troubleshooting Guide](#troubleshooting-guide)
- [Error Handling Best Practices](#error-handling-best-practices)

---

## HTTP Status Codes

### Success Codes (2xx)

| Code | Status | Description | Usage |
|------|--------|-------------|-------|
| 200 | OK | Request successful | GET, PUT requests |
| 201 | Created | Resource created successfully | POST requests |
| 204 | No Content | Successful deletion | DELETE requests |

### Client Error Codes (4xx)

| Code | Status | Description | Common Causes |
|------|--------|-------------|---------------|
| 400 | Bad Request | Invalid request data | Validation failures, malformed JSON |
| 401 | Unauthorized | Authentication required/failed | Missing or invalid JWT token |
| 403 | Forbidden | Insufficient permissions | User lacks required role |
| 404 | Not Found | Resource doesn't exist | Invalid ID, deleted resource |
| 409 | Conflict | Resource conflict | Duplicate entries |
| 422 | Unprocessable Entity | Validation failed | Business logic violations |
| 429 | Too Many Requests | Rate limit exceeded | Flood protection triggered |

### Server Error Codes (5xx)

| Code | Status | Description | Action |
|------|--------|-------------|--------|
| 500 | Internal Server Error | Unexpected server error | Contact support, check logs |
| 503 | Service Unavailable | Service temporarily unavailable | Retry later |

---

## Response Structures

### ApiResult<T> (Standard Response)

Most endpoints return data wrapped in an `ApiResult<T>` structure:

```json
{
  "isSuccess": true,
  "data": {
    // Actual response data
  },
  "errorMessage": null
}
```

**Error Response:**

```json
{
  "isSuccess": false,
  "data": null,
  "errorMessage": "Description of what went wrong"
}
```

### Simple Object Response

Some endpoints return simple objects:

**Success:**
```json
{
  "ok": true,
  "message": "Operation successful"
}
```

**Error:**
```json
{
  "ok": false,
  "message": "Error description"
}
```

### Validation Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["Password must be at least 6 characters"]
  },
  "traceId": "00-trace-id-here"
}
```

---

## Common Errors by Category

### Authentication Errors

#### 1. Missing Authentication Token

**Status:** 401 Unauthorized

**Response:**
```json
{
  "ok": false,
  "message": "User not authenticated"
}
```

**Cause:**
- No JWT token provided
- Cookie not sent with request

**Solution:**
```javascript
// Ensure credentials are included
fetch('/api/endpoint', {
  credentials: 'include'
});

// Or include Bearer token
fetch('/api/endpoint', {
  headers: {
    'Authorization': 'Bearer ' + token
  }
});
```

#### 2. Invalid Credentials

**Status:** 401 Unauthorized

**Response:**
```json
{
  "ok": false,
  "message": "Invalid credentials"
}
```

**Cause:**
- Incorrect username or password
- User account doesn't exist

**Solution:**
- Verify username and password
- Check if user is registered
- Ensure correct schoolId for students/teachers

#### 3. Expired Token

**Status:** 401 Unauthorized

**Response:**
```json
{
  "ok": false,
  "message": "Token expired"
}
```

**Cause:**
- JWT token expired (default: 3 hours)

**Solution:**
- Re-authenticate via `/api/Auth/login`
- Implement automatic re-authentication before expiration

```javascript
// Check token expiration before requests
function isTokenExpired(token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  return payload.exp * 1000 < Date.now();
}

if (isTokenExpired(token)) {
  await reAuthenticate();
}
```

### Authorization Errors

#### 1. Insufficient Permissions

**Status:** 403 Forbidden

**Response:**
```json
{
  "ok": false,
  "message": "Forbidden"
}
```

**Cause:**
- User lacks required role (e.g., Student trying to delete records)
- Attempting to access another user's data

**Solution:**
- Check user's roles via `/api/Auth/me`
- Ensure proper role-based UI rendering
- Request admin assistance for privileged operations

#### 2. Not a Room Participant

**Status:** HubException

**Response:**
```
HubException: Unauthorized: Not a room participant
```

**Cause:**
- Trying to join SignalR room without being added as participant
- Invalid room access token

**Solution:**
- Join room via REST API first: `POST /api/ChatRooms/join`
- Use the returned `roomAccessToken`

---

### Validation Errors

#### 1. Missing Required Fields

**Status:** 400 Bad Request

**Response:**
```json
{
  "isSuccess": false,
  "errorMessage": "UserName is required; Password is required"
}
```

**Cause:**
- Required fields not provided in request

**Solution:**
```javascript
// Validate before sending
function validateRegistration(data) {
  const errors = [];
  if (!data.userName) errors.push('Username is required');
  if (!data.email) errors.push('Email is required');
  if (!data.password) errors.push('Password is required');
  if (!data.role) errors.push('Role is required');
  
  return errors;
}
```

#### 2. Password Requirements Not Met

**Status:** 400 Bad Request

**Response:**
```json
{
  "isSuccess": false,
  "errorMessage": "Password must be at least 6 characters; Password must contain at least one uppercase letter; Password must contain at least one digit"
}
```

**Requirements:**
- Minimum 6 characters
- At least one digit
- At least one lowercase letter
- At least one uppercase letter

**Solution:**
```javascript
function validatePassword(password) {
  const errors = [];
  if (password.length < 6) 
    errors.push('Password must be at least 6 characters');
  if (!/[a-z]/.test(password)) 
    errors.push('Password must contain a lowercase letter');
  if (!/[A-Z]/.test(password)) 
    errors.push('Password must contain an uppercase letter');
  if (!/\d/.test(password)) 
    errors.push('Password must contain a digit');
  
  return errors;
}
```

#### 3. Invalid Email Format

**Status:** 400 Bad Request

**Response:**
```json
{
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

**Solution:**
```javascript
function validateEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}
```

#### 4. Room Capacity Exceeded

**Status:** 400 Bad Request

**Response:**
```json
{
  "ok": false,
  "message": "Room is full"
}
```

**Cause:**
- Chat room reached `maxParticipants` limit

**Solution:**
- Wait for participants to leave
- Create a new room
- Admin can increase room capacity

---

### Resource Errors

#### 1. Resource Not Found

**Status:** 404 Not Found

**Response:**
```json
{
  "ok": false,
  "message": "Room not found"
}
```

**Common Resources:**
- Students
- Teachers
- Classes
- Schools
- Chat Rooms
- Announcements

**Solution:**
- Verify ID is correct (valid GUID format)
- Check if resource was deleted
- Refresh resource list

#### 2. Duplicate Resource

**Status:** 409 Conflict

**Response:**
```json
{
  "isSuccess": false,
  "errorMessage": "A user with this username already exists"
}
```

**Cause:**
- Username already registered
- Email already in use
- Duplicate class names in same school

**Solution:**
- Choose different username/email
- Check existing records before creation

---

### Rate Limiting Errors

#### 1. API Rate Limit Exceeded

**Status:** 429 Too Many Requests

**Response:**
```json
{
  "error": "Rate limit exceeded. Please try again later."
}
```

**Limits:**
- General API: Configurable per IP/user
- Chat messages: 30 messages per minute per user

**Solution:**
- Implement exponential backoff
- Cache frequently requested data
- Reduce request frequency

```javascript
async function retryWithBackoff(fn, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      if (error.status === 429 && i < maxRetries - 1) {
        const delay = Math.pow(2, i) * 1000; // 1s, 2s, 4s
        await new Promise(resolve => setTimeout(resolve, delay));
      } else {
        throw error;
      }
    }
  }
}
```

#### 2. Chat Flood Protection

**Status:** HubException

**Response:**
```
HubException: Rate limit exceeded. Please slow down.
```

**Cause:**
- Sending more than 30 messages per minute in chat

**Solution:**
- Implement client-side rate limiting
- Queue messages
- Display warning to user

```javascript
class MessageQueue {
  constructor(maxPerMinute = 30) {
    this.queue = [];
    this.timestamps = [];
    this.maxPerMinute = maxPerMinute;
  }

  canSend() {
    const now = Date.now();
    this.timestamps = this.timestamps.filter(
      t => now - t < 60000
    );
    return this.timestamps.length < this.maxPerMinute;
  }

  async send(message, sendFn) {
    if (this.canSend()) {
      this.timestamps.push(Date.now());
      await sendFn(message);
    } else {
      throw new Error('Rate limit: Please wait before sending more messages');
    }
  }
}
```

---

## SignalR Connection Errors

### 1. Connection Failed

**Error:**
```
Error: Failed to complete negotiation with the server
```

**Causes:**
- Server not running
- CORS configuration issues
- Network connectivity problems
- Invalid URL

**Solution:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub", {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect({
    nextRetryDelayInMilliseconds: (retryContext) => {
      if (retryContext.elapsedMilliseconds < 60000) {
        return Math.random() * 10000;
      } else {
        return null; // Stop retrying
      }
    }
  })
  .build();

connection.onclose(async (error) => {
  console.error('Connection closed:', error);
  // Implement reconnection logic
});

try {
  await connection.start();
} catch (err) {
  console.error('Connection failed:', err);
  // Show user-friendly error message
}
```

### 2. Unauthorized Hub Access

**Error:**
```
HubException: Unauthorized: User not authenticated
```

**Causes:**
- Missing JWT token
- Invalid token
- Token not passed correctly to hub

**Solution:**
```javascript
// Ensure token is provided
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub", {
    accessTokenFactory: async () => {
      const token = await getAuthToken();
      if (!token) {
        throw new Error('No authentication token available');
      }
      return token;
    }
  })
  .build();
```

### 3. Invalid Room Access Token

**Error:**
```
HubException: Unauthorized: Invalid room access token
```

**Causes:**
- Wrong room access token
- Token expired
- Token for different room

**Solution:**
```javascript
// Get fresh room access token
const joinResponse = await fetch('/api/ChatRooms/join', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify({ roomId, password })
});

const { roomAccessToken } = await joinResponse.json();

// Use fresh token
await connection.invoke("JoinRoom", roomId, roomAccessToken);
```

### 4. Kicked from Room

**Event:**
```
KickedFromRoom: "You have been removed from the call by a moderator"
```

**Cause:**
- Moderator removed you from room

**Solution:**
```javascript
connection.on("KickedFromRoom", (message) => {
  console.log('Kicked:', message);
  
  // Clean up local state
  stopMediaStreams();
  
  // Redirect or show message
  alert(message);
  window.location.href = '/rooms';
});
```

---

## Troubleshooting Guide

### Authentication Issues

**Problem:** Can't login, always get 401

**Checklist:**
1. ✅ Verify username and password are correct
2. ✅ Check if user is registered (`POST /api/Auth/register`)
3. ✅ Ensure `Content-Type: application/json` header is set
4. ✅ Check request body format matches expected DTO
5. ✅ Verify backend is running on correct port

**Debug Steps:**
```bash
# Test registration
curl -X POST https://localhost:7266/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"userName":"test","email":"test@test.com","password":"Test123","role":"Student","schoolId":null}' \
  -v

# Test login
curl -X POST https://localhost:7266/api/Auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{"userName":"test","password":"Test123"}' \
  -v

# Verify token
curl -X GET https://localhost:7266/api/Auth/me \
  -b cookies.txt \
  -v
```

---

### SignalR Connection Issues

**Problem:** SignalR connection keeps disconnecting

**Checklist:**
1. ✅ Check network stability
2. ✅ Verify server is running
3. ✅ Implement automatic reconnection
4. ✅ Check for token expiration
5. ✅ Monitor browser console for errors

**Solution:**
```javascript
connection.onreconnecting((error) => {
  console.log('Reconnecting...', error);
  showReconnectingUI();
});

connection.onreconnected((connectionId) => {
  console.log('Reconnected!', connectionId);
  hideReconnectingUI();
  
  // Re-join room
  connection.invoke("JoinRoom", roomId, roomAccessToken);
});

connection.onclose((error) => {
  console.error('Connection closed', error);
  
  if (error) {
    // Attempt manual reconnection
    setTimeout(() => connection.start(), 5000);
  }
});
```

---

### CORS Issues

**Problem:** Request blocked by CORS policy

**Error:**
```
Access to fetch at 'https://localhost:7266/api/Auth/login' from origin 
'http://localhost:5173' has been blocked by CORS policy
```

**Causes:**
- Frontend URL not in CORS whitelist
- Missing `AllowCredentials`
- Preflight request failed

**Backend Fix (Program.cs):**
```csharp
options.AddPolicy("AllowFrontend", policy =>
{
    policy.WithOrigins("http://localhost:5173") // Add your frontend URL
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
});
```

**Frontend Fix:**
```javascript
fetch('/api/endpoint', {
  credentials: 'include', // Important!
  // ...
});
```

---

## Error Handling Best Practices

### 1. Centralized Error Handling

```javascript
class ApiClient {
  async request(url, options = {}) {
    try {
      const response = await fetch(url, {
        ...options,
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...options.headers
        }
      });

      if (!response.ok) {
        return this.handleError(response);
      }

      return await response.json();
    } catch (error) {
      return this.handleNetworkError(error);
    }
  }

  async handleError(response) {
    const error = await response.json();
    
    switch (response.status) {
      case 400:
        throw new ValidationError(error.errorMessage || error.errors);
      case 401:
        this.redirectToLogin();
        throw new AuthenticationError('Please login again');
      case 403:
        throw new AuthorizationError('Insufficient permissions');
      case 404:
        throw new NotFoundError('Resource not found');
      case 429:
        throw new RateLimitError('Too many requests');
      case 500:
        throw new ServerError('Server error. Please try again later');
      default:
        throw new Error('Unknown error occurred');
    }
  }

  handleNetworkError(error) {
    console.error('Network error:', error);
    throw new NetworkError('Network connection failed');
  }

  redirectToLogin() {
    localStorage.clear();
    window.location.href = '/login';
  }
}
```

### 2. User-Friendly Error Messages

```javascript
const ERROR_MESSAGES = {
  'Invalid credentials': 'Username or password is incorrect',
  'Room is full': 'This room has reached maximum capacity',
  'Rate limit exceeded': 'You\'re sending messages too quickly. Please wait.',
  'Unauthorized: Not a room participant': 'You need to join this room first'
};

function getUserFriendlyMessage(error) {
  return ERROR_MESSAGES[error] || 'Something went wrong. Please try again.';
}
```

### 3. Graceful Degradation

```javascript
async function loadData() {
  try {
    const data = await api.fetchData();
    displayData(data);
  } catch (error) {
    if (error instanceof NotFoundError) {
      displayEmptyState();
    } else if (error instanceof NetworkError) {
      displayOfflineMode();
      enableRetryButton();
    } else {
      displayGenericError();
    }
  }
}
```

### 4. Logging and Monitoring

```javascript
function logError(error, context = {}) {
  console.error('Error occurred:', {
    message: error.message,
    stack: error.stack,
    context,
    timestamp: new Date().toISOString(),
    userAgent: navigator.userAgent,
    url: window.location.href
  });

  // Send to monitoring service (optional)
  // sendToMonitoring(error, context);
}

try {
  await riskyOperation();
} catch (error) {
  logError(error, { operation: 'riskyOperation', userId: currentUser.id });
  throw error;
}
```

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Authentication](./AUTHENTICATION.md)
- [API Reference](./API_REFERENCE.md)
