# Getting Started with School Management System API

## Overview

The School Management System (SMS) API is a comprehensive REST API built with ASP.NET Core, providing complete functionality for managing educational institutions. This guide will walk you through initial setup, user registration, and common workflows.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Base URL and Authentication](#base-url-and-authentication)
- [Quick Start](#quick-start)
- [Common Workflows](#common-workflows)
  - [School Administrator Setup](#school-administrator-setup)
  - [Student Enrollment](#student-enrollment)
  - [Teacher Onboarding](#teacher-onboarding)
  - [Class Management](#class-management)
  - [Attendance Tracking](#attendance-tracking)
  - [Chat Room Creation](#chat-room-creation)
  - [Video Call Setup](#video-call-setup)
- [Next Steps](#next-steps)

---

## Prerequisites

Before you begin, ensure you have:

- **API Base URL**: `https://localhost:7266` (development) or your production URL
- **Valid credentials** or ability to register new users
- **API client** (Postman, curl, or your application)
- For real-time features: **SignalR client library** (JavaScript, C#, etc.)

---

## Base URL and Authentication

### Base URL

```
Development: https://localhost:7266
Production: https://your-production-url.com
```

### Authentication

All authenticated endpoints require a JWT Bearer token. The API uses cookie-based authentication for web clients and supports query string tokens for SignalR connections.

**Authentication Flow:**

1. Register or login to obtain JWT token
2. Token is set as an HTTP-only cookie (`auth_token`)
3. Include token in Authorization header for API calls: `Authorization: Bearer <token>`
4. For SignalR: Token passed via query string or cookie

---

## Quick Start

### Step 1: Register Your First User (School Administrator)

```bash
curl -X POST https://localhost:7266/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "email": "admin@school.com",
    "password": "Admin123!",
    "role": "Admin",
    "schoolId": null
  }'
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Registration successful!"
}
```

**Available Roles:**
- `Admin` / `SuperAdmin` / `Principal` / `SchoolIncharge` - Full system access
- `Teacher` - Teacher-specific features
- `Student` - Student-specific features

### Step 2: Login to Get Authentication Token

```bash
curl -X POST https://localhost:7266/api/Auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "userName": "admin",
    "password": "Admin123!"
  }'
```

**Response:**
```json
{
  "message": "Login successful"
}
```

The JWT token is automatically set as an HTTP-only cookie. For programmatic access (non-browser), you can modify the backend to return the token directly.

### Step 3: Verify Authentication

```bash
curl -X GET https://localhost:7266/api/Auth/me \
  -b cookies.txt
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "admin",
  "email": "admin@school.com",
  "schoolId": null,
  "roles": ["Admin"]
}
```

---

## Common Workflows

### School Administrator Setup

#### 1. Create a School

```bash
curl -X POST https://localhost:7266/api/School \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Lincoln High School",
    "address": "123 Education Ave, Springfield",
    "phoneNumber": "+1-555-0100",
    "email": "info@lincolnhigh.edu",
    "principalName": "Dr. Jane Smith"
  }'
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Lincoln High School",
    "address": "123 Education Ave, Springfield",
    "phoneNumber": "+1-555-0100",
    "email": "info@lincolnhigh.edu",
    "principalName": "Dr. Jane Smith"
  },
  "errorMessage": null
}
```

#### 2. Create Classes/Grades

```bash
curl -X POST https://localhost:7266/api/Class \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Grade 10",
    "section": "A",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "capacity": 30
  }'
```

---

### Student Enrollment

#### 1. Register a Student Account

```bash
curl -X POST https://localhost:7266/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john.doe",
    "email": "john.doe@student.lincolnhigh.edu",
    "password": "Student123!",
    "role": "Student",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  }'
```

#### 2. Create Student Profile

After registration, create the student's detailed profile:

```bash
curl -X POST https://localhost:7266/api/Student \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "2008-05-15",
    "gender": "Male",
    "address": "456 Student St, Springfield",
    "phoneNumber": "+1-555-0101",
    "email": "john.doe@student.lincolnhigh.edu",
    "guardianName": "Jane Doe",
    "guardianPhoneNumber": "+1-555-0102",
    "classId": "class-id-here",
    "enrollmentDate": "2024-01-09",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  }'
```

#### 3. Assign to Class

Student-class assignment is typically handled during profile creation via `classId`. To reassign:

```bash
curl -X PUT https://localhost:7266/api/Student/{studentId} \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "classId": "new-class-id"
  }'
```

---

### Teacher Onboarding

#### 1. Register Teacher Account

```bash
curl -X POST https://localhost:7266/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "mary.smith",
    "email": "mary.smith@lincolnhigh.edu",
    "password": "Teacher123!",
    "role": "Teacher",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  }'
```

#### 2. Create Teacher Profile

```bash
curl -X POST https://localhost:7266/api/Teacher \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "firstName": "Mary",
    "lastName": "Smith",
    "dateOfBirth": "1985-03-20",
    "gender": "Female",
    "address": "789 Teacher Rd, Springfield",
    "phoneNumber": "+1-555-0200",
    "email": "mary.smith@lincolnhigh.edu",
    "subject": "Mathematics",
    "qualification": "M.Ed. Mathematics",
    "hireDate": "2024-01-09",
    "schoolId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  }'
```

#### 3. Assign Classes to Teacher

Teachers can be assigned to teach specific classes (handled via class assignment endpoints).

---

### Class Management

#### 1. List All Classes

```bash
curl -X GET https://localhost:7266/api/Class \
  -b cookies.txt
```

#### 2. Get Class Details with Students

```bash
curl -X GET https://localhost:7266/api/Class/{classId} \
  -b cookies.txt
```

#### 3. Update Class Information

```bash
curl -X PUT https://localhost:7266/api/Class/{classId} \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Grade 10",
    "section": "B",
    "capacity": 35
  }'
```

---

### Attendance Tracking

#### 1. Mark Student Attendance

```bash
curl -X POST https://localhost:7266/api/Attendance \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "studentId": "student-id-here",
    "date": "2024-01-09",
    "status": "Present",
    "remarks": "On time"
  }'
```

**Status Values:** `Present`, `Absent`, `Late`, `Excused`

#### 2. Mark Teacher Attendance

```bash
curl -X POST https://localhost:7266/api/TeacherAttendance \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "teacherId": "teacher-id-here",
    "date": "2024-01-09",
    "status": "Present",
    "remarks": ""
  }'
```

#### 3. Get Attendance Report

```bash
# Student attendance for a specific date range
curl -X GET "https://localhost:7266/api/Attendance?studentId={id}&startDate=2024-01-01&endDate=2024-01-09" \
  -b cookies.txt
```

---

### Chat Room Creation

Chat rooms support encrypted messaging with role-based permissions.

#### 1. Create a Chat Room

```bash
curl -X POST https://localhost:7266/api/ChatRooms \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Grade 10A Discussion",
    "description": "Class discussion room for Grade 10 Section A",
    "password": "SecurePass123!",
    "privacyLevel": "Private",
    "maxParticipants": 50,
    "allowRecording": true
  }'
```

**Response:**
```json
{
  "id": "room-id-here",
  "name": "Grade 10A Discussion",
  "description": "Class discussion room for Grade 10 Section A",
  "createdBy": "admin-user-id",
  "createdByUsername": "admin",
  "createdAt": "2024-01-09T10:00:00Z",
  "privacyLevel": "Private",
  "maxParticipants": 50,
  "currentParticipants": 1,
  "allowRecording": true,
  "isEncrypted": true,
  "isActive": true,
  "isUserModerator": true
}
```

**Privacy Levels:**
- `Public` - Anyone can discover and join
- `Private` - Invite-only, requires password
- `Protected` - Discoverable but requires password

#### 2. Join a Chat Room

```bash
curl -X POST https://localhost:7266/api/ChatRooms/join \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "roomId": "room-id-here",
    "password": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "ok": true,
  "message": "Successfully joined room",
  "roomAccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roomDetails": {
    "id": "room-id-here",
    "name": "Grade 10A Discussion",
    "isUserModerator": false
  }
}
```

**Important:** Save the `roomAccessToken` - it's required for SignalR hub connection.

#### 3. Connect to Chat via SignalR

**JavaScript Example:**

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/chatHub", {
    accessTokenFactory: () => authToken // Your JWT token
  })
  .withAutomaticReconnect()
  .build();

// Start connection
await connection.start();

// Join room with access token
await connection.invoke("JoinRoom", roomId, roomAccessToken);

// Listen for messages
connection.on("ReceiveMessage", (message) => {
  console.log(`${message.sender}: ${message.content}`);
});

// Send message
await connection.invoke("SendMessage", roomId, "Hello everyone!");

// Load message history
const history = await connection.invoke("LoadMessageHistory", roomId, 50);
console.log(history);
```

---

### Video Call Setup

Video calls use WebRTC with SignalR for signaling.

#### 1. Use Existing Room or Create New Room

Video calls use the same chat rooms. Follow [Chat Room Creation](#chat-room-creation) steps.

#### 2. Connect to Video Call Hub

**JavaScript Example:**

```javascript
const videoConnection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7266/videoCallHub", {
    accessTokenFactory: () => authToken
  })
  .withAutomaticReconnect()
  .build();

await videoConnection.start();

// Join video room
await videoConnection.invoke("JoinVideoRoom", roomId, roomAccessToken);

// Listen for existing participants
videoConnection.on("ExistingParticipants", async (participants) => {
  for (const participant of participants) {
    // Create peer connection and send offer
    await createPeerConnection(participant.connectionId);
  }
});

// Listen for new users joining
videoConnection.on("UserJoinedCall", async (user) => {
  // Create peer connection for new user
  await createPeerConnection(user.connectionId);
});

// WebRTC signaling
videoConnection.on("ReceiveOffer", async (connectionId, offer) => {
  await handleOffer(connectionId, offer);
});

videoConnection.on("ReceiveAnswer", async (connectionId, answer) => {
  await handleAnswer(connectionId, answer);
});

videoConnection.on("ReceiveIceCandidate", async (connectionId, candidate) => {
  await handleIceCandidate(connectionId, candidate);
});

// Send WebRTC offer
async function sendOffer(targetConnectionId, offer) {
  await videoConnection.invoke("SendOffer", targetConnectionId, offer);
}

// Toggle audio/video
async function toggleMedia(audioEnabled, videoEnabled) {
  await videoConnection.invoke("UpdateMediaState", roomId, audioEnabled, videoEnabled);
}
```

#### 3. Start/Stop Recording (Moderators Only)

```bash
# Start recording
curl -X POST https://localhost:7266/api/ChatRooms/recording/start \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "roomId": "room-id-here",
    "roomAccessToken": "access-token-here"
  }'

# Stop recording
curl -X POST https://localhost:7266/api/ChatRooms/recording/stop \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "recordingId": "recording-id-here"
  }'

# Get room recordings
curl -X GET https://localhost:7266/api/ChatRooms/{roomId}/recordings \
  -b cookies.txt
```

---

## Next Steps

### Explore More Features

1. **Announcements**: Create school-wide or class-specific announcements
   - `POST /api/Announcement`
   
2. **Combined Details**: Get aggregated data for dashboards
   - `GET /api/Combine/*`

3. **Advanced Attendance**: Bulk attendance operations, reports, analytics

### Read More Documentation

- [Authentication & Authorization Guide](./AUTHENTICATION.md) - In-depth auth strategies
- [Error Handling Reference](./ERROR_HANDLING.md) - Complete error codes and solutions
- [SignalR Real-time Features](./signalr/QUICK_REFERENCE.md) - Advanced real-time patterns
- [Code Samples Library](./CODE_SAMPLES.md) - Multi-language examples
- [API Reference](./API_REFERENCE.md) - Complete endpoint documentation

### Integration Guides

- [External Partner Integration](./PARTNER_INTEGRATION.md) - Third-party integrations
- [Postman Collection Guide](./POSTMAN_GUIDE.md) - Testing with Postman
- [SDK Documentation](./SDK_GUIDE.md) - C# and TypeScript client libraries

### Get Help

- Check [FAQ](./FAQ.md) for common questions
- Review [Troubleshooting Guide](./ERROR_HANDLING.md) for solutions
- Consult [Security Best Practices](./SECURITY_BEST_PRACTICES.md)

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**API Base URL:** https://localhost:7266
