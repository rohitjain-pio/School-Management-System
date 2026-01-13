# System Overview
## School Management System - Architecture at a Glance

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 15 minutes  
**Status:** ✅ Current

---

## 🎯 Purpose

This document provides a high-level technical overview of the School Management System's architecture, key components, and technology decisions. It serves as the entry point for developers, architects, and technical stakeholders to understand how the system works.

---

## 🏗️ Architecture Pattern

### **Multi-Tenant SaaS Architecture**

**Pattern Type:** Shared Database, Shared Schema (Discriminator-Based Isolation)

```
┌─────────────────────────────────────────────────────────┐
│                    INTERNET                              │
└────────────────────┬────────────────────────────────────┘
                     │ HTTPS (TLS 1.3)
         ┌───────────▼───────────┐
         │  Cloud Load Balancer   │
         │  • SSL Termination     │
         │  • Rate Limiting       │
         └───────────┬───────────┘
                     │
         ┌───────────▼───────────────────────┐
         │    API Gateway / Reverse Proxy     │
         │    • Request Routing               │
         │    • CORS Handling                 │
         └───────────┬───────────────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
┌─────────┐   ┌─────────┐   ┌─────────┐
│ Web App │   │ Web App │   │ Web App │
│Instance1│   │Instance2│   │Instance3│
└────┬────┘   └────┬────┘   └────┬────┘
     │             │             │
     └─────────────┼─────────────┘
                   │
    ┌──────────────┼──────────────┐
    │              │              │
    ▼              ▼              ▼
┌─────────┐   ┌─────────┐   ┌─────────┐
│  Redis  │   │SQL Server│  │ Cloud   │
│ Cache   │   │Database │   │ Storage │
└─────────┘   └─────────┘   └─────────┘
```

**Why This Pattern?**

✅ **Cost-Effective:** All schools share infrastructure  
✅ **Easy Scaling:** Add new schools without new databases  
✅ **Centralized Management:** SuperAdmin controls everything  
✅ **Simple Maintenance:** One codebase, one database schema  
⚠️ **Requires Strong Isolation:** SchoolId validation critical  

---

## 🧱 System Components

### **1. Frontend Layer**

**Technology:** React 18 + TypeScript + Vite  
**State Management:** TanStack Query (React Query)  
**UI Framework:** Tailwind CSS + Shadcn/ui  
**Real-Time:** SignalR Client

**Key Features:**
- Single Page Application (SPA)
- Role-based UI rendering
- Optimistic updates
- Real-time notifications via WebSocket
- Responsive design (mobile-first)

**Build Output:**
- Bundled JavaScript (code-split by route)
- Static assets (images, fonts)
- Served via Nginx in Docker container
- CDN distribution for static files

### **2. Backend Layer**

**Technology:** .NET 9.0 (ASP.NET Core Web API)  
**Framework Pattern:** Clean Architecture  
**Authentication:** ASP.NET Core Identity + JWT  
**Real-Time:** SignalR Server

**Project Structure:**
```
Backend/
├── SMSPrototype1/          # API Controllers, Middleware, Program.cs
├── SMSServices/            # Business Logic Layer
├── SMSRepository/          # Data Access Layer
├── SMSDataContext/         # Entity Framework DbContext
└── SMSDataModel/           # Entity Models, DTOs
```

**Key Components:**
- RESTful API endpoints (11 controllers)
- JWT authentication with refresh tokens
- SignalR hubs for real-time features
- Middleware pipeline for security
- Background services (notifications, cleanup)

### **3. Data Layer**

**Primary Database:** SQL Server 2022  
**Caching Layer:** Redis 7.x  
**File Storage:** Azure Blob Storage / AWS S3

**Database Design:**
- 40+ tables (users, schools, students, teachers, classes, etc.)
- SchoolId discriminator on multi-tenant tables
- Indexes optimized for school-filtered queries
- Audit log table for compliance

**Caching Strategy:**
- User session data (JWT blacklist)
- Frequently accessed school data
- API rate limiting counters
- SignalR connection mappings

### **4. Security Layer**

**Authentication Flow:**
```
1. User logs in → POST /api/Auth/login
2. Backend validates credentials (BCrypt)
3. Generate JWT access token (3-hour expiry)
4. Generate refresh token (30-day expiry)
5. Store refresh token in database
6. Return access token + httpOnly cookie with refresh token
7. Client includes JWT in Authorization: Bearer header
8. Middleware validates JWT on each request
9. Extract SchoolId claim from token
10. All queries filtered by SchoolId
```

**Authorization:**
- Claims-based (SchoolId, Role, UserId)
- Policy-based authorization
- Resource-based authorization for ownership checks

---

## 🔄 Data Flow Examples

### **Example 1: Student Creation**

```
1. Teacher clicks "Add Student" in UI
   ↓
2. Frontend sends POST /api/Student
   Body: { firstName, lastName, classId, ... }
   Headers: Authorization: Bearer <JWT>
   ↓
3. SchoolIsolationMiddleware validates:
   - JWT valid?
   - SchoolId claim present?
   - User role allowed?
   ↓
4. StudentController.CreateStudentAsync():
   - Extract SchoolId from User.Claims
   - Validate ClassId belongs to user's school
   - Call StudentService.CreateStudentAsync()
   ↓
5. StudentService (Business Logic):
   - Validate business rules (age, grade level)
   - Generate student ID number
   - Call StudentRepository.AddAsync()
   ↓
6. StudentRepository (Data Access):
   - Map DTO to Entity
   - Set Student.SchoolId = user's SchoolId
   - dbContext.Students.Add(student)
   - dbContext.SaveChangesAsync()
   ↓
7. Database writes record:
   Students table: { Id, SchoolId, FirstName, ... }
   ↓
8. Response returned:
   201 Created with student object
   ↓
9. Frontend updates UI (TanStack Query invalidates cache)
```

