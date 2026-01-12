# Anti-Pattern Remediation Tracker
## SchoolSync School Management System

**Start Date:** January 12, 2026  
**Target Completion:** February 23, 2026 (6 weeks)  
**Status:** 🔴 Not Started

---

## Progress Overview

| Phase | Status | Start Date | End Date | Completion |
|-------|--------|-----------|----------|------------|
| Phase 1: Immediate Fixes | 🔴 Not Started | - | - | 0% |
| Phase 2: Pagination | 🔴 Not Started | - | - | 0% |
| Phase 3: Observability | 🔴 Not Started | - | - | 0% |
| Phase 4: Infrastructure | 🔴 Not Started | - | - | 0% |
| Phase 5: Code Quality | 🔴 Not Started | - | - | 0% |
| **Overall** | **🔴 0%** | - | - | **0/5 Phases** |

---

## Phase 1: Immediate Fixes (Week 1)

**Goal:** Fix production blockers and quick wins  
**Target Effort:** 13-21 hours  
**Status:** 🔴 Not Started

### Tasks

- [ ] **Fix hardcoded localhost URLs** (1-2h)
  - Files: `ErrorMonitorContext.tsx`, `geminiService.ts`
  - Use `import.meta.env.VITE_API_URL` instead
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Fix CombinedDetails bugs** (3-4h)
  - File: `CombinedDetailsRepository.cs`
  - Fix: `Students.Where(x=>x.Id==schoolId)` → `Students.Where(x=>x.SchoolId==schoolId)`
  - Fix: Implement present students/teachers counts
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Optimize dashboard queries** (4-6h)
  - File: `CombinedDetailsRepository.cs`
  - Consolidate 4 queries into 1
  - Use projection with single query
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add health check endpoints** (4-6h)
  - File: `Program.cs`
  - Add `/health`, `/health/ready`, `/health/live`
  - Install: `AspNetCore.HealthChecks.SqlServer`
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Parallelize sequential queries** (3-4h)
  - Files: Multiple services
  - Use `Task.WhenAll` for independent queries
  - **Assigned to:** _______________
  - **Completion date:** _______________

### Verification Checklist

- [ ] Frontend works with environment variables
- [ ] Dashboard shows correct data
- [ ] Dashboard loads in <1 second
- [ ] `/health` endpoint returns 200 OK
- [ ] Health checks show SQL Server status

---

## Phase 2: Pagination & Performance (Week 2)

**Goal:** Fix scalability issues  
**Target Effort:** 24-34 hours  
**Status:** 🔴 Not Started

### Tasks

- [ ] **Implement pagination infrastructure** (6-8h)
  - Create: `Backend/SMSDataModel/Model/PagedRequest.cs`
  - Create: `Backend/SMSDataModel/Model/PagedResponse.cs`
  - Add pagination extension methods
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add pagination to Student endpoint** (2-3h)
  - Files: `StudentController.cs`, `IStudentService.cs`, `StudentService.cs`
  - Update: `GET /api/Student` to accept `PagedRequest`
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add pagination to Teacher endpoint** (2-3h)
  - Files: `TeacherController.cs`, `ITeacherService.cs`, `TeacherService.cs`
  - Update: `GET /api/Teacher` to accept `PagedRequest`
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add pagination to Class endpoint** (2-3h)
  - Files: `ClassController.cs`, `ISchoolClassServices.cs`, `SchoolClassServices.cs`
  - Update: `GET /api/Class` to accept `PagedRequest`
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Update frontend hooks for pagination** (4-6h)
  - Files: `useStudents.tsx`, `useTeachers.tsx`, `useClasses.tsx`
  - Add pagination state and controls
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Implement attendance bulk endpoint** (6-8h)
  - Create: `POST /api/Attendance/class/{classId}/attendance`
  - Support bulk insert/update
  - Update: `Attendance.tsx` to use new endpoint
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Create list/detail DTOs** (4-5h)
  - Create: `StudentListItemDto.cs` (minimal fields)
  - Create: `StudentDetailDto.cs` (all fields)
  - Apply to other entities (Teacher, Class, etc.)
  - **Assigned to:** _______________
  - **Completion date:** _______________

### Verification Checklist

- [ ] Can load 10,000+ students without issues
- [ ] Page size limited to 100 records max
- [ ] Frontend shows pagination controls
- [ ] Data transfer reduced by 90%
- [ ] Attendance save functionality works

