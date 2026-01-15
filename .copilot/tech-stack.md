# Technology Stack & Dependencies
**School Management System - Complete Tech Inventory**

## Backend Stack (.NET 9.0)

### Core Framework
- **.NET Runtime:** 9.0.0
- **Language:** C# 13
- **API Pattern:** RESTful + SignalR (WebSockets)
- **Architecture:** Clean Architecture (Repository + Service patterns)

### Database
- **RDBMS:** SQL Server 2022
- **ORM:** Entity Framework Core 9.0.1
- **Migration Tool:** EF Core CLI (`dotnet ef`)
- **Connection Pool:** Default ADO.NET pooling

### Authentication & Authorization
- **Authentication:** JWT Bearer Tokens
- **Token Library:** `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Identity:** ASP.NET Core Identity
- **Password Hashing:** PBKDF2 (built-in Identity)
- **Token Expiry:** 60 minutes (access), 7 days (refresh)

### Key NuGet Packages
```xml
<!-- Core Framework -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.1" />

<!-- Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />

<!-- Real-time Communication -->
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="9.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### Logging
- **Framework:** Serilog
- **Sinks:** Console, File (logs/ directory)
- **Format:** Structured JSON logging
- **Levels:** Debug, Information, Warning, Error, Fatal

---

## Frontend Stack (React 18)

### Core Framework
- **Framework:** React 18.3.1
- **Language:** TypeScript 5.5.3
- **Build Tool:** Vite 5.4.1
- **Package Manager:** Bun (alternative: npm/yarn)
- **Node Version:** 18+ required

### UI Components
- **Component Library:** Radix UI (headless components)
- **Styling:** Tailwind CSS 3.4.11
- **Icons:** Lucide React 0.462.0
- **Themes:** next-themes 0.3.0 (dark/light mode)

### State Management
- **Data Fetching:** TanStack Query (React Query) 5.56.2
- **Forms:** React Hook Form 7.62.0
- **Form Validation:** Zod 3.23.8 + @hookform/resolvers 3.9.0
- **Context:** React Context API (built-in)

### Routing & Navigation
- **Router:** React Router DOM 6.26.2
- **Protected Routes:** Custom ProtectedRoute wrapper
- **Route Guards:** JWT token validation

### Real-time Communication
- **SignalR Client:** @microsoft/signalr 9.0.6
- **WebSocket Support:** Built-in browser WebSocket API

### UI/UX Libraries
```json
"dependencies": {
  // Core
  "react": "^18.3.1",
  "react-dom": "^18.3.1",
  "react-router-dom": "^6.26.2",
  
  // State Management
  "@tanstack/react-query": "^5.56.2",
  "react-hook-form": "^7.62.0",
  "zod": "^3.23.8",
  
  // UI Components (Radix)
  "@radix-ui/react-dialog": "^1.1.2",
  "@radix-ui/react-dropdown-menu": "^2.1.1",
  "@radix-ui/react-select": "^2.1.1",
  "@radix-ui/react-tabs": "^1.1.0",
  "@radix-ui/react-toast": "^1.2.1",
  
  // Styling
  "tailwindcss": "^3.4.11",
  "tailwindcss-animate": "^1.0.7",
  "class-variance-authority": "^0.7.1",
  "clsx": "^2.1.1",
  "tailwind-merge": "^2.5.2",
  
  // Utilities
  "date-fns": "^3.6.0",
  "lucide-react": "^0.462.0",
  "sonner": "^1.5.0"
}
```

### Development Tools
- **Linter:** ESLint 9.9.0
- **Type Checker:** TypeScript compiler
- **Hot Reload:** Vite HMR (Hot Module Replacement)
- **Code Formatter:** Prettier (recommended)

---

## Database Schema

### Database Engine
- **Server:** SQL Server 2022 (Developer/Standard Edition)
- **Collation:** SQL_Latin1_General_CP1_CI_AS
- **Compatibility Level:** 160 (SQL Server 2022)

### Key Tables (35+ total)
```sql
-- Core Multi-Tenancy
Schools
AspNetUsers (with SchoolId FK)

-- Academic
Students, Teachers, Classes, Subjects
Attendance, Grades, Assignments

-- Administrative
Exams, ExamResults, FeeStructures, Payments
Timetables, Holidays, Announcements

-- Communication
ChatRooms, ChatMessages, Notifications

-- Security
AspNetRoles, AspNetUserRoles, AspNetUserClaims
AuditLogs
```

### Indexing Strategy
- **Primary Keys:** Clustered indexes (Guid Id)
- **Foreign Keys:** Non-clustered indexes (all FKs)
- **SchoolId:** Non-clustered indexes on ALL tables
- **Performance:** Composite indexes on (SchoolId + frequently queried columns)

---

## Infrastructure & DevOps

### Cloud Platform (Azure)
- **App Service:** Linux container (P1v2 tier for production)
- **Database:** Azure SQL Database (S2 tier: 50 DTUs)
- **Storage:** Azure Blob Storage (profile pictures, documents)
- **CDN:** Azure CDN (static assets)
- **Monitoring:** Application Insights

### Containerization
- **Runtime:** Docker 24+
- **Base Images:**
  - Backend: `mcr.microsoft.com/dotnet/aspnet:9.0`
  - Frontend: `nginx:alpine`
