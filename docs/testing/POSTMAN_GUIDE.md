# Postman Collection Guide

## Overview

This guide explains how to set up and use Postman to test the School Management System API. It includes collection setup, environment configuration, authentication workflows, and test automation.

## Table of Contents

- [Installation](#installation)
- [Collection Setup](#collection-setup)
- [Environment Configuration](#environment-configuration)
- [Authentication Setup](#authentication-setup)
- [Testing Workflows](#testing-workflows)
- [Automated Testing](#automated-testing)

---

## Installation

### Install Postman

Download and install Postman from [postman.com](https://www.postman.com/downloads/)

**Supported Platforms:**
- Windows
- macOS
- Linux

---

## Collection Setup

### Option 1: Export from Swagger

1. **Access Swagger UI:**
   ```
   https://localhost:7266/swagger/index.html
   ```

2. **Export OpenAPI Specification:**
   - Click on the `/swagger/v1/swagger.json` link
   - Save the JSON file

3. **Import to Postman:**
   - Open Postman
   - Click **Import** button
   - Select the `swagger.json` file
   - Postman will create a collection with all endpoints

### Option 2: Manual Collection Creation

**Create a new collection:**

1. Click **New** → **Collection**
2. Name: `School Management System API`
3. Add description:
   ```
   Complete REST API for school management including authentication, 
   student/teacher management, attendance, and real-time features.
   ```

**Add folders:**
- Authentication
- Schools
- Students
- Teachers
- Classes
- Attendance
- Announcements
- Chat Rooms
- Combined Details

---

## Environment Configuration

### Create Development Environment

1. **Create Environment:**
   - Click **Environments** → **Create Environment**
   - Name: `SMS API - Development`

2. **Add Variables:**

| Variable | Initial Value | Current Value |
|----------|---------------|---------------|
| `base_url` | `https://localhost:7266` | `https://localhost:7266` |
| `api_url` | `{{base_url}}/api` | `{{base_url}}/api` |
| `auth_token` | (empty) | (set by login script) |
| `school_id` | (empty) | (set manually or by script) |
| `student_id` | (empty) | (set by create student) |
| `teacher_id` | (empty) | (set by create teacher) |
| `class_id` | (empty) | (set by create class) |
| `room_id` | (empty) | (set by create room) |
| `room_access_token` | (empty) | (set by join room) |

3. **Select Environment:**
   - Choose `SMS API - Development` from environment dropdown

### Create Production Environment

Repeat the process with production URLs when ready to deploy.

---

## Authentication Setup

### Register Request

**Method:** POST  
**URL:** `{{api_url}}/Auth/register`  
**Headers:**
```
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "userName": "testuser",
  "email": "testuser@school.edu",
  "password": "Test123",
  "role": "Admin",
  "schoolId": null
}
```

### Login Request with Pre-request Script

**Method:** POST  
**URL:** `{{api_url}}/Auth/login`  
**Headers:**
```
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "userName": "testuser",
  "password": "Test123"
}
```

**Tests Tab (Extract Cookie):**
```javascript
// Extract auth_token from cookie if using cookie-based auth
const cookies = pm.cookies.all();
console.log('Cookies:', cookies);

// For development: If API returns token in response
if (pm.response.code === 200) {
    const response = pm.response.json();
    if (response.token) {
        pm.environment.set("auth_token", response.token);
        console.log("Auth token saved:", response.token.substring(0, 20) + "...");
    }
}

// Alternatively, extract from Set-Cookie header
const setCookieHeader = pm.response.headers.get("Set-Cookie");
if (setCookieHeader && setCookieHeader.includes("auth_token")) {
    // Parse cookie value
    const match = setCookieHeader.match(/auth_token=([^;]+)/);
    if (match) {
        pm.environment.set("auth_token", match[1]);
    }
}
```

**Note:** For cookie-based authentication, Postman automatically manages cookies. The script above is useful if you need to extract the token for header-based auth.

### Get Current User Request

**Method:** GET  
**URL:** `{{api_url}}/Auth/me`  
**Headers:**
```
Authorization: Bearer {{auth_token}}
```

**Tests Tab:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("User data is present", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('username');
    pm.expect(jsonData).to.have.property('email');
});

// Save user ID for later use
const user = pm.response.json();
pm.environment.set("current_user_id", user.id);
```

---

## Testing Workflows

### Complete Student Enrollment Workflow

This folder demonstrates the complete process of enrolling a student.

#### 1. Create School

**Method:** POST  
**URL:** `{{api_url}}/School`  
**Body:**
```json
{
  "name": "Test High School",
  "address": "123 Test St",
  "phoneNumber": "+1-555-0100",
  "email": "info@testschool.edu",
  "principalName": "Dr. Principal"
}
```

**Tests:**
```javascript
pm.test("School created successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response.isSuccess).to.be.true;
    
    // Save school ID
    pm.environment.set("school_id", response.data.id);
});
```

#### 2. Create Class

**Method:** POST  
**URL:** `{{api_url}}/Class`  
**Body:**
```json
{
  "name": "Grade 10",
  "section": "A",
  "schoolId": "{{school_id}}",
  "capacity": 30
}
```

**Tests:**
```javascript
pm.test("Class created successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    
    // Save class ID
    pm.environment.set("class_id", response.data.id);
});
```

#### 3. Register Student Account

**Method:** POST  
**URL:** `{{api_url}}/Auth/register`  
**Body:**
```json
{
  "userName": "john.doe",
  "email": "john.doe@student.test.edu",
  "password": "Student123",
  "role": "Student",
  "schoolId": "{{school_id}}"
}
```

#### 4. Create Student Profile

**Method:** POST  
**URL:** `{{api_url}}/Student`  
**Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "2008-05-15",
  "gender": "Male",
  "address": "456 Student St",
  "phoneNumber": "+1-555-0101",
  "email": "john.doe@student.test.edu",
  "guardianName": "Jane Doe",
  "guardianPhoneNumber": "+1-555-0102",
  "classId": "{{class_id}}",
  "enrollmentDate": "2024-01-09",
  "schoolId": "{{school_id}}"
}
```

**Tests:**
```javascript
pm.test("Student created successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    
    // Save student ID
    pm.environment.set("student_id", response.data.id);
});
```

### Chat Room Workflow

#### 1. Create Chat Room

**Method:** POST  
**URL:** `{{api_url}}/ChatRooms`  
**Body:**
```json
{
  "name": "Test Chat Room",
  "description": "Test room for API testing",
  "password": "TestRoom123",
  "privacyLevel": "Private",
  "maxParticipants": 50,
  "allowRecording": true
}
```

**Tests:**
```javascript
pm.test("Room created successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    
    // Save room ID
    pm.environment.set("room_id", response.id);
});
```

#### 2. Join Chat Room

**Method:** POST  
**URL:** `{{api_url}}/ChatRooms/join`  
**Body:**
```json
{
  "roomId": "{{room_id}}",
  "password": "TestRoom123"
}
```

**Tests:**
```javascript
pm.test("Joined room successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response.ok).to.be.true;
    
    // Save room access token
    pm.environment.set("room_access_token", response.roomAccessToken);
});
```

---

## Automated Testing

### Collection-Level Pre-request Script

Add to collection settings to run before every request:

```javascript
// Ensure authentication
const authToken = pm.environment.get("auth_token");
if (!authToken && pm.info.requestName !== "Login" && pm.info.requestName !== "Register") {
    console.warn("Warning: No auth token found. Please login first.");
}

// Log request details
console.log(`Request: ${pm.info.requestName}`);
console.log(`URL: ${pm.request.url}`);
```

### Collection-Level Test Script

Add to collection settings to run after every request:

```javascript
// Common tests for all requests
pm.test("Response time is less than 2000ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(2000);
});

pm.test("Response has Content-Type header", function () {
    pm.response.to.have.header("Content-Type");
});

// Log response status
console.log(`Response Status: ${pm.response.code} ${pm.response.status}`);

// Check for common error patterns
if (pm.response.code >= 400) {
    console.error("Error Response:", pm.response.json());
}
```

### Collection Runner

**Run entire collection:**

1. Click **Collections** → **Run collection**
2. Select `School Management System API`
3. Configure:
   - **Environment:** SMS API - Development
   - **Iterations:** 1
   - **Delay:** 500ms between requests
   - **Data File:** (optional) CSV with test data
4. Click **Run**

**Review results:**
- View passed/failed tests
- Check response times
- Export results as JSON/HTML

### Newman (CLI Runner)

**Install Newman:**
```bash
npm install -g newman
```

**Export Collection:**
1. Right-click collection → **Export**
2. Save as `SMS_API_Collection.json`

**Run tests:**
```bash
newman run SMS_API_Collection.json \
  -e SMS_Development_Environment.json \
  --reporters cli,html \
  --reporter-html-export newman-report.html
```

**CI/CD Integration:**
```yaml
# GitHub Actions example
- name: Run API Tests
  run: |
    newman run postman/SMS_API_Collection.json \
      -e postman/SMS_Development_Environment.json \
      --reporters junit \
      --reporter-junit-export results.xml
```

---

## Advanced Features

### Dynamic Variables

Use Postman's built-in dynamic variables:

```json
{
  "email": "user_{{$randomInt}}@test.com",
  "phoneNumber": "+1-{{$randomInt}}",
  "timestamp": "{{$timestamp}}"
}
```

### Data-Driven Testing

**Create CSV file (`students.csv`):**
```csv
firstName,lastName,email
Alice,Johnson,alice.j@test.edu
Bob,Smith,bob.s@test.edu
Carol,Williams,carol.w@test.edu
```

**Run collection with data file:**
- Select data file in Collection Runner
- Postman runs request once per row
- Access data: `{{firstName}}`, `{{lastName}}`, etc.

### Mock Server

**Create mock server:**
1. Right-click collection → **Mock Collection**
2. Set mock URL in environment: `mock_url`
3. Use for frontend development without backend

---

## Troubleshooting

### SSL Certificate Errors

**Issue:** `Error: unable to verify the first certificate`

**Solution:**
- Go to Settings → **SSL certificate verification** → Turn OFF (development only)

### Cookie Not Persisting

**Issue:** Auth cookie not saved between requests

**Solution:**
- Ensure all requests in the same domain
- Check cookie settings in Postman
- Use Postman Interceptor extension for browser cookies

### Environment Variable Not Set

**Issue:** `{{variable}}` appears in request instead of value

**Solution:**
- Verify environment is selected
- Check variable name spelling
- Ensure test script sets the variable correctly

---

## Export and Sharing

### Export Collection

1. Right-click collection → **Export**
2. Choose format: Collection v2.1 (recommended)
3. Share JSON file with team

### Publish Documentation

1. Click collection → **View Documentation**
2. Click **Publish**
3. Choose public or team visibility
4. Share generated URL

---

## Best Practices

1. **Use Environments:** Separate dev/staging/production
2. **Add Tests:** Validate responses automatically
3. **Use Variables:** Make requests reusable
4. **Document Requests:** Add descriptions and examples
5. **Version Control:** Commit collections to Git
6. **Organize Folders:** Group related requests
7. **Add Examples:** Save successful responses
8. **Monitor API:** Set up Postman Monitors for uptime checks

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Authentication](./AUTHENTICATION.md)
- [Error Handling](./ERROR_HANDLING.md)
