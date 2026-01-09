# School Management System Documentation

Welcome to the SMS (School Management System) documentation. This directory contains all project documentation organized by topic.

---

## 📁 Documentation Structure

### � [API Documentation](api/)
Core API reference and fundamentals:
- [Getting Started Guide](api/GETTING_STARTED.md) - Complete quickstart for administrators, students, and teachers
- [Authentication & Authorization](api/AUTHENTICATION.md) - JWT authentication, roles, and security
- [Error Handling Reference](api/ERROR_HANDLING.md) - HTTP status codes, error responses, and troubleshooting
- [API Changelog & Versioning](api/API_CHANGELOG.md) - Version history, breaking changes, and migration guides

### 🔧 [Integration Guides](guides/)
Client libraries and integration examples:
- [Code Samples Library](guides/CODE_SAMPLES.md) - Multi-language examples (JavaScript, Python, C#, cURL)
- [SDK & Client Libraries](guides/SDK_GUIDE.md) - C#, TypeScript, and Python client documentation
- [External Partner Integration](guides/PARTNER_INTEGRATION.md) - Third-party integrations, parent portals, mobile apps

### 🧪 [Testing](testing/)
Testing tools and workflows:
- [Postman Collection Guide](testing/POSTMAN_GUIDE.md) - API testing with Postman, collection runner, Newman CLI

### 📡 [Real-time Features](signalr/)
SignalR implementation for chat and video:
- [Implementation Summary](signalr/IMPLEMENTATION_SUMMARY.md) - Overview of SignalR setup
- [Quick Reference](signalr/QUICK_REFERENCE.md) - Quick reference guide
- [Security & Persistence](signalr/SECURITY_AND_PERSISTENCE.md) - Security implementation
- [Fixes & Testing](signalr/FIXES_AND_TESTING.md) - Troubleshooting guide
- [Test Interface](signalr/test.html) - SignalR testing HTML

### 📝 [OpenAPI/Swagger](openapi/)
OpenAPI schema and Swagger documentation:
- [OpenAPI Schema Guide](openapi/OPENAPI_SCHEMA_GUIDE.md) - Swagger/OpenAPI documentation

### 🔌 [Model Context Protocol](mcp/)
MCP server integration:
- [MCP Overview](mcp/README.md) - Model Context Protocol introduction
- [Installation Guide](mcp/INSTALL.md) - Step-by-step setup
- [Installed Servers](mcp/MCP_SERVERS_INSTALLED.md) - Current configuration

---

## 🏗️ Project Architecture

### Backend (.NET 9)
- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Real-time**: SignalR

### Frontend (React)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **UI Library**: Shadcn/ui with Tailwind CSS
- **State Management**: React Query
- **Routing**: React Router v6

### Real-time Communication
- **Protocol**: SignalR with WebSockets
- **Hub**: ChatHub for messaging
- **Authentication**: JWT via query string and cookies

---

## 🚀 Quick Start

1. **Backend Setup**
   ```powershell
   cd Backend
   dotnet restore
   dotnet ef database update
   dotnet run --project SMSPrototype1
   ```

2. **Frontend Setup**
   ```powershell
   cd Frontend
   npm install
   npm run dev
   ```

3. **Access Application**
   - Frontend: http://localhost:5173
   - Backend API: https://localhost:7266
   - Swagger: https://localhost:7266/swagger

---

## 📚 Additional Resources

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [React Documentation](https://react.dev/)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [MCP Documentation](https://modelcontextprotocol.io/)

---

**Last Updated**: January 8, 2026  
**Project Version**: 2.0  
**Documentation Maintainer**: Development Team