### **Example 2: Cross-School Access Attempt (Blocked)**

```
1. Malicious user modifies JWT to change SchoolId claim
   ↓
2. Sends GET /api/Student?schoolId=OTHER_SCHOOL_ID
   ↓
3. SchoolIsolationMiddleware detects:
   - JWT signature invalid (claim was modified)
   - Returns 401 Unauthorized
   - Logs security incident
   ↓
4. Request never reaches controller
   ↓
5. User sees error, account flagged for review
```

---

## 🎭 User Roles & Permissions

### **Role Hierarchy**

```
SuperAdmin (Platform Owner)
    ↓
Admin (School Principal/Manager)
    ↓
Teacher (Faculty)
    ↓
Student (Learner)
    ↓
Parent (Guardian)
```

### **Permission Matrix**

| Action | SuperAdmin | Admin | Teacher | Student | Parent |
|--------|------------|-------|---------|---------|--------|
| **Create School** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Delete School** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Access Any School** | ✅* | ❌ | ❌ | ❌ | ❌ |
| **Manage Teachers** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Manage Students** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Manage Classes** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Mark Attendance** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Enter Grades** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **View Own Grades** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **View Child Grades** | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Send Announcements** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Chat with Teachers** | ✅ | ✅ | ✅ | ✅ | ✅ |

*SuperAdmin access is logged in audit trail

---

## 🚀 Deployment Architecture

### **Container Strategy**

**Docker Containers:**
1. **frontend-app** (Nginx + React build)
2. **backend-api** (.NET 9 Web API)
3. **postgres-db** (SQL Server 2022)
4. **redis-cache** (Redis 7)

**Docker Compose (Development):**
```yaml
version: '3.8'
services:
  frontend:
    build: ./Frontend
    ports: ["80:80"]
    
  backend:
    build: ./Backend
    ports: ["7266:8080"]
    depends_on: [database, redis]
    
  database:
    image: mcr.microsoft.com/mssql/server:2022
    
  redis:
    image: redis:7-alpine
```

### **Production Deployment (Cloud)**

**Option A: Azure**
- Azure App Service (Backend)
- Azure Static Web Apps (Frontend)
- Azure SQL Database (Managed)
- Azure Cache for Redis
- Azure Blob Storage (Files)
- Application Insights (Monitoring)

**Option B: AWS**
- Elastic Beanstalk (Backend)
- S3 + CloudFront (Frontend)
- RDS SQL Server (Managed)
- ElastiCache Redis
- S3 (File Storage)
- CloudWatch (Monitoring)

**CI/CD Pipeline:**
```
Push to GitHub main branch
    ↓
GitHub Actions Workflow Triggered
    ↓
1. Run unit tests
2. Run integration tests
3. Build Docker images
4. Push images to registry
5. Deploy to staging
6. Run smoke tests
7. Deploy to production (if tests pass)
8. Send deployment notification
```

---

## 📊 Performance Targets

### **Response Time Goals**

| Operation | Target | Maximum |
|-----------|--------|---------|
| **API Response (P95)** | < 200ms | 500ms |
| **Dashboard Load** | < 1s | 2s |
| **Student List (500 items)** | < 300ms | 1s |
| **File Upload (10MB)** | < 5s | 15s |
| **Report Generation** | < 3s | 10s |

### **Scalability Targets**

| Metric | Current | Month 1 | Month 6 | Year 1 |
|--------|---------|---------|---------|--------|
| **Schools** | 0 | 5 | 20 | 50 |
| **Concurrent Users** | 0 | 50 | 200 | 500 |
| **API Requests/sec** | 0 | 10 | 50 | 100 |
| **Database Size** | 1GB | 5GB | 20GB | 50GB |

---

## 🔧 Development Environment

### **Required Tools**

**Backend:**
- Visual Studio 2022 / Rider
- .NET 9.0 SDK
- SQL Server 2022 (local or Docker)
- Redis (Docker)

**Frontend:**
- VS Code
- Node.js 20+
- Bun package manager
- React DevTools extension

**DevOps:**
- Docker Desktop
- Git
- Postman/Insomnia (API testing)

### **Getting Started**

```bash
# Clone repository
git clone <repo-url>

# Backend setup
cd Backend
dotnet restore
dotnet ef database update
dotnet run --project SMSPrototype1

# Frontend setup (new terminal)
cd Frontend
bun install
bun run dev

# Access application
# Frontend: http://localhost:5173
# Backend:  https://localhost:7266
```

---

## 📚 Related Documents

**Next Steps:**
- [02_MULTI_TENANCY_DESIGN.md](./02_MULTI_TENANCY_DESIGN.md) ⭐ CRITICAL
- [03_SECURITY_ARCHITECTURE.md](./03_SECURITY_ARCHITECTURE.md) ⭐ CRITICAL
- [04_DATABASE_SCHEMA.md](./04_DATABASE_SCHEMA.md)

**Reference:**
- [00_EXECUTIVE_SUMMARY.md](./00_EXECUTIVE_SUMMARY.md) - Business overview
- [README.md](./README.md) - Master navigation

---

**Document Status:** ✅ Complete  
**Last Review:** January 13, 2026  
**Next Review:** February 1, 2026
