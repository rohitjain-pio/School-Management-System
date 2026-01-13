# Monitoring & Logging
## Observability Stack for Production

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 15 minutes  
**Status:** 🟡 Partially Implemented

---

## 🎯 **Observability Goals**

1. **Detect issues before users report them**
2. **Troubleshoot problems quickly (< 5 min to root cause)**
3. **Track performance trends over time**
4. **Comply with audit requirements**
5. **Optimize costs based on usage patterns**

---

## 📊 **The Four Pillars**

### **1. Logs** → What happened?
Event records with timestamps (errors, info, warnings)

### **2. Metrics** → How is the system performing?
Numeric measurements (CPU, memory, request count)

### **3. Traces** → Where is time spent?
Request journey through services

### **4. Alerts** → When should I be notified?
Automated notifications for critical issues

---

## 📝 **Logging with Serilog**

### **Configuration (Program.cs)**

```csharp
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SchoolManagementSystem")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        autoCreateSqlTable: true,
        restrictedToMinimumLevel: LogEventLevel.Warning) // Only warnings+ to DB
    .WriteTo.ApplicationInsights(
        builder.Configuration["ApplicationInsights:InstrumentationKey"],
        TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog();
```

### **Structured Logging Examples**

```csharp
// ❌ BAD: String interpolation (not searchable)
_logger.LogInformation($"User {userId} created student {studentId}");

// ✅ GOOD: Structured logging (searchable properties)
_logger.LogInformation(
    "User {UserId} created student {StudentId} in school {SchoolId}",
    userId, studentId, schoolId);

// Log levels
_logger.LogTrace("Detailed diagnostic info (development only)");
_logger.LogDebug("Debugging information");
_logger.LogInformation("General informational message");
_logger.LogWarning("Something unexpected but handled");
_logger.LogError(exception, "Error occurred processing request");
_logger.LogCritical("Critical failure, immediate attention required");

// Log with context
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["SchoolId"] = schoolId,
    ["CorrelationId"] = correlationId
}))
{
    _logger.LogInformation("Processing student creation");
    // All logs in this scope will include these properties
}
```

### **What to Log**

**DO Log:**
- ✅ Authentication attempts (success + failure)
- ✅ Authorization failures
- ✅ Data modifications (CRUD operations)
- ✅ SuperAdmin access (all actions)
- ✅ Payment transactions
- ✅ API errors (4xx, 5xx responses)
- ✅ Performance issues (slow queries > 1s)
- ✅ External API calls (3rd party services)

**DON'T Log:**
- ❌ Passwords or sensitive credentials
- ❌ Full credit card numbers
- ❌ Personal health information (unless encrypted)
- ❌ JWT tokens
- ❌ Excessive trace logs in production

---

## 📈 **Metrics with Application Insights**

### **Built-in Metrics**

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

**Automatic Tracking:**
- HTTP requests (count, duration, status codes)
- Dependencies (SQL queries, Redis calls, HTTP calls)
- Exceptions
- Page views (frontend)
- Custom events

### **Custom Metrics**

```csharp
public class MetricsService
{
    private readonly TelemetryClient _telemetry;
    
    public void TrackStudentCreated(Guid schoolId)
    {
        _telemetry.TrackEvent("StudentCreated", new Dictionary<string, string>
        {
            ["SchoolId"] = schoolId.ToString()
        });
        
        _telemetry.GetMetric("StudentCount", "SchoolId")
            .TrackValue(1, schoolId.ToString());
    }
    
    public void TrackLoginDuration(double milliseconds, bool success)
    {
        _telemetry.TrackMetric("LoginDuration", milliseconds, new Dictionary<string, string>
        {
            ["Success"] = success.ToString()
        });
    }
    
    public void TrackAttendanceMarked(int studentCount, Guid classId)
    {
        _telemetry.TrackEvent("AttendanceMarked", 
            properties: new Dictionary<string, string>
            {
                ["ClassId"] = classId.ToString()
            },
            metrics: new Dictionary<string, double>
            {
                ["StudentCount"] = studentCount
            });
    }
}
```

### **Key Metrics to Track**

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| **Request Rate** | Requests per second | > 100 (scale up) |
| **Response Time (P95)** | 95th percentile latency | > 500ms |
| **Error Rate** | Failed requests / total | > 1% |
| **CPU Usage** | App Service CPU | > 80% |
| **Memory Usage** | App Service memory | > 90% |
| **Database DTU** | Database utilization | > 80% |
| **Active Users** | Concurrent users | Monitor trends |
| **Cache Hit Rate** | Redis cache efficiency | < 70% (investigate) |
| **Failed Logins** | Authentication failures | > 50/hour (attack?) |

---

## 🔍 **Distributed Tracing**

### **Correlation IDs**

```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        // Add to response headers
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        // Add to logs
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Register
app.UseMiddleware<CorrelationMiddleware>();
```

**Tracing a Request:**

```
User Request → [Correlation-ID: abc-123]
    ↓
API Gateway → [Correlation-ID: abc-123]
    ↓
AuthController → [Correlation-ID: abc-123]
    ↓
StudentService → [Correlation-ID: abc-123]
    ↓
Database Query → [Correlation-ID: abc-123]
    ↓
Response → [Correlation-ID: abc-123]
```

