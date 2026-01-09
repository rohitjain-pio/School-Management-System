# Penetration Testing Guide - School Management System API

**Last Updated:** January 9, 2026  
**Version:** 1.0  
**Scope:** Web API, SignalR WebSockets, Database

---

## Table of Contents

- [Overview](#overview)
- [Pre-Testing Requirements](#pre-testing-requirements)
- [Testing Methodology](#testing-methodology)
- [Attack Scenarios](#attack-scenarios)
- [Testing Tools](#testing-tools)
- [Vulnerability Assessment](#vulnerability-assessment)
- [Remediation Guidelines](#remediation-guidelines)
- [Reporting Requirements](#reporting-requirements)

---

## Overview

### Purpose

This document outlines the penetration testing approach for the School Management System (SMS) API. Penetration testing simulates real-world attacks to identify security vulnerabilities before malicious actors can exploit them.

### Scope of Testing

**In-Scope:**
- REST API endpoints (`/api/*`)
- SignalR hubs (`/chatHub`, `/videoCallHub`)
- Authentication mechanisms (JWT, cookies, refresh tokens)
- Authorization controls (RBAC, resource-based)
- Database security (SQL Server)
- File upload functionality
- Session management
- Rate limiting mechanisms
- CORS configuration
- Security headers

**Out-of-Scope:**
- Social engineering attacks
- Physical security
- Denial of Service (DoS) attacks (unless specifically requested)
- Testing on production environment (use staging only)
- Network infrastructure (unless API-related)

### Testing Schedule

**Frequency:**
- **Full Penetration Test**: Annually
- **Targeted Testing**: After major releases or security updates
- **Vulnerability Scans**: Quarterly
- **Code Security Review**: Before each release

---

## Pre-Testing Requirements

### 1. Legal Authorization

**Required Documentation:**
- [ ] Signed penetration testing agreement
- [ ] Rules of engagement document
- [ ] Scope approval from stakeholders
- [ ] Non-disclosure agreement (NDA) with testing team
- [ ] Emergency contact list

**Rules of Engagement:**
```
Authorized Actions:
✓ Vulnerability scanning
✓ Automated testing tools (OWASP ZAP, Burp Suite)
✓ Manual penetration testing
✓ Exploitation of discovered vulnerabilities (staging only)
✓ Social engineering (if pre-approved)

Prohibited Actions:
✗ Testing on production environment without approval
✗ Data destruction or corruption
✗ DoS/DDoS attacks
✗ Testing outside defined scope
✗ Sharing discovered vulnerabilities publicly before remediation
```

### 2. Test Environment Setup

**Staging Environment Requirements:**
- Identical to production configuration
- Separate database with realistic test data
- Isolated from production network
- Monitoring and logging enabled
- Backups created before testing

**Test Accounts:**
```
Student Account:
  Username: pentest_student
  Password: PenTest123!
  Role: Student

Teacher Account:
  Username: pentest_teacher
  Password: PenTest123!
  Role: Teacher

Admin Account:
  Username: pentest_admin
  Password: PenTest123!
  Role: Admin

SuperAdmin Account:
  Username: pentest_superadmin
  Password: PenTest123!
  Role: SuperAdmin
```

### 3. Baseline Documentation

**Collect Before Testing:**
- [ ] API documentation (Swagger/OpenAPI)
- [ ] Network architecture diagram
- [ ] Authentication flow diagrams
- [ ] Database schema
- [ ] Known vulnerabilities from previous tests
- [ ] Security controls documentation

---

## Testing Methodology

### OWASP Testing Framework

Following **OWASP Web Security Testing Guide (WSTG) v4.2**

#### Phase 1: Information Gathering (Reconnaissance)

**Objectives:**
- Identify all API endpoints
- Map application structure
- Discover hidden functionality
- Enumerate user roles and permissions

**Activities:**
1. **Passive Reconnaissance**
   - Review Swagger documentation (`/swagger/index.html`)
   - Analyze client-side JavaScript (React app)
   - Check for exposed `.git` directory
   - Search for leaked credentials in public repos

2. **Active Reconnaissance**
   - Enumerate endpoints with directory brute-forcing
   - Test for HTTP methods (GET, POST, PUT, DELETE, OPTIONS, TRACE)
   - Identify API versioning
   - Map authentication endpoints

**Tools:**
- `curl`, `wget`
- Burp Suite Spider
- Postman/Newman
- `dirsearch`, `gobuster`

**Example Commands:**
```bash
# Enumerate endpoints
curl -X OPTIONS https://api-staging.sms.edu/api/Student

# Test for hidden endpoints
gobuster dir -u https://api-staging.sms.edu \
  -w /usr/share/wordlists/dirbuster/directory-list-2.3-medium.txt \
  -t 50

# Check for .git exposure
curl https://api-staging.sms.edu/.git/config
```

#### Phase 2: Authentication Testing

**Test Cases:**

1. **Weak Password Policy**
   ```bash
   # Attempt registration with weak password
   curl -X POST https://api-staging.sms.edu/api/Auth/register \
     -H "Content-Type: application/json" \
     -d '{"userName":"weaktest","password":"123","email":"weak@test.com","role":"Student"}'
   ```

2. **Brute Force Attack**
   ```bash
   # Use Hydra for brute force (rate limiting should block this)
   hydra -l admin_test -P /usr/share/wordlists/rockyou.txt \
     -s 7266 api-staging.sms.edu https-post-form \
     "/api/Auth/login:userName=^USER^&password=^PASS^:F=Unauthorized"
   ```

3. **Session Fixation**
   ```bash
   # Attempt to set JWT token before authentication
   # Token should change after login
   ```

4. **JWT Vulnerabilities**
   ```python
   # Test with jwt_tool
   python3 jwt_tool.py <JWT_TOKEN> -M at
   python3 jwt_tool.py <JWT_TOKEN> -M pb  # Brute force secret
   python3 jwt_tool.py <JWT_TOKEN> -X a   # Algorithm confusion (None attack)
   ```

5. **Token Expiration**
   ```bash
   # Modify token expiration to future date
   # Token should be rejected (signature validation)
   ```

#### Phase 3: Authorization Testing

**Test Cases:**

1. **Vertical Privilege Escalation**
   ```bash
   # Login as Student, attempt Admin action
   TOKEN=$(curl -X POST https://api-staging.sms.edu/api/Auth/login \
     -H "Content-Type: application/json" \
     -d '{"userName":"pentest_student","password":"PenTest123!"}' \
     | jq -r '.data.token')

   # Attempt to create user (Admin-only)
   curl -X POST https://api-staging.sms.edu/api/User \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"userName":"hacker","password":"Hack123!","role":"Admin"}'
   
   # Expected: 403 Forbidden
   ```

2. **Horizontal Privilege Escalation (IDOR)**
   ```bash
   # Student A accessing Student B's data
   # Login as Student A
   TOKEN_A=$(curl -X POST https://api-staging.sms.edu/api/Auth/login \
     -H "Content-Type: application/json" \
     -d '{"userName":"student_a","password":"Test123!"}' | jq -r '.data.token')

   # Get Student B's ID (enumeration attack)
   STUDENT_B_ID="<student-b-guid>"

   # Attempt to access Student B's record
   curl -X GET https://api-staging.sms.edu/api/Student/$STUDENT_B_ID \
     -H "Authorization: Bearer $TOKEN_A"
   
   # Expected: 403 Forbidden
   ```

3. **Multi-Tenant Bypass**
   ```bash
   # Admin from School A accessing School B's data
   # Modify SchoolId in request
   curl -X GET https://api-staging.sms.edu/api/Student?schoolId=<school-b-guid> \
     -H "Authorization: Bearer $ADMIN_TOKEN_SCHOOL_A"
   
   # Expected: 403 Forbidden or empty result
   ```

#### Phase 4: Input Validation Testing

**Test Cases:**

1. **SQL Injection**
   ```bash
   # Test search endpoint
   curl "https://api-staging.sms.edu/api/Student?search=Robert%27%20OR%20%271%27%3D%271" \
     -H "Authorization: Bearer $TOKEN"
   
   # Test with SQLMap
   sqlmap -u "https://api-staging.sms.edu/api/Student?search=test" \
     --cookie="auth_token=$TOKEN" \
     --batch --level=5 --risk=3
   ```

2. **NoSQL Injection** (if MongoDB used)
   ```json
   {
     "email": {"$gt": ""},
     "password": {"$gt": ""}
   }
   ```

3. **XSS (Cross-Site Scripting)**
   ```bash
   # Stored XSS in chat message
   curl -X POST https://api-staging.sms.edu/api/ChatRooms/messages \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "roomId":"<room-guid>",
       "content":"<script>alert(document.cookie)</script>"
     }'
   
   # Check if message is sanitized when retrieved
   ```

4. **XXE (XML External Entity)**
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
   <user>
     <name>&xxe;</name>
   </user>
   ```

5. **Command Injection**
   ```bash
   # Test file upload filename
   curl -X POST https://api-staging.sms.edu/api/Student/upload \
     -H "Authorization: Bearer $TOKEN" \
     -F "file=@test.jpg;filename=test.jpg;rm -rf /"
   ```

6. **Path Traversal**
   ```bash
   # Test file download
   curl "https://api-staging.sms.edu/api/File?path=../../etc/passwd" \
     -H "Authorization: Bearer $TOKEN"
   ```

7. **LDAP Injection**
   ```bash
   # If LDAP authentication used
   curl -X POST https://api-staging.sms.edu/api/Auth/login \
     -d '{"userName":"admin)(|(password=*)","password":"x"}'
   ```

#### Phase 5: Session Management Testing

**Test Cases:**

1. **Session Timeout**
   ```bash
   # Verify token expires after 3 hours
   sleep 10800  # 3 hours
   curl https://api-staging.sms.edu/api/Student \
     -H "Authorization: Bearer $OLD_TOKEN"
   
   # Expected: 401 Unauthorized
   ```

2. **Token Invalidation on Logout**
   ```bash
   # Logout
   curl -X POST https://api-staging.sms.edu/api/Auth/logout \
     -H "Authorization: Bearer $TOKEN"
   
   # Attempt to use same token
   curl https://api-staging.sms.edu/api/Student \
     -H "Authorization: Bearer $TOKEN"
   
   # Expected: 401 Unauthorized
   ```

3. **CSRF (Cross-Site Request Forgery)**
   ```html
   <!-- Host on attacker site -->
   <form action="https://api-staging.sms.edu/api/Student" method="POST">
     <input name="firstName" value="Hacked" />
     <input name="lastName" value="User" />
   </form>
   <script>document.forms[0].submit();</script>
   ```
   - Expected: Blocked by SameSite cookie attribute

#### Phase 6: Business Logic Testing

**Test Cases:**

1. **Account Enumeration**
   ```bash
   # Different responses for existing vs non-existing users
   curl -X POST https://api-staging.sms.edu/api/Auth/login \
     -d '{"userName":"admin","password":"wrong"}'
   # Response: "Invalid credentials" or "User not found"?
   
   # Should return same generic message
   ```

2. **Rate Limit Bypass**
   ```bash
   # Attempt to bypass rate limit with:
   # - IP rotation
   # - User-Agent rotation
   # - Referrer spoofing
   
   for i in {1..100}; do
     curl -X POST https://api-staging.sms.edu/api/Auth/login \
       -H "X-Forwarded-For: 192.168.1.$i" \
       -d '{"userName":"admin","password":"test"}'
   done
   ```

3. **Mass Assignment**
   ```bash
   # Attempt to set admin role during registration
   curl -X POST https://api-staging.sms.edu/api/Auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "userName":"hacker",
       "password":"Hack123!",
       "email":"hacker@test.com",
       "role":"SuperAdmin",
       "isAdmin":true
     }'
   ```

4. **Resource Exhaustion**
   ```bash
   # Upload extremely large files
   dd if=/dev/zero of=bigfile.jpg bs=1M count=1000
   curl -X POST https://api-staging.sms.edu/api/Student/upload \
     -H "Authorization: Bearer $TOKEN" \
     -F "file=@bigfile.jpg"
   ```

5. **Price/Quantity Manipulation**
   - Not applicable unless payment system exists

#### Phase 7: Cryptography Testing

**Test Cases:**

1. **Weak Encryption Algorithms**
   ```bash
   # Check TLS version and cipher suites
   nmap --script ssl-enum-ciphers -p 443 api-staging.sms.edu
   
   # Test SSL/TLS vulnerabilities
   testssl.sh https://api-staging.sms.edu
   ```

2. **Insecure Random Number Generation**
   - Review password reset token generation
   - Check session ID randomness

3. **Padding Oracle Attack**
   - Test if encryption uses CBC mode without proper padding

#### Phase 8: Error Handling Testing

**Test Cases:**

1. **Information Disclosure in Errors**
   ```bash
   # Trigger server error
   curl -X POST https://api-staging.sms.edu/api/Student \
     -H "Authorization: Bearer $TOKEN" \
     -d 'invalid json'
   
   # Check if stack trace or internal paths exposed
   ```

2. **Debug Mode Enabled**
   ```bash
   # Check for debug endpoints
   curl https://api-staging.sms.edu/debug
   curl https://api-staging.sms.edu/trace.axd
   ```

---

## Attack Scenarios

### Scenario 1: Student Data Breach

**Objective:** Unauthorized access to all student records

**Attack Path:**
1. **Reconnaissance**
   - Enumerate student IDs via IDOR
   - Identify pagination endpoints

2. **Exploit IDOR**
   ```python
   import requests
   import uuid

   token = "student-jwt-token"
   headers = {"Authorization": f"Bearer {token}"}

   for i in range(1000):
       student_id = str(uuid.uuid4())
       response = requests.get(
           f"https://api-staging.sms.edu/api/Student/{student_id}",
           headers=headers
       )
       if response.status_code == 200:
           print(f"Found student: {response.json()}")
   ```

3. **Mass Data Exfiltration**
   - Use pagination to retrieve all students
   - Bypass rate limiting with distributed requests

**Expected Defense:**
- Resource-based authorization blocks cross-user access
- Rate limiting prevents mass enumeration
- Audit logs detect suspicious access patterns

### Scenario 2: Privilege Escalation

**Objective:** Student gains Admin access

**Attack Path:**
1. **JWT Manipulation**
   - Decode JWT token
   - Modify `role` claim to "Admin"
   - Re-encode with weak secret (if brute-forced)

2. **Role Parameter Injection**
   ```bash
   # During registration
   curl -X POST https://api-staging.sms.edu/api/Auth/register \
     -d '{"userName":"hacker","password":"Hack123!","role":"Admin"}'
   ```

3. **Session Hijacking**
   - Steal JWT via XSS
   - Use stolen token to perform admin actions

**Expected Defense:**
- JWT signature validation prevents token manipulation
- Role assignment restricted to admins only
- XSS prevention blocks token theft

### Scenario 3: Chat Message Manipulation

**Objective:** Read private chat messages or impersonate users

**Attack Path:**
1. **Join Private Room Without Password**
   ```bash
   # Bypass password check
   curl -X POST https://api-staging.sms.edu/api/ChatRooms/{room-id}/join \
     -H "Authorization: Bearer $TOKEN"
   ```

2. **Message Injection**
   - Send message with forged sender ID
   - Inject malicious content (XSS)

3. **Message Interception**
   - If encryption is weak or missing, intercept messages

**Expected Defense:**
- Room access tokens validate entry
- Message encryption prevents interception
- XSS sanitization blocks malicious content

### Scenario 4: Video Recording Theft

**Objective:** Download unauthorized video recordings

**Attack Path:**
1. **Enumerate Recording IDs**
   ```bash
   # Guess recording URLs
   for id in $(seq 1 1000); do
     curl "https://api-staging.sms.edu/recordings/$id.mp4" -o "$id.mp4"
   done
   ```

2. **Bypass Access Controls**
   - Access recordings without proper authorization
   - Download recordings from other schools

**Expected Defense:**
- Access tokens required for recording downloads
- Resource-based authorization checks ownership
- Recordings deleted after retention period

---

## Testing Tools

### Essential Tools

1. **Burp Suite Professional**
   - **Purpose**: Web proxy, scanner, repeater
   - **Use Cases**: Manual testing, automated scanning
   - **Cost**: $399/year per user

2. **OWASP ZAP (Zed Attack Proxy)**
   - **Purpose**: Free alternative to Burp Suite
   - **Use Cases**: Automated scanning, API testing
   - **Cost**: Free

3. **Postman/Newman**
   - **Purpose**: API testing, automation
   - **Use Cases**: Functional and security testing
   - **Cost**: Free (Pro: $12/user/month)

4. **SQLMap**
   - **Purpose**: SQL injection testing
   - **Installation**: `sudo apt install sqlmap`

5. **Nmap**
   - **Purpose**: Port scanning, service detection
   - **Installation**: `sudo apt install nmap`

6. **Nikto**
   - **Purpose**: Web server scanner
   - **Installation**: `sudo apt install nikto`

7. **jwt_tool**
   - **Purpose**: JWT analysis and exploitation
   - **Installation**: 
     ```bash
     git clone https://github.com/ticarpi/jwt_tool
     pip3 install termcolor cprint pycryptodomex requests
     ```

8. **Gobuster/Dirsearch**
   - **Purpose**: Directory brute-forcing
   - **Installation**: `sudo apt install gobuster`

### Specialized Tools

9. **Nuclei**
   - **Purpose**: Vulnerability scanner with templates
   - **Installation**: 
     ```bash
     go install -v github.com/projectdiscovery/nuclei/v2/cmd/nuclei@latest
     ```

10. **Commix**
    - **Purpose**: Command injection testing
    - **Installation**: `sudo apt install commix`

11. **XSSer**
    - **Purpose**: XSS detection and exploitation
    - **Installation**: `sudo apt install xsser`

12. **WPScan** (if WordPress integration exists)
    - **Purpose**: WordPress security scanner

### Cloud-Based Tools

13. **Detectify**
    - **Purpose**: Automated security scanner
    - **Cost**: Enterprise pricing

14. **HackerOne**
    - **Purpose**: Bug bounty platform
    - **Cost**: Pay-per-vulnerability

---

## Vulnerability Assessment

### OWASP Top 10 (2021) Mapping

| OWASP Category | SMS API Risk | Test Priority |
|----------------|-------------|---------------|
| **A01: Broken Access Control** | Critical | High |
| **A02: Cryptographic Failures** | High | High |
| **A03: Injection** | Medium | High |
| **A04: Insecure Design** | Medium | Medium |
| **A05: Security Misconfiguration** | High | High |
| **A06: Vulnerable Components** | Medium | Medium |
| **A07: Authentication Failures** | Critical | High |
| **A08: Software & Data Integrity Failures** | Low | Low |
| **A09: Security Logging & Monitoring Failures** | Medium | Medium |
| **A10: Server-Side Request Forgery (SSRF)** | Low | Low |

### CVSS Scoring

**Common Vulnerability Scoring System (CVSS) v3.1**

**Severity Ratings:**
- **Critical (9.0-10.0)**: Immediate remediation required
- **High (7.0-8.9)**: Remediation within 7 days
- **Medium (4.0-6.9)**: Remediation within 30 days
- **Low (0.1-3.9)**: Remediation within 90 days

**Example Scoring:**
```
Vulnerability: Student IDOR allowing cross-student data access
CVSS Vector: CVSS:3.1/AV:N/AC:L/PR:L/UI:N/S:U/C:H/I:N/A:N
Base Score: 6.5 (Medium)

Breakdown:
- Attack Vector (AV): Network (N)
- Attack Complexity (AC): Low (L)
- Privileges Required (PR): Low (L)
- User Interaction (UI): None (N)
- Scope (S): Unchanged (U)
- Confidentiality (C): High (H)
- Integrity (I): None (N)
- Availability (A): None (N)
```

---

## Remediation Guidelines

### Remediation Prioritization

1. **Critical Vulnerabilities**
   - **Timeline**: Fix within 24-48 hours
   - **Examples**: 
     - Authentication bypass
     - SQL injection allowing data extraction
     - Remote code execution

2. **High Vulnerabilities**
   - **Timeline**: Fix within 7 days
   - **Examples**:
     - IDOR allowing unauthorized data access
     - XSS allowing session hijacking
     - Privilege escalation

3. **Medium Vulnerabilities**
   - **Timeline**: Fix within 30 days
   - **Examples**:
     - Information disclosure
     - Weak password policy
     - Missing security headers

4. **Low Vulnerabilities**
   - **Timeline**: Fix within 90 days
   - **Examples**:
     - Banner disclosure
     - SSL/TLS configuration issues

### Remediation Workflow

```
1. Vulnerability Reported
   ↓
2. Validate & Reproduce
   ↓
3. Assign CVSS Score
   ↓
4. Prioritize (Critical/High/Medium/Low)
   ↓
5. Develop Fix
   ↓
6. Code Review
   ↓
7. Test Fix
   ↓
8. Deploy to Staging
   ↓
9. Retest (Penetration Tester)
   ↓
10. Deploy to Production
    ↓
11. Update Documentation
    ↓
12. Close Ticket
```

---

## Reporting Requirements

### Penetration Test Report Structure

#### Executive Summary
- Overview of testing
- High-level findings
- Risk summary (Critical/High/Medium/Low counts)
- Recommendations

#### Methodology
- Testing approach (OWASP WSTG)
- Tools used
- Test environment details
- Limitations and constraints

#### Findings

**For Each Vulnerability:**

```markdown
### Finding: [Vulnerability Name]

**Severity:** Critical/High/Medium/Low
**CVSS Score:** X.X
**Category:** OWASP A01 - Broken Access Control

**Description:**
[Detailed description of the vulnerability]

**Impact:**
[Potential impact if exploited]

**Affected Endpoints:**
- POST /api/Student
- GET /api/Student/{id}

**Steps to Reproduce:**
1. Login as Student A
2. Send GET request to /api/Student/{student-b-id}
3. Observe unauthorized data access

**Proof of Concept:**
```bash
curl -X GET https://api-staging.sms.edu/api/Student/abc-123 \
  -H "Authorization: Bearer <student-a-token>"
```

**Remediation:**
Implement resource-based authorization:
```csharp
if (studentId != CurrentUserId && !User.IsInRole("Admin"))
    return Forbid();
```

**References:**
- OWASP WSTG-v4.2: WSTG-ATHZ-01
- CWE-639: Authorization Bypass Through User-Controlled Key
```

#### Risk Matrix

| Severity | Count | Examples |
|----------|-------|----------|
| Critical | 0 | - |
| High | 3 | IDOR, XSS, Privilege Escalation |
| Medium | 12 | Missing headers, weak password policy |
| Low | 8 | Information disclosure, version detection |

#### Remediation Roadmap

```
Week 1: Fix Critical (0)
Week 2-3: Fix High (3)
Week 4-8: Fix Medium (12)
Week 9-12: Fix Low (8)
```

### Report Delivery

**Formats:**
- PDF (for management/stakeholders)
- HTML (for technical team)
- CSV (vulnerability export for tracking)

**Distribution:**
- CTO/CISO
- Development Team Lead
- Security Team
- Compliance Team

---

## Post-Testing Activities

### 1. Remediation Verification

- [ ] Schedule retest after fixes
- [ ] Verify each vulnerability is properly fixed
- [ ] Check for regression issues
- [ ] Update security documentation

### 2. Lessons Learned

- [ ] Document root causes
- [ ] Update secure coding guidelines
- [ ] Train developers on findings
- [ ] Improve security awareness

### 3. Continuous Improvement

- [ ] Add security test cases to CI/CD
- [ ] Implement automated vulnerability scanning
- [ ] Update threat model
- [ ] Plan next penetration test

---

## Compliance and Legal

### Data Protection

**FERPA Compliance:**
- No actual student data accessed during testing
- Use synthetic test data only
- Report any accidental access immediately

**GDPR Compliance:**
- Ensure test data does not contain real PII
- Document data handling procedures
- Delete all test data after engagement

### Responsible Disclosure

If vulnerabilities are discovered:
1. Report to security team immediately (security@sms.edu)
2. Do NOT publicly disclose until patched
3. Allow 90 days for remediation before public disclosure
4. Coordinate disclosure with vendor if third-party components affected

---

## Summary

**Testing Frequency:** Annually (full), Quarterly (scans)  
**Total Attack Scenarios:** 4  
**Total Test Cases:** 50+  
**Critical Areas:** Authentication, Authorization, Input Validation

**Success Criteria:**
- Zero Critical vulnerabilities
- < 5 High vulnerabilities
- All vulnerabilities remediated within SLA

---

**Related Documentation:**
- [Security Implementation Guide](SECURITY_IMPLEMENTATION_GUIDE.md)
- [Security Testing Checklist](SECURITY_TESTING_CHECKLIST.md)
- [Incident Response Plan](INCIDENT_RESPONSE_PLAN.md)