---

## Phase 3: Observability (Week 3)

**Goal:** Add logging and monitoring  
**Target Effort:** 32-48 hours  
**Status:** 🔴 Not Started

### Tasks

- [ ] **Install & configure Serilog** (4-6h)
  - Install: `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Serilog.Sinks.Console`
  - Configure in `Program.cs`
  - Add configuration in `appsettings.json`
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add logging to all services** (12-16h)
  - Files: All services (StudentService, TeacherService, etc.)
  - Log: Info, Warning, Error levels
  - Include context: StudentId, UserId, etc.
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Add correlation ID middleware** (2-3h)
  - Create: `Backend/SMSPrototype1/Middleware/CorrelationIdMiddleware.cs`
  - Add `X-Correlation-ID` header
  - Track requests across services
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Configure Application Insights** (6-8h)
  - Install: `Microsoft.ApplicationInsights.AspNetCore`
  - Configure in Azure Portal
  - Add instrumentation key
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Set up monitoring alerts** (8-12h)
  - Create alerts for: Error rate, Response time, Availability
  - Configure action groups (email, SMS)
  - Test alert notifications
  - **Assigned to:** _______________
  - **Completion date:** _______________

### Verification Checklist

- [ ] Logs written to `logs/` directory
- [ ] Structured JSON logs in Seq/App Insights
- [ ] All API requests have correlation IDs
- [ ] Application Insights dashboard shows telemetry
- [ ] Alerts trigger for test scenarios

---

## Phase 4: Infrastructure (Week 4)

**Goal:** Production-ready deployment  
**Target Effort:** 42-64 hours  
**Status:** 🔴 Not Started

### Tasks

- [ ] **Create Dockerfile** (4-6h)
  - Create: `Backend/Dockerfile`
  - Multi-stage build (build + runtime)
  - Non-root user for security
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Create docker-compose.yml** (2-3h)
  - Create: `docker-compose.yml`
  - Services: API + SQL Server
  - Health checks configured
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Configure database backups** (8-12h)
  - Azure SQL: Enable automated backups (30-day retention)
  - On-premise: Create maintenance plan
  - Test restore procedure
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Build CI/CD pipeline** (16-24h)
  - Create: `.github/workflows/deploy.yml`
  - Stages: Build → Test → Deploy (Staging) → Deploy (Prod)
  - Database migration automation
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Set up load balancing** (12-16h)
  - Azure App Service: Scale to 3 instances
  - OR Kubernetes: HPA with 2-10 replicas
  - Configure health checks
  - **Assigned to:** _______________
  - **Completion date:** _______________

### Verification Checklist

- [ ] Docker image builds successfully
- [ ] `docker-compose up` runs entire stack
- [ ] Database backup created and verified
- [ ] CI/CD pipeline deploys to staging
- [ ] Multiple API instances running
- [ ] Load balancer distributes traffic

---

## Phase 5: Code Quality (Weeks 5-6)

**Goal:** Clean architecture  
**Target Effort:** 40-56 hours  
**Status:** 🔴 Not Started

### Tasks

- [ ] **Split AuthController** (8-12h)
  - Create: `AuthenticationController.cs` (Login, Logout, GetMe)
  - Create: `RegistrationController.cs` (Register, Confirm)
  - Create: `TokenController.cs` (Refresh, Blacklist)
  - Create: `PasswordController.cs` (Reset, Change)
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Refactor Program.cs with extensions** (4-6h)
  - Create: `Backend/SMSPrototype1/Extensions/ServiceCollectionExtensions.cs`
  - Move service registrations to extension methods
  - Group by domain/layer
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Implement Result pattern** (16-20h)
  - Create: `Backend/SMSDataModel/Model/Result.cs`
  - Update all services to return `Result<T>`
  - Update controllers to handle Result
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Refactor ChatHub to use service** (8-12h)
  - Create: `Backend/SMSServices/Services/ChatService.cs`
  - Move business logic from Hub to Service
  - Hub calls service methods
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Move CORS to configuration** (2-3h)
  - Update: `appsettings.json` with CORS origins
  - Update: `Program.cs` to read from config
  - **Assigned to:** _______________
  - **Completion date:** _______________

