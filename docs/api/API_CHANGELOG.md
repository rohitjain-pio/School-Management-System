# API Changelog & Versioning Strategy

## Overview

This document tracks changes, additions, and deprecations in the School Management System API. It includes version history, migration guides, and backward compatibility notes.

## Current Version

**Version:** 1.0  
**Status:** Stable  
**Release Date:** January 9, 2026  
**Base URL:** `https://localhost:7266` (Development)

---

## Table of Contents

- [Versioning Strategy](#versioning-strategy)
- [Version History](#version-history)
- [v1 to v2 Migration Guide](#v1-to-v2-migration-guide)
- [Breaking Changes](#breaking-changes)
- [Deprecations](#deprecations)
- [Backward Compatibility](#backward-compatibility)

---

## Versioning Strategy

### Semantic Versioning

The API follows **Semantic Versioning 2.0.0** (SemVer):

```
MAJOR.MINOR.PATCH

Example: 2.1.3
- MAJOR (2): Breaking changes
- MINOR (1): New features (backward compatible)
- PATCH (3): Bug fixes (backward compatible)
```

### Version Format

**API Versioning Approaches:**

1. **URL Path Versioning** (Planned for v2)
   ```
   /api/v1/Students
   /api/v2/Students
   ```

2. **Header Versioning** (Future consideration)
   ```
   X-API-Version: 2.0
   ```

3. **Query Parameter** (Not recommended)
   ```
   /api/Students?version=2.0
   ```

**Current Implementation:**
- **v1.0**: No explicit version in URL (default)
- **Swagger**: `/swagger/v1/swagger.json`

### Version Support Policy

| Version | Status | Support End | Notes |
|---------|--------|-------------|-------|
| v1.0 | **Stable** | TBD | Current production version |
| v2.0 | **Planned** | N/A | Scheduled for Q2 2026 |

**Support Lifecycle:**
- **Current Version**: Full support, active development
- **Previous Version**: Security updates only for 12 months after next major release
- **Deprecated**: No support, migration required

---

## Version History

### Version 1.0 (Current) - January 9, 2026

**Status:** Stable Release

**Features:**
- ✅ User authentication (JWT with cookie-based storage)
- ✅ School management (CRUD operations)
- ✅ Student management (profiles, enrollment)
- ✅ Teacher management (profiles, class assignments)
- ✅ Class management (grades, sections)
- ✅ Attendance tracking (students and teachers)
- ✅ Announcements (school-wide communications)
- ✅ Chat rooms (encrypted messaging, SignalR)
- ✅ Video calls (WebRTC signaling, SignalR)
- ✅ Combined details (aggregated queries)
- ✅ Rate limiting middleware
- ✅ CORS configuration for frontend
- ✅ Role-based authorization
- ✅ Message encryption for chat
- ✅ Room access tokens for chat/video
- ✅ Video recording infrastructure

**Known Limitations:**
- No token refresh mechanism (planned for v1.1)
- Limited attendance reporting (enhanced in v1.2)
- No file upload support for announcements (planned for v1.3)
- No bulk operations for student/teacher management

**API Endpoints (v1.0):**

**Authentication:**
- POST `/api/Auth/register`
- POST `/api/Auth/login`
- POST `/api/Auth/logout`
- GET `/api/Auth/me`

**Schools:**
- GET `/api/School`
- GET `/api/School/{id}`
- POST `/api/School`
- PUT `/api/School/{id}`
- DELETE `/api/School/{id}`

**Students:**
- GET `/api/Student`
- GET `/api/Student/{id}`
- POST `/api/Student`
- PUT `/api/Student/{id}`
- DELETE `/api/Student/{id}`

**Teachers:**
- GET `/api/Teacher`
- GET `/api/Teacher/{id}`
- POST `/api/Teacher`
- PUT `/api/Teacher/{id}`
- DELETE `/api/Teacher/{id}`

**Classes:**
- GET `/api/Class`
- GET `/api/Class/{id}`
- POST `/api/Class`
- PUT `/api/Class/{id}`
- DELETE `/api/Class/{id}`

**Attendance:**
- GET `/api/Attendance`
- GET `/api/Attendance/{id}`
- POST `/api/Attendance`
- PUT `/api/Attendance/{id}`
- DELETE `/api/Attendance/{id}`

**Teacher Attendance:**
- GET `/api/TeacherAttendance`
- GET `/api/TeacherAttendance/{id}`
- POST `/api/TeacherAttendance`
- PUT `/api/TeacherAttendance/{id}`
- DELETE `/api/TeacherAttendance/{id}`

**Announcements:**
- GET `/api/Announcement`
- GET `/api/Announcement/{id}`
- POST `/api/Announcement`
- PUT `/api/Announcement/{id}`
- DELETE `/api/Announcement/{id}`

**Chat Rooms:**
- GET `/api/ChatRooms`
- POST `/api/ChatRooms`
- POST `/api/ChatRooms/join`
- DELETE `/api/ChatRooms/{id}`
- POST `/api/ChatRooms/recording/start`
- POST `/api/ChatRooms/recording/stop`
- GET `/api/ChatRooms/{roomId}/recordings`

**Combined Details:**
- Various aggregated query endpoints

**SignalR Hubs:**
- `/chatHub` - Real-time messaging
- `/videoCallHub` - WebRTC signaling

---

### Version 1.1 (Planned) - Q1 2026

**Status:** Development

**Planned Features:**
- 🔄 Token refresh mechanism
- 🔄 Password reset via email
- 🔄 Two-factor authentication (2FA)
- 🔄 Enhanced attendance reports
- 🔄 Bulk student/teacher import (CSV)

**New Endpoints:**
```
POST /api/Auth/refresh
POST /api/Auth/forgot-password
POST /api/Auth/reset-password
POST /api/Auth/enable-2fa
POST /api/Student/import
POST /api/Teacher/import
GET /api/Attendance/report/{classId}
```

**Breaking Changes:** None (backward compatible)

**Migration Required:** No

---

### Version 1.2 (Planned) - Q2 2026

**Status:** Planning

**Planned Features:**
- 🔄 File uploads for announcements
- 🔄 Student grades and transcripts
- 🔄 Assignment management
- 🔄 Exam scheduling
- 🔄 Parent portal integration

**New Endpoints:**
```
POST /api/Announcement/{id}/upload
GET /api/Grades
POST /api/Grades
PUT /api/Grades/{id}
DELETE /api/Grades/{id}
GET /api/Assignments
POST /api/Assignments
GET /api/Exams
POST /api/Exams
```

**Breaking Changes:** None (backward compatible)

---

### Version 2.0 (Planned) - Q3 2026

**Status:** Design Phase

**Major Changes:**
- 🔄 **URL versioning** (`/api/v2/...`)
- 🔄 **Pagination** for all list endpoints
- 🔄 **GraphQL** support (optional)
- 🔄 **Webhook** notifications
- 🔄 **API rate limiting** per user/role
- 🔄 **Enhanced search** with filters
- 🔄 **Multi-tenancy** support

**Breaking Changes:**
- Pagination required for all GET list endpoints
- Response structure standardization
- Changed authentication flow (refresh tokens mandatory)

**Migration Guide:** See [v1 to v2 Migration Guide](#v1-to-v2-migration-guide)

---

## v1 to v2 Migration Guide

**Note:** This is a preliminary guide. Final details will be available closer to v2.0 release.

### Breaking Changes in v2.0

#### 1. URL Versioning

**v1 (Current):**
```
GET /api/Students
```

**v2 (Planned):**
```
GET /api/v2/Students
```

**Migration:**
- Update all API calls to include `/v2/` in URL
- v1 endpoints will continue to work for 12 months after v2 release

#### 2. Pagination Required

**v1 (Current):**
```bash
GET /api/Students
# Returns all students (no pagination)
```

**v2 (Planned):**
```bash
GET /api/v2/Students?page=1&pageSize=20
```

**Response Structure (v2):**
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalRecords": 95
  }
}
```

**Migration:**
- Add pagination parameters to all list requests
- Handle paginated responses in client code

#### 3. Standardized Response Format

**v1 (Current):**
```json
{
  "isSuccess": true,
  "data": {...},
  "errorMessage": null
}
```

**v2 (Planned):**
```json
{
  "success": true,
  "data": {...},
  "error": null,
  "metadata": {
    "timestamp": "2026-03-15T10:00:00Z",
    "requestId": "uuid"
  }
}
```

**Migration:**
- Update response parsing logic
- Change `isSuccess` to `success`
- Change `errorMessage` to `error`

#### 4. Token Refresh Required

**v1 (Current):**
- No refresh token
- Re-authenticate when token expires

**v2 (Planned):**
- Refresh token returned on login
- Use refresh token to get new access token

**Migration:**
```javascript
// v1
async function login() {
  await fetch('/api/Auth/login', {...});
}

// v2
async function login() {
  const response = await fetch('/api/v2/Auth/login', {...});
  const { accessToken, refreshToken } = await response.json();
  
  // Store both tokens
  saveTokens(accessToken, refreshToken);
}

async function refreshAccessToken() {
  const refreshToken = getRefreshToken();
  const response = await fetch('/api/v2/Auth/refresh', {
    method: 'POST',
    body: JSON.stringify({ refreshToken })
  });
  
  const { accessToken } = await response.json();
  saveAccessToken(accessToken);
}
```

---

## Breaking Changes

### v1.0 Breaking Changes

**Initial Release:** No breaking changes from previous versions (first stable release)

### Planned v2.0 Breaking Changes

See [v1 to v2 Migration Guide](#v1-to-v2-migration-guide) above.

---

## Deprecations

### Current Deprecations (v1.0)

**None** - All endpoints are actively supported.

### Planned Deprecations (v1.1+)

| Endpoint/Feature | Deprecated In | Removed In | Replacement |
|------------------|---------------|------------|-------------|
| Cookie-only auth | v1.1 | v2.0 | Header-based with refresh tokens |
| Non-paginated lists | v1.2 | v2.0 | Paginated endpoints |

### Deprecation Policy

When a feature is deprecated:

1. **Announcement:** Marked as deprecated in documentation
2. **Warning:** API returns deprecation warning header
   ```
   X-Deprecated: true
   X-Deprecated-Message: This endpoint is deprecated. Use /api/v2/... instead
   X-Sunset: 2026-12-31
   ```
3. **Transition Period:** Minimum 6 months notice
4. **Removal:** Feature removed in next major version

---

## Backward Compatibility

### v1.x Compatibility Promise

**Within v1.x releases:**
- ✅ All existing endpoints remain functional
- ✅ Request/response formats unchanged
- ✅ New features added without breaking existing functionality
- ✅ Bug fixes do not alter expected behavior

**Client Recommendations:**
- Version lock dependencies to specific v1.x version
- Test against new minor/patch releases before production deployment
- Monitor deprecation warnings
- Subscribe to API changelog notifications

### Testing Backward Compatibility

```bash
# Run integration tests against new version
npm run test:api -- --version=1.1

# Compare responses
diff <(curl /api/Students) <(curl /api/v1.1/Students)
```

---

## Change Log Details

### January 9, 2026 - v1.0.0

**Initial stable release**

**Added:**
- Complete REST API for school management
- SignalR real-time features (chat, video)
- JWT authentication with cookie storage
- Role-based authorization
- Message encryption
- Room access tokens
- Video recording infrastructure
- Rate limiting middleware

**Fixed:**
- N/A (initial release)

**Security:**
- Implemented BCrypt password hashing for room passwords
- Added flood protection for chat (30 messages/min)
- XSS protection via input sanitization
- CSRF protection with SameSite cookies

---

## Reporting Issues

Found a bug or unexpected behavior?

1. **Check Known Issues:** Review this changelog and [GitHub Issues](https://github.com/your-repo/issues)
2. **Verify Version:** Ensure you're using the expected API version
3. **Report:** Submit detailed bug report with:
   - API version
   - Endpoint and HTTP method
   - Request payload
   - Expected vs actual response
   - Steps to reproduce

---

## Staying Updated

**Subscribe to Updates:**
- GitHub Releases: Watch repository for new releases
- Email Notifications: Subscribe to API changelog mailing list
- RSS Feed: `/api/changelog.xml` (planned)

**Testing Pre-release Versions:**
- Beta versions available at `/api/beta/...`
- Staging environment: `https://staging-api.yourschool.com`

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Next Review:** February 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Authentication](./AUTHENTICATION.md)
- [API Reference](./API_REFERENCE.md)
