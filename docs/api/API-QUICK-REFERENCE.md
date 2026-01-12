# School Management System - API Quick Reference

**Last Updated:** January 12, 2026

---

## 🚀 Quick Start

### Prerequisites
1. ✅ .NET 9.0 SDK installed
2. ✅ SQL Server LocalDB installed
3. ✅ Node.js/Bun for frontend

### Database Setup
```powershell
# Check LocalDB
sqllocaldb info

# Create/Start instance
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

# Run migrations
cd Backend/SMSPrototype1
dotnet ef database update

# Seed data (if seeder exists)
cd ../DataSeeder
dotnet run
```

### Start Backend
```powershell
cd Backend/SMSPrototype1
dotnet run
```
Backend will be available at: `https://localhost:7266`  
Swagger UI: `https://localhost:7266/swagger`

### Start Frontend
```powershell
cd Frontend
bun install
bun run dev
```
Frontend will be available at: `http://localhost:5173`

---

## 📚 API Modules Overview

| Module | Endpoints | Auth Required | Frontend Hook | Status |
|--------|-----------|---------------|---------------|--------|
| **Authentication** | 8 | Mixed | `authService.ts` | ✅ Complete |
| **Classes** | 5 | Yes | `useClasses.tsx` | ✅ Complete |
| **Students** | 6 | Yes | `useStudents.tsx` | ✅ Complete |
| **Teachers** | 5 | Yes | `useTeachers.tsx` | ✅ Complete |
| **Schools** | 5 | Mixed | `useSchools.tsx` | ⚠️ Partial |
| **Announcements** | 5 | Yes | `useAnnouncement.tsx` | ✅ Complete |
| **Attendance** | 5 | Yes | None | ❌ Missing Hook |
| **Teacher Attendance** | 5 | Yes | None | ❌ No Frontend |
| **Chat Rooms** | 7 | Yes | `useRooms.tsx` | ✅ Complete |
| **Dashboard** | 2 | Mixed | `useDashboardHome.tsx` | ✅ Complete |
| **ChatHub** (SignalR) | - | Yes | Used in ChatPage | ✅ Complete |
| **VideoCallHub** (SignalR) | - | Yes | Used in VideoCallPage | ✅ Complete |

---

## 🔐 Authorization Policies

| Policy | Roles Required | Usage |
|--------|---------------|-------|
| `AdminOnly` | Admin | School CRUD, System admin |
| `SchoolAdminOnly` | SchoolAdmin | School-level admin |
| `AdminOrSchoolAdmin` | Admin, SchoolAdmin | Most management features |
| `TeacherOrAbove` | Teacher, SchoolAdmin, Admin | Teaching features, view students |
| `StudentOrAbove` | All authenticated | General features |
| `SameSchool` | Any + same school | School-scoped data access |

---

## 📡 All API Endpoints

### Authentication (`/api/Auth`) - Public + Authenticated
```
POST   /api/Auth/register                      # Public
POST   /api/Auth/login                         # Public
GET    /api/Auth/me                            # 🔒 Auth Required
POST   /api/Auth/logout                        # 🔒 Auth Required
POST   /api/Auth/refresh                       # Public (uses cookie)
POST   /api/Auth/request-password-reset        # Public
POST   /api/Auth/reset-password                # Public
POST   /api/Auth/change-password               # 🔒 Auth Required
```

### Classes (`/api/Class`) - AdminOrSchoolAdmin
```
GET    /api/Class                              # 🔒 AdminOrSchoolAdmin
GET    /api/Class/{id}                         # 🔒 AdminOrSchoolAdmin
POST   /api/Class                              # 🔒 AdminOrSchoolAdmin
PUT    /api/Class/{id}                         # 🔒 AdminOrSchoolAdmin
DELETE /api/Class/{id}                         # 🔒 AdminOrSchoolAdmin
```

### Students (`/api/Student`) - TeacherOrAbove / AdminOrSchoolAdmin
```
GET    /api/Student                            # 🔒 TeacherOrAbove
GET    /api/Student/{id}                       # 🔒 TeacherOrAbove
GET    /api/Student/GetStudentByClassIdAsync/{classId}  # 🔒 TeacherOrAbove
POST   /api/Student                            # 🔒 AdminOrSchoolAdmin
PUT    /api/Student/{id}                       # 🔒 AdminOrSchoolAdmin
DELETE /api/Student/{id}                       # 🔒 AdminOrSchoolAdmin
```

