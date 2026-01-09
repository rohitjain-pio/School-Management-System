# External Partner Integration Guide

## Overview

This guide helps third-party developers integrate with the School Management System API. It covers authentication strategies, common integration patterns, webhooks, security best practices, and support for parent portals, mobile apps, and other external systems.

## Table of Contents

- [Integration Overview](#integration-overview)
- [Partner Onboarding](#partner-onboarding)
- [Authentication for Partners](#authentication-for-partners)
- [Common Integration Patterns](#common-integration-patterns)
- [Parent Portal Integration](#parent-portal-integration)
- [Mobile App Integration](#mobile-app-integration)
- [Data Synchronization](#data-synchronization)
- [Security Considerations](#security-considerations)
- [Rate Limiting](#rate-limiting)
- [Support and SLA](#support-and-sla)

---

## Integration Overview

### Use Cases

**Common integration scenarios:**

1. **Parent Portals**
   - View student information
   - Check attendance records
   - Receive announcements
   - Monitor academic progress

2. **Mobile Applications**
   - Native iOS/Android apps
   - Student/teacher/parent apps
   - Real-time notifications

3. **Learning Management Systems (LMS)**
   - Sync student rosters
   - Import/export grades
   - Assignment integration

4. **HR Systems**
   - Teacher onboarding
   - Payroll integration
   - Attendance tracking

5. **Communication Platforms**
   - SMS notifications
   - Email campaigns
   - Push notifications

### Architecture

```
┌─────────────────┐
│  Partner App    │
│  (Web/Mobile)   │
└────────┬────────┘
         │
         │ HTTPS/REST
         │
┌────────▼────────┐
│   SMS API       │
│  (ASP.NET Core) │
├─────────────────┤
│  Authentication │
│  Authorization  │
│  Rate Limiting  │
└────────┬────────┘
         │
┌────────▼────────┐
│   SQL Server    │
│   Database      │
└─────────────────┘
```

---

## Partner Onboarding

### Step 1: Register Partner Account

**Contact:** SMS API Team to register as a partner

**Provide:**
- Company/Organization name
- Contact email
- Technical contact
- Use case description
- Expected API usage volume

**Receive:**
- API access credentials
- Partner ID (for tracking)
- Documentation access
- Sandbox environment URL

### Step 2: Sandbox Access

**Sandbox Environment:**
```
Base URL: https://sandbox-api.sms.edu
Swagger: https://sandbox-api.sms.edu/swagger
```

**Test Credentials:**
- Username: `partner_test`
- Password: Provided by API team
- School ID: Test school UUID

### Step 3: Development and Testing

1. Review API documentation
2. Implement authentication
3. Build integration endpoints
4. Test in sandbox environment
5. Perform security audit
6. Load testing (if high volume)

### Step 4: Production Approval

**Submit for review:**
- Integration documentation
- Security checklist completion
- Test results
- Error handling implementation
- Rate limit compliance

**Approval process:**
- Technical review (1-2 weeks)
- Security audit
- Production credentials issued

---

## Authentication for Partners

### Option 1: Service Account (Recommended)

**Best for:** Backend integrations, scheduled tasks

**Setup:**
1. Create dedicated service account
2. Assign appropriate role (e.g., `Partner` role with limited permissions)
3. Use long-lived credentials with secure storage

**Example:**
```javascript
// Service account credentials (secure storage)
const SERVICE_ACCOUNT = {
  username: 'partner_service_account',
  password: process.env.SMS_API_PASSWORD // From secure env
};

// Authenticate once, reuse token
let authToken = null;
let tokenExpiry = null;

async function getAuthToken() {
  // Refresh if expired
  if (!authToken || Date.now() > tokenExpiry) {
    const response = await fetch('https://api.sms.edu/api/Auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(SERVICE_ACCOUNT)
    });
    
    const data = await response.json();
    authToken = data.token;
    tokenExpiry = Date.now() + (2.5 * 60 * 60 * 1000); // 2.5 hours
  }
  
  return authToken;
}

// Use in requests
async function apiRequest(url, options = {}) {
  const token = await getAuthToken();
  
  return fetch(url, {
    ...options,
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers
    }
  });
}
```

### Option 2: User-Delegated Auth

**Best for:** User-facing applications (parent portals, mobile apps)

**Flow:**
1. User enters their SMS credentials in partner app
2. Partner app authenticates with SMS API
3. Token stored securely on user's device
4. Requests made on behalf of user

**Example:**
```javascript
// User login in partner app
async function loginUser(username, password) {
  const response = await fetch('https://api.sms.edu/api/Auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userName: username, password: password })
  });
  
  if (response.ok) {
    const { token } = await response.json();
    // Store securely (e.g., encrypted storage, secure cookie)
    await secureStorage.set('sms_token', token);
    return token;
  }
  
  throw new Error('Login failed');
}

// Make authenticated requests on behalf of user
async function getUserStudents(userId) {
  const token = await secureStorage.get('sms_token');
  
  const response = await fetch(`https://api.sms.edu/api/Student?userId=${userId}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  return await response.json();
}
```

### Option 3: OAuth 2.0 (Future)

**Status:** Planned for v2.0

Will support standard OAuth 2.0 flows:
- Authorization Code Flow (for web apps)
- PKCE Flow (for mobile/SPA)
- Client Credentials Flow (for service accounts)

---

## Common Integration Patterns

### Pattern 1: Data Retrieval (Read-Only)

**Use Case:** Parent portal displaying student information

```python
# Python example - Parent portal backend
import requests
from typing import List, Dict

class SMSAPIClient:
    def __init__(self, base_url: str, username: str, password: str):
        self.base_url = base_url
        self.session = requests.Session()
        self._authenticate(username, password)
    
    def _authenticate(self, username: str, password: str):
        response = self.session.post(
            f"{self.base_url}/api/Auth/login",
            json={"userName": username, "password": password}
        )
        response.raise_for_status()
    
    def get_student_info(self, student_id: str) -> Dict:
        """Get student profile"""
        response = self.session.get(
            f"{self.base_url}/api/Student/{student_id}"
        )
        response.raise_for_status()
        return response.json()["data"]
    
    def get_student_attendance(
        self,
        student_id: str,
        start_date: str,
        end_date: str
    ) -> List[Dict]:
        """Get attendance records"""
        response = self.session.get(
            f"{self.base_url}/api/Attendance",
            params={
                "studentId": student_id,
                "startDate": start_date,
                "endDate": end_date
            }
        )
        response.raise_for_status()
        return response.json()["data"]
    
    def get_announcements(self, school_id: str) -> List[Dict]:
        """Get school announcements"""
        response = self.session.get(
            f"{self.base_url}/api/Announcement",
            params={"schoolId": school_id}
        )
        response.raise_for_status()
        return response.json()["data"]

# Usage in parent portal
client = SMSAPIClient(
    "https://api.sms.edu",
    "parent_service_account",
    os.getenv("SMS_PASSWORD")
)

# Get student info for logged-in parent
student = client.get_student_info(student_id)
attendance = client.get_student_attendance(
    student_id,
    "2024-01-01",
    "2024-01-31"
)
announcements = client.get_announcements(student["schoolId"])
```

### Pattern 2: Data Synchronization

**Use Case:** Sync student roster with LMS

```csharp
// C# example - LMS integration
public class SMSSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SMSSyncService> _logger;
    
    public async Task SyncStudentsAsync(Guid schoolId)
    {
        // Fetch all students from SMS API
        var response = await _httpClient.GetAsync(
            $"/api/Student?schoolId={schoolId}"
        );
        
        var result = await response.Content
            .ReadFromJsonAsync<ApiResult<List<Student>>>();
        
        var students = result.Data;
        
        // Sync with local LMS database
        foreach (var student in students)
        {
            await UpsertStudentInLMS(student);
        }
        
        _logger.LogInformation(
            "Synced {Count} students from SMS API",
            students.Count
        );
    }
    
    private async Task UpsertStudentInLMS(Student student)
    {
        // Check if student exists in LMS
        var existing = await _lmsDb.Students
            .FirstOrDefaultAsync(s => s.ExternalId == student.Id);
        
        if (existing != null)
        {
            // Update existing
            existing.FirstName = student.FirstName;
            existing.LastName = student.LastName;
            existing.Email = student.Email;
            await _lmsDb.SaveChangesAsync();
        }
        else
        {
            // Create new
            _lmsDb.Students.Add(new LMSStudent
            {
                ExternalId = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email
            });
            await _lmsDb.SaveChangesAsync();
        }
    }
}

// Schedule sync
public class SyncJob : IHostedService
{
    private Timer _timer;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Sync every 6 hours
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(6)
        );
        
        return Task.CompletedTask;
    }
    
    private void DoWork(object state)
    {
        // Run sync
        _syncService.SyncStudentsAsync(schoolId).Wait();
    }
}
```

### Pattern 3: Webhook Notifications (Future)

**Status:** Planned for v1.2

**Concept:**
```javascript
// Register webhook endpoint
POST /api/Webhooks/register
{
  "url": "https://partner.com/webhooks/sms",
  "events": ["student.created", "attendance.marked", "announcement.posted"],
  "secret": "webhook_secret_for_verification"
}

// Partner receives webhook
POST https://partner.com/webhooks/sms
Headers:
  X-SMS-Event: student.created
  X-SMS-Signature: sha256=...

Body:
{
  "event": "student.created",
  "timestamp": "2024-01-09T10:00:00Z",
  "data": {
    "studentId": "uuid",
    "schoolId": "uuid",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

---

## Parent Portal Integration

### Complete Parent Portal Example

```typescript
// TypeScript React example
import React, { useState, useEffect } from 'react';

interface Student {
  id: string;
  firstName: string;
  lastName: string;
  classId: string;
  email: string;
}

interface Attendance {
  id: string;
  date: string;
  status: 'Present' | 'Absent' | 'Late' | 'Excused';
  remarks?: string;
}

class ParentPortalAPI {
  private baseUrl = 'https://api.sms.edu/api';
  private token: string | null = null;
  
  async login(username: string, password: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ userName: username, password: password })
    });
    
    if (!response.ok) {
      throw new Error('Login failed');
    }
  }
  
  async getChildren(): Promise<Student[]> {
    // Assuming parent account has linked students
    const response = await fetch(`${this.baseUrl}/Student/my-children`, {
      credentials: 'include'
    });
    
    const result = await response.json();
    return result.data;
  }
  
  async getAttendance(
    studentId: string,
    startDate: string,
    endDate: string
  ): Promise<Attendance[]> {
    const response = await fetch(
      `${this.baseUrl}/Attendance?studentId=${studentId}&startDate=${startDate}&endDate=${endDate}`,
      { credentials: 'include' }
    );
    
    const result = await response.json();
    return result.data;
  }
}

// React Component
const ParentDashboard: React.FC = () => {
  const [students, setStudents] = useState<Student[]>([]);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [attendance, setAttendance] = useState<Attendance[]>([]);
  const api = new ParentPortalAPI();
  
  useEffect(() => {
    loadStudents();
  }, []);
  
  async function loadStudents() {
    const data = await api.getChildren();
    setStudents(data);
    if (data.length > 0) {
      setSelectedStudent(data[0]);
      loadAttendance(data[0].id);
    }
  }
  
  async function loadAttendance(studentId: string) {
    const endDate = new Date().toISOString().split('T')[0];
    const startDate = new Date(Date.now() - 30*24*60*60*1000)
      .toISOString().split('T')[0];
    
    const data = await api.getAttendance(studentId, startDate, endDate);
    setAttendance(data);
  }
  
  return (
    <div className="parent-dashboard">
      <h1>Parent Portal</h1>
      
      <div className="student-selector">
        {students.map(student => (
          <button
            key={student.id}
            onClick={() => {
              setSelectedStudent(student);
              loadAttendance(student.id);
            }}
          >
            {student.firstName} {student.lastName}
          </button>
        ))}
      </div>
      
      {selectedStudent && (
        <div className="student-details">
          <h2>{selectedStudent.firstName} {selectedStudent.lastName}</h2>
          
          <h3>Attendance Record (Last 30 Days)</h3>
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Status</th>
                <th>Remarks</th>
              </tr>
            </thead>
            <tbody>
              {attendance.map(record => (
                <tr key={record.id}>
                  <td>{record.date}</td>
                  <td className={`status-${record.status.toLowerCase()}`}>
                    {record.status}
                  </td>
                  <td>{record.remarks || '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          
          <div className="stats">
            <p>Present: {attendance.filter(a => a.status === 'Present').length}</p>
            <p>Absent: {attendance.filter(a => a.status === 'Absent').length}</p>
            <p>Late: {attendance.filter(a => a.status === 'Late').length}</p>
          </div>
        </div>
      )}
    </div>
  );
};
```

---

## Mobile App Integration

### iOS Swift Example

```swift
// Swift iOS example
import Foundation

class SMSAPIClient {
    private let baseURL = "https://api.sms.edu/api"
    private var authToken: String?
    
    func login(username: String, password: String) async throws {
        let url = URL(string: "\(baseURL)/Auth/login")!
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = ["userName": username, "password": password]
        request.httpBody = try JSONSerialization.data(withJSONObject: body)
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse,
              httpResponse.statusCode == 200 else {
            throw APIError.loginFailed
        }
        
        let result = try JSONDecoder().decode(LoginResponse.self, from: data)
        self.authToken = result.token
        
        // Store token securely
        KeychainHelper.save(result.token, forKey: "sms_auth_token")
    }
    
    func getStudents() async throws -> [Student] {
        let url = URL(string: "\(baseURL)/Student")!
        var request = URLRequest(url: url)
        request.setValue("Bearer \(authToken ?? "")", forHTTPHeaderField: "Authorization")
        
        let (data, _) = try await URLSession.shared.data(for: request)
        let result = try JSONDecoder().decode(APIResult<[Student]>.self, from: data)
        
        return result.data
    }
}

struct Student: Codable {
    let id: String
    let firstName: String
    let lastName: String
    let email: String
}

struct APIResult<T: Codable>: Codable {
    let isSuccess: Bool
    let data: T
    let errorMessage: String?
}

enum APIError: Error {
    case loginFailed
    case unauthorized
}
```

---

## Security Considerations

### 1. Secure Credential Storage

**DO:**
- Use environment variables for production
- Azure Key Vault / AWS Secrets Manager
- Encrypted configuration files
- Secure mobile device storage (Keychain/Keystore)

**DON'T:**
- Hard-code credentials in source code
- Commit credentials to version control
- Store passwords in plain text

### 2. HTTPS Only

All API communication must use HTTPS. Certificate validation should be enabled in production.

### 3. Input Validation

Validate and sanitize all data before sending to API:

```javascript
function validateEmail(email) {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email);
}

function sanitizeInput(input) {
  // Remove potentially dangerous characters
  return input.replace(/[<>'"]/g, '');
}
```

### 4. Token Management

- Store tokens securely
- Refresh before expiration
- Clear tokens on logout
- Implement token rotation

### 5. Error Handling

Never expose sensitive information in error messages:

```javascript
// Bad
catch (error) {
  alert(`Error: ${error.message}`); // May expose internal details
}

// Good
catch (error) {
  console.error('API error:', error); // Log for debugging
  alert('An error occurred. Please try again.'); // Generic user message
}
```

---

## Rate Limiting

### Current Limits (v1.0)

- **General API:** 100 requests per minute per IP
- **Chat messages:** 30 messages per minute per user
- **Authentication:** 10 login attempts per 15 minutes per IP

### Handling Rate Limits

```javascript
async function apiRequestWithRetry(url, options, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = await fetch(url, options);
      
      if (response.status === 429) {
        // Rate limited
        const retryAfter = response.headers.get('Retry-After') || 60;
        console.log(`Rate limited. Retrying after ${retryAfter}s`);
        await sleep(retryAfter * 1000);
        continue;
      }
      
      return response;
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      await sleep(1000 * Math.pow(2, i)); // Exponential backoff
    }
  }
}
```

---

## Support and SLA

### Support Channels

**Technical Support:**
- Email: api-support@sms.edu
- Documentation: https://docs.sms.edu
- Status Page: https://status.sms.edu

**Response Times:**
- Critical issues: 2 hours
- High priority: 8 hours
- Medium priority: 24 hours
- Low priority: 48 hours

### SLA (Service Level Agreement)

- **Uptime:** 99.9% monthly
- **API Response Time:** < 500ms (95th percentile)
- **Planned Maintenance:** Announced 48 hours in advance

### Breaking Changes Policy

- **Notice:** Minimum 3 months before breaking changes
- **Deprecation:** Features marked deprecated 6 months before removal
- **Version Support:** Previous major version supported for 12 months

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Contact:** api-partners@sms.edu  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Authentication](./AUTHENTICATION.md)
- [Code Samples](./CODE_SAMPLES.md)