**All logs for this request have the same Correlation-ID:**

```kusto
// Application Insights query
traces
| where customDimensions.CorrelationId == "abc-123"
| project timestamp, message, severityLevel
| order by timestamp asc
```

---

## 🚨 **Alerting Strategy**

### **Alert Levels**

**P0 - Critical (Immediate Response):**
- Production system down (health check fails)
- Database unavailable
- Error rate > 5%
- Payment processing failures

**P1 - High (15-minute Response):**
- Error rate > 1%
- Response time > 1s (P95)
- CPU > 90% for 10 minutes
- Failed login rate spike (attack?)

**P2 - Medium (1-hour Response):**
- Memory usage > 80%
- Cache hit rate < 60%
- Disk space < 20%

**P3 - Low (Next Day):**
- Info-level anomalies
- Performance degradation trends
- Backup warnings

### **Application Insights Alerts**

**Alert 1: High Error Rate**

```json
{
  "name": "High Error Rate Alert",
  "description": "Alert when error rate exceeds 1%",
  "severity": 1,
  "condition": {
    "allOf": [
      {
        "metricName": "requests/failed",
        "operator": "GreaterThan",
        "threshold": 1,
        "timeAggregation": "Percentage",
        "windowSize": "PT5M"
      }
    ]
  },
  "actions": [
    {
      "actionGroupId": "/subscriptions/.../actionGroups/emergency-response"
    }
  ]
}
```

**Alert 2: Slow Response Time**

```kusto
// Application Insights query alert
requests
| where timestamp > ago(10m)
| summarize percentile(duration, 95) by bin(timestamp, 1m)
| where percentile_duration_95 > 500
```

**Alert 3: Failed Login Spike**

```kusto
customEvents
| where name == "LoginAttempt"
| where customDimensions.Success == "false"
| where timestamp > ago(1h)
| summarize count() by bin(timestamp, 5m)
| where count_ > 50
```

### **Action Groups**

**Email:**
```json
{
  "actionGroupName": "emergency-response",
  "emailReceivers": [
    {
      "name": "SuperAdmin",
      "emailAddress": "admin@schoolms.com"
    }
  ]
}
```

**SMS:**
```json
{
  "smsReceivers": [
    {
      "name": "OnCall",
      "phoneNumber": "+91-XXXXX-XXXXX"
    }
  ]
}
```

**Webhook (Slack/Teams):**
```json
{
  "webhookReceivers": [
    {
      "name": "Slack",
      "serviceUri": "https://hooks.slack.com/services/..."
    }
  ]
}
```

---

## 🖥️ **Dashboard Examples**

### **Operational Dashboard (Azure Portal)**

```
┌─────────────────────────────────────────────────────┐
│  School Management System - Live Operations         │
├─────────────────────────────────────────────────────┤
│                                                      │
│  🟢 System Status: Healthy                          │
│  📊 Active Users: 247                               │
│  ⏱️  Avg Response Time: 145ms                       │
│  ❌ Error Rate: 0.03%                               │
│                                                      │
│  ┌────────────────────┬──────────────────────────┐ │
│  │ Request Rate       │  CPU Usage               │ │
│  │ [Graph: 45 req/s] │  [Graph: 52%]            │ │
│  └────────────────────┴──────────────────────────┘ │
│                                                      │
│  ┌─────────────────────────────────────────────── │ │
│  │ Top 5 Slowest Endpoints (Last Hour)            │ │
│  ├─────────────────────────────────────────────── │ │
│  │ 1. GET /api/grades/report → 850ms              │ │
│  │ 2. POST /api/attendance → 520ms                │ │
│  │ 3. GET /api/students → 340ms                   │ │
│  │ 4. GET /api/chat/messages → 280ms              │ │
│  │ 5. POST /api/files/upload → 250ms              │ │
│  └─────────────────────────────────────────────── │ │
│                                                      │
│  Recent Errors (Last 30 minutes):                   │
│  • 3 errors → ValidationException (StudentService)  │
│  • 1 error → TimeoutException (Database)            │
└─────────────────────────────────────────────────────┘
```

### **Application Insights Queries**

**Top 10 Slowest Queries:**
```kusto
requests
| where timestamp > ago(1h)
| where success == true
| summarize avg(duration), count() by operation_Name
| order by avg_duration desc
| take 10
```

**Error Breakdown:**
```kusto
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc
```

**User Activity:**
```kusto
customEvents
| where name in ("LoginAttempt", "StudentCreated", "AttendanceMarked")
| where timestamp > ago(1d)
| summarize count() by name, bin(timestamp, 1h)
| render timechart
```

**Geographic Distribution:**
```kusto
requests
| where timestamp > ago(7d)
| summarize count() by client_City, client_CountryOrRegion
| order by count_ desc
| take 20
```

---

## 🔐 **Security Monitoring**

### **Security Events to Track**