### Teachers (`/api/Teacher`) - Mixed
```
GET    /api/Teacher                            # 🔒 AdminOrSchoolAdmin
GET    /api/Teacher/{id}                       # 🔒 TeacherOrAbove
POST   /api/Teacher                            # 🔒 AdminOrSchoolAdmin
PUT    /api/Teacher/{id}                       # 🔒 AdminOrSchoolAdmin
DELETE /api/Teacher/{id}                       # 🔒 AdminOrSchoolAdmin
```

### Schools (`/api/School`) - Mixed
```
GET    /api/School                             # Public (for registration)
GET    /api/School/search?schoolName={name}    # Public
GET    /api/School/getbyId/{schoolId}          # 🔒 Authenticated
POST   /api/School/CreateSchoolAsync           # 🔒 AdminOnly
PUT    /api/School/UpdateSchool/{schoolId}     # 🔒 AdminOrSchoolAdmin
DELETE /api/School/{schoolId}                  # 🔒 AdminOnly
```

### Attendance (`/api/Attendance`) - TeacherOrAbove
```
GET    /api/Attendance                         # 🔒 TeacherOrAbove
GET    /api/Attendance/{id}                    # 🔒 TeacherOrAbove
POST   /api/Attendance                         # 🔒 TeacherOrAbove
PUT    /api/Attendance/{id}                    # 🔒 TeacherOrAbove
DELETE /api/Attendance/{id}                    # 🔒 TeacherOrAbove
```

### Teacher Attendance (`/api/TeacherAttendance`) - AdminOrSchoolAdmin
```
GET    /api/TeacherAttendance/GetTeacherAttendance  # 🔒 AdminOrSchoolAdmin
GET    /api/TeacherAttendance/GetTeacherByAttendanceId/{id}  # 🔒 AdminOrSchoolAdmin
POST   /api/TeacherAttendance/createTeacherAttendance  # 🔒 AdminOrSchoolAdmin
PUT    /api/TeacherAttendance/{teacherId}      # 🔒 AdminOrSchoolAdmin
DELETE /api/TeacherAttendance/{attendanceId}   # 🔒 AdminOrSchoolAdmin
```

### Announcements (`/api/Announcement`) - Mixed
```
GET    /api/Announcement                       # 🔒 Authenticated (any)
GET    /api/Announcement/GetAnnouncementByIdAsync/{id}  # 🔒 Authenticated
POST   /api/Announcement/CreateAnnouncement    # 🔒 TeacherOrAbove
PUT    /api/Announcement/{id}                  # 🔒 TeacherOrAbove
DELETE /api/Announcement/{id}                  # 🔒 AdminOrSchoolAdmin
```

### Chat Rooms (`/api/ChatRooms`) - Authenticated
```
GET    /api/ChatRooms                          # 🔒 Authenticated
POST   /api/ChatRooms                          # 🔒 Authenticated
POST   /api/ChatRooms/join                     # 🔒 Authenticated
DELETE /api/ChatRooms/{id}                     # 🔒 Creator or Admin
POST   /api/ChatRooms/recording/start          # 🔒 Moderator
POST   /api/ChatRooms/recording/stop           # 🔒 Moderator
GET    /api/ChatRooms/{roomId}/recordings      # 🔒 Room Participant
```

### Dashboard/Combine (`/api/Combine`) - Mixed
```
GET    /api/Combine                            # Public (home stats)
GET    /api/Combine/{schoolId}                 # 🔒 Authenticated
```

---

## 🔌 SignalR Hubs

### ChatHub (`/chatHub`)
**Connection:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_URL}/chatHub?access_token=${token}`)
  .build();
```

**Methods:**
- `JoinRoom(roomId)` - Join a chat room
- `SendMessage(roomId, message)` - Send message to room
- `LeaveRoom(roomId)` - Leave a chat room