- **Orchestration:** Docker Compose (dev), Azure App Service (prod)

### CI/CD Pipeline
- **Platform:** GitHub Actions
- **Workflows:**
  - Build & Test (on PR)
  - Deploy to Staging (on merge to develop)
  - Deploy to Production (on merge to main)
- **Secrets Management:** GitHub Secrets + Azure Key Vault

### Monitoring & Logging
- **APM:** Application Insights (Azure)
- **Logs:** Serilog → File + Console
- **Metrics:** Custom metrics (API response time, active users)
- **Alerts:** Azure Monitor alerts (error rate, downtime)

---

## Third-Party Integrations (Planned)

### Payment Gateway
- **Provider:** Razorpay
- **SDK:** Razorpay .NET SDK
- **Features:** UPI, Cards, Netbanking, Wallets

### SMS Service
- **Provider:** Twilio
- **SDK:** Twilio .NET SDK
- **Use Cases:** OTP, notifications, attendance alerts

### Email Service
- **Provider:** SendGrid
- **SDK:** SendGrid .NET SDK
- **Use Cases:** Welcome emails, password reset, reports

### File Storage
- **Provider:** Azure Blob Storage
- **SDK:** Azure.Storage.Blobs
- **Use Cases:** Student photos, assignment uploads, reports

---

## Development Environment

### Required Software
- **Backend:**
  - .NET 9.0 SDK
  - Visual Studio 2022 / VS Code + C# extension
  - SQL Server 2022 (LocalDB or full instance)
  
- **Frontend:**
  - Node.js 18+ (or Bun runtime)
  - VS Code + ESLint + TypeScript extensions
  
- **Database Tools:**
  - SQL Server Management Studio (SSMS)
  - Azure Data Studio (alternative)
  - `sqlcmd` CLI tool

### Development URLs
- **Backend API:** https://localhost:7266
- **Frontend App:** http://localhost:5173
- **Swagger UI:** https://localhost:7266/swagger
- **SignalR Hub:** wss://localhost:7266/chathub

### Environment Variables
```bash
# Backend (appsettings.Development.json)
ConnectionStrings__DefaultConnection="Server=localhost;Database=SMSDatabase;Trusted_Connection=True;"
JwtSettings__Secret="your-256-bit-secret-key"
JwtSettings__Issuer="https://localhost:7266"
JwtSettings__Audience="https://localhost:5173"

# Frontend (.env.development)
VITE_API_BASE_URL="https://localhost:7266"
VITE_SIGNALR_HUB_URL="https://localhost:7266/chathub"
```

---

## Security Stack

### Encryption
- **In-Transit:** TLS 1.2+ (HTTPS everywhere)
- **At-Rest:** SQL Server Transparent Data Encryption (TDE)
- **Passwords:** ASP.NET Identity PBKDF2 hashing

### CORS Policy
```csharp
// Allowed Origins (Development)
http://localhost:5173
http://localhost:3000

// Allowed Origins (Production)
https://yourschool.example.com
```

### Rate Limiting
- **Strategy:** Token bucket algorithm
- **Limits:** 100 requests/minute per IP
- **Library:** AspNetCoreRateLimit (planned)

### API Security
- **Authentication:** JWT Bearer tokens (required on all endpoints except /auth)
- **Authorization:** Role-based + policy-based
- **CSRF Protection:** SameSite cookies + CORS validation
- **XSS Protection:** Content Security Policy headers

---

## Testing Stack (Planned)

### Backend Testing
- **Framework:** xUnit
- **Mocking:** Moq
- **Integration Tests:** WebApplicationFactory
- **Code Coverage:** Coverlet

### Frontend Testing
- **Framework:** Vitest (Vite-native)
- **Component Testing:** React Testing Library
- **E2E Testing:** Playwright (planned)

### Load Testing
- **Tool:** k6 (load testing scripts exist in `/Backend/performance-tests/`)
- **Scenarios:** Auth, CRUD operations, SignalR chat

---

## Version Control & Collaboration

### Git Workflow
- **Branching Strategy:** GitFlow
  - `main` - Production releases
  - `develop` - Integration branch
  - `feature/*` - Feature branches
  - `hotfix/*` - Production fixes

### Code Quality
- **Backend:** .editorconfig, StyleCop (optional)
- **Frontend:** ESLint, Prettier
- **Pre-commit Hooks:** Husky (recommended)

---

## Browser Support

### Minimum Requirements
- **Chrome:** 90+
- **Firefox:** 88+
- **Safari:** 14+
- **Edge:** 90+
- **Mobile:** iOS Safari 14+, Chrome Android 90+

---

## Performance Targets

### API Response Times
- **p50 (median):** < 100ms
- **p95:** < 200ms
- **p99:** < 500ms

### Frontend Metrics
- **First Contentful Paint (FCP):** < 1.5s
- **Time to Interactive (TTI):** < 3.5s
- **Lighthouse Score:** 90+ (Performance)

### Database
- **Query Time:** < 50ms (simple), < 200ms (complex joins)
- **Connection Pool:** 100 max connections
- **Index Hit Ratio:** > 95%