```csharp
public class SecurityMonitoringService
{
    public void TrackFailedLogin(string email, string ipAddress)
    {
        _logger.LogWarning(
            "Failed login attempt for {Email} from {IpAddress}",
            email, ipAddress);
        
        _telemetry.TrackEvent("FailedLogin", new Dictionary<string, string>
        {
            ["Email"] = email,
            ["IpAddress"] = ipAddress
        });
    }
    
    public void TrackCrossSchoolAccessAttempt(Guid userId, Guid userSchool, Guid attemptedSchool)
    {
        _logger.LogCritical(
            "SECURITY: User {UserId} from School {UserSchool} attempted to access School {AttemptedSchool}",
            userId, userSchool, attemptedSchool);
        
        _telemetry.TrackEvent("CrossSchoolAccessAttempt", new Dictionary<string, string>
        {
            ["UserId"] = userId.ToString(),
            ["UserSchool"] = userSchool.ToString(),
            ["AttemptedSchool"] = attemptedSchool.ToString(),
            ["Severity"] = "Critical"
        });
        
        // Immediate alert
        _alertService.SendSecurityAlert("Cross-school access attempt detected!");
    }
    
    public void TrackSuperAdminAccess(Guid userId, Guid schoolId, string action)
    {
        _logger.LogWarning(
            "SuperAdmin {UserId} accessed School {SchoolId} - Action: {Action}",
            userId, schoolId, action);
        
        // Store in audit log
        await _auditLogService.LogAsync(new AuditLog
        {
            UserId = userId,
            SchoolId = schoolId,
            Action = action,
            Severity = "Warning",
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### **Security Alerts**

```kusto
// Alert: Multiple failed logins from same IP
customEvents
| where name == "FailedLogin"
| where timestamp > ago(5m)
| summarize count() by tostring(customDimensions.IpAddress)
| where count_ > 10

// Alert: Cross-school access attempts
customEvents
| where name == "CrossSchoolAccessAttempt"
| where timestamp > ago(1h)

// Alert: Unusual SuperAdmin activity
customEvents
| where name == "SuperAdminAccess"
| where timestamp > ago(1h)
| summarize count() by tostring(customDimensions.UserId)
| where count_ > 20  // More than 20 accesses in 1 hour
```

---

## 📱 **Frontend Monitoring**

### **React Error Boundary**

```typescript
import { Component, ReactNode } from 'react';
import * as Sentry from '@sentry/react';

class ErrorBoundary extends Component<
  { children: ReactNode },
  { hasError: boolean }
> {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // Log to Application Insights
    appInsights.trackException({
      exception: error,
      properties: errorInfo
    });
    
    // Also log to Sentry (alternative)
    Sentry.captureException(error);
  }

  render() {
    if (this.state.hasError) {
      return <ErrorFallback />;
    }
    return this.props.children;
  }
}
```

### **Performance Tracking**

```typescript
// Track page load time
useEffect(() => {
  const loadTime = performance.timing.loadEventEnd - performance.timing.navigationStart;
  
  appInsights.trackMetric({
    name: 'PageLoadTime',
    average: loadTime,
    properties: {
      page: window.location.pathname,
      user: currentUser.id
    }
  });
}, []);

// Track API calls
const fetchStudents = async () => {
  const startTime = performance.now();
  
  try {
    const response = await api.get('/api/students');
    const duration = performance.now() - startTime;
    
    appInsights.trackMetric({
      name: 'APICallDuration',
      average: duration,
      properties: {
        endpoint: '/api/students',
        success: 'true'
      }
    });
    
    return response.data;
  } catch (error) {
    appInsights.trackException({
      exception: error,
      properties: {
        endpoint: '/api/students'
      }
    });
    throw error;
  }
};
```

---

## ✅ **Monitoring Checklist**

**Logging:**
- [ ] Serilog configured with structured logging
- [ ] Logs written to console, file, and database
- [ ] Log levels configured (Info+ in prod)
- [ ] Sensitive data not logged
- [ ] Correlation IDs implemented

**Metrics:**
- [ ] Application Insights configured
- [ ] Custom metrics tracking business events
- [ ] Performance metrics baseline established
- [ ] Cost tracking enabled

**Tracing:**
- [ ] Distributed tracing with correlation IDs
- [ ] Dependency tracking enabled
- [ ] End-to-end request visibility

**Alerts:**
- [ ] Critical alerts configured (P0, P1)
- [ ] Action groups set up (email, SMS)
- [ ] Alert recipients updated
- [ ] Test alerts sent and verified
- [ ] Runbooks documented for common alerts

**Dashboards:**
- [ ] Operational dashboard created
- [ ] Performance dashboard created
- [ ] Security dashboard created
- [ ] Business metrics dashboard created

**Security Monitoring:**
- [ ] Failed login tracking
- [ ] Cross-school access detection
- [ ] SuperAdmin activity auditing
- [ ] Anomaly detection configured

---

## 📚 **Next Steps**

1. **Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)
2. **Testing:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)
3. **Compliance:** [15_COMPLIANCE_PRIVACY.md](./15_COMPLIANCE_PRIVACY.md)

---

**Document Status:** ✅ Complete  
**Implementation:** 🟡 Partially Complete (Serilog ✅, AI ❌)  
**Priority:** Configure Application Insights before production