**Events:**
- `ReceiveMessage` - Receive new message
- `UserJoined` - User joined room
- `UserLeft` - User left room

### VideoCallHub (`/videoCallHub`)
**Connection:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_URL}/videoCallHub?access_token=${token}`)
  .build();
```

**Methods:**
- `JoinCall(roomId)` - Join video call
- `SendOffer(peerId, offer)` - WebRTC offer
- `SendAnswer(peerId, answer)` - WebRTC answer
- `SendIceCandidate(peerId, candidate)` - ICE candidate

---

## 📝 Common Request/Response Patterns

### Standard API Response
```json
{
  "content": {...},        // The actual data
  "isSuccess": true,       // Operation success status
  "statusCode": 200,       // HTTP status code
  "errorMessage": null     // Error message if failed
}
```

### Authentication Response
```json
{
  "message": "Login successful",
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "schoolId": "guid",
    "roles": ["Admin"]
  }
}
```

### Error Response
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 400,
  "errorMessage": "Validation error message"
}
```

---

## 🔑 Authentication Flow

### 1. Login
```javascript
POST /api/Auth/login
Body: { userName: "user", password: "pass" }

Response:
- Sets auth_token cookie (JWT, 3 hours)
- Sets refresh_token cookie (7 days)
- Returns user object
```

### 2. Accessing Protected Endpoints
```javascript
// Cookie is sent automatically
GET /api/Student

// Or use Bearer token
Headers: { Authorization: "Bearer {token}" }
```

### 3. Token Refresh
```javascript
POST /api/Auth/refresh
// Uses refresh_token from cookie

Response:
- New auth_token (3 hours)
- New refresh_token (rotated)
```

### 4. Logout
```javascript
POST /api/Auth/logout

Actions:
- Revokes all refresh tokens
- Blacklists current JWT
- Clears cookies
```

---

## 🧪 Testing

### Using PowerShell
```powershell
# Test register
$body = @{
    userName = "testuser"
    email = "test@example.com"
    password = "Test@1234"
    role = "Admin"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:7266/api/Auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Using Swagger
1. Navigate to `https://localhost:7266/swagger`
2. Test endpoints directly from UI
3. Use "Authorize" button to add JWT token

### Using Test Script
```powershell
cd D:\Projects\SMS\School-Management-System
.\api-test-script.ps1
```

---

## ⚠️ Known Issues

### Critical
- 🔴 **Database Connection**: Backend crashes on startup
  - **Solution**: Run migrations and seed data

### Medium
- ⚠️ **Missing Frontend**: Teacher Attendance has no UI
- ⚠️ **Partial Frontend**: School management CRUD incomplete
- ⚠️ **Debug Controller**: Should not be in production

### Low
- 🟡 **No Pagination**: All list endpoints return full data
- 🟡 **Hard Deletes**: No soft delete pattern
- 🟡 **BCrypt Warning**: Using outdated package version

---

## 📚 Documentation Files

- [00-API_STATUS_REPORT.md](./00-API_STATUS_REPORT.md) - **START HERE** - Complete status report
- [01-AUTHENTICATION_API.md](./01-AUTHENTICATION_API.md) - Auth endpoints documentation
- [02-CLASS_API.md](./02-CLASS_API.md) - Class management documentation
- [API-QUICK-REFERENCE.md](./API-QUICK-REFERENCE.md) - This file

---

## 🛠️ Development Commands

### Backend
```powershell
# Build
dotnet build

# Run
dotnet run

# Run migrations
dotnet ef migrations add MigrationName
dotnet ef database update

# Run tests
dotnet test
```

### Frontend
```powershell
# Install dependencies
bun install

# Development server
bun run dev

# Build for production
bun run build

# Type check
bun run type-check
```

---

## 📞 Need Help?

1. Check [00-API_STATUS_REPORT.md](./00-API_STATUS_REPORT.md) for detailed analysis
2. Review specific API documentation files
3. Check Swagger UI for live API documentation
4. Review backend code in `Backend/SMSPrototype1/Controllers/`
5. Review frontend hooks in `Frontend/src/hooks/`

---

**Maintained By**: Development Team  
**Project**: School Management System  
**Version**: 1.0  
**Last Updated**: January 12, 2026