- [ ] **Optimize AuthController login** (2-3h)
  - Parallelize: GetRoles + GenerateRefreshToken
  - Fire-and-forget: Audit log
  - **Assigned to:** _______________
  - **Completion date:** _______________

### Verification Checklist

- [ ] No controller has >200 lines
- [ ] No service has >300 lines
- [ ] All services return Result<T>
- [ ] Controllers handle errors consistently
- [ ] ChatHub only orchestrates, doesn't contain logic
- [ ] CORS configured via appsettings
- [ ] Login completes in <500ms (P95)

---

## Key Metrics Tracking

### Before Remediation (Baseline)

| Metric | Value | Date Measured |
|--------|-------|---------------|
| Dashboard Load Time | _____ seconds | __________ |
| API Response Time (P95) | _____ ms | __________ |
| Max Students in DB | _____ | __________ |
| Deployment Time | _____ hours | __________ |
| Uptime (last month) | _____ % | __________ |
| Error Rate | _____ % | __________ |

### After Each Phase

#### Phase 1 Metrics
| Metric | Target | Actual | Date |
|--------|--------|--------|------|
| Dashboard Load Time | <1s | _____ | _____ |
| Health Check Response | 200 OK | _____ | _____ |

#### Phase 2 Metrics
| Metric | Target | Actual | Date |
|--------|--------|--------|------|
| Max Students Supported | 10,000+ | _____ | _____ |
| Data Transfer Reduction | 90% | _____ | _____ |

#### Phase 3 Metrics
| Metric | Target | Actual | Date |
|--------|--------|--------|------|
| Logs Generated | Yes | _____ | _____ |
| Alert Triggered | Yes | _____ | _____ |

#### Phase 4 Metrics
| Metric | Target | Actual | Date |
|--------|--------|--------|------|
| Deployment Time | <10 min | _____ | _____ |
| Active Instances | 3 | _____ | _____ |
| Uptime | 99.9% | _____ | _____ |

#### Phase 5 Metrics
| Metric | Target | Actual | Date |
|--------|--------|--------|------|
| Largest File (lines) | <300 | _____ | _____ |
| API Response (P95) | <200ms | _____ | _____ |

---

## Issues & Blockers

| Date | Issue | Severity | Resolution | Resolved By | Date Resolved |
|------|-------|----------|-----------|-------------|---------------|
| | | | | | |
| | | | | | |
| | | | | | |

---

## Team Assignment

| Team Member | Role | Phases | Hours Allocated |
|-------------|------|--------|----------------|
| | Backend Dev | | |
| | Backend Dev | | |
| | Frontend Dev | | |
| | DevOps Eng | | |
| | QA/Tester | | |

---

## Weekly Status Updates

### Week 1 (Phase 1)
**Date:** __________  
**Completed:**
- [ ] 
- [ ] 

**In Progress:**
- [ ] 

**Blocked:**
- [ ] 

**Next Week:**
- [ ] 

---

### Week 2 (Phase 2)
**Date:** __________  
**Completed:**
- [ ] 

**In Progress:**
- [ ] 

**Blocked:**
- [ ] 

**Next Week:**
- [ ] 

---

### Week 3 (Phase 3)
**Date:** __________  
**Completed:**
- [ ] 

**In Progress:**
- [ ] 

**Blocked:**
- [ ] 

**Next Week:**
- [ ] 

---

### Week 4 (Phase 4)
**Date:** __________  
**Completed:**
- [ ] 

**In Progress:**
- [ ] 

**Blocked:**
- [ ] 

**Next Week:**
- [ ] 

---

### Weeks 5-6 (Phase 5)
**Date:** __________  
**Completed:**
- [ ] 

**In Progress:**
- [ ] 

**Blocked:**
- [ ] 

**Final Status:**
- [ ] 

---

## Sign-Off

### Phase 1 Approval
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______

### Phase 2 Approval
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______

### Phase 3 Approval
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______

### Phase 4 Approval
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______
- [ ] DevOps Lead: _____________ Date: _______

### Phase 5 Approval
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______

### Final Project Sign-Off
- [ ] Technical Lead: _____________ Date: _______
- [ ] Product Owner: _____________ Date: _______
- [ ] Stakeholders: _____________ Date: _______

---

**Document Version:** 1.0  
**Last Updated:** January 12, 2026  
**Next Review:** Weekly during remediation
