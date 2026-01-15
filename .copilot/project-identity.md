# Project Identity
**School Management System - Multi-Tenant SaaS Platform**

## Who We Are
- **Project Name:** School Management System (SMS)
- **Type:** Multi-tenant B2B SaaS platform
- **Target Market:** K-12 schools in India
- **Launch Date:** February 13, 2026 (28 days remaining)
- **Current Date:** January 15, 2026
- **Stage:** MVP Development (Week 1, Day 3)

## What We're Building
A secure, scalable school management platform where ONE SuperAdmin (you) can manage MULTIPLE schools from a single dashboard. Each school's data is completely isolated using SchoolId discriminator pattern.

## Core Value Proposition
1. **True Multi-Tenancy** - One platform, infinite schools, zero data leakage
2. **SuperAdmin Control** - Manage all schools from central dashboard
3. **Indian Compliance** - DPDP Act 2023 compliant
4. **Cost-Effective** - Shared infrastructure = lower prices
5. **Rapid Onboarding** - New school setup in < 1 hour

## Technology Stack

### Backend
- **Framework:** .NET 9.0 with C#
- **Database:** SQL Server 2022
- **ORM:** Entity Framework Core 9.0
- **API:** RESTful + SignalR (real-time chat)
- **Auth:** JWT Bearer tokens with role-based claims
- **Logging:** Serilog with structured logging

### Frontend
- **Framework:** React 18.3.1 with TypeScript
- **Build Tool:** Vite 5.4.1
- **UI Library:** Radix UI + Tailwind CSS
- **State:** React Query (TanStack Query)
- **Routing:** React Router v6
- **Real-time:** SignalR client (@microsoft/signalr)

### Infrastructure
- **Cloud:** Azure (App Service, SQL Database, Blob Storage)
- **CI/CD:** GitHub Actions
- **Monitoring:** Application Insights
- **Deployment:** Docker containers

## Project Structure
```
School-Management-System/
├── Backend/
│   ├── SMSPrototype1/          # Main API project
│   ├── SMSDataContext/         # EF Core DbContext
│   ├── SMSDataModel/           # Entity models
│   ├── SMSRepository/          # Data access layer
│   └── SMSServices/            # Business logic + SignalR hubs
├── Frontend/                    # React SPA
├── docs/                        # Comprehensive documentation
└── .copilot/                    # AI context system (this folder)
```

## Key Roles
- **SuperAdmin** - You (manages all schools, silent access)
- **SchoolAdmin** - School principal/admin (manages their school)
- **Teacher** - Teaching staff (grades, attendance, chat)
- **Student** - Students (view grades, assignments, chat)
- **Parent** - Parents (view child's data, communicate)

## Critical Success Factors
1. **Security First** - SchoolId isolation is non-negotiable
2. **Fast Response Times** - API < 200ms (p95)
3. **Zero Downtime** - 99.9% uptime SLA
4. **Data Privacy** - DPDP Act 2023 compliance
5. **Scalability** - Support 10,000+ students per school
