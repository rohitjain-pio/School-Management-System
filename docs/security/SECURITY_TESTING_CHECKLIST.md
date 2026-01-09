# Security Testing Checklist - School Management System API

**Last Updated:** January 9, 2026  
**Version:** 1.0

---

## Table of Contents

- [Authentication Testing](#authentication-testing)
- [Authorization Testing](#authorization-testing)
- [Input Validation Testing](#input-validation-testing)
- [Session Management Testing](#session-management-testing)
- [Encryption Testing](#encryption-testing)
- [Rate Limiting Testing](#rate-limiting-testing)
- [CORS Testing](#cors-testing)
- [Security Headers Testing](#security-headers-testing)
- [SignalR Security Testing](#signalr-security-testing)
- [Compliance Testing](#compliance-testing)

---

## Authentication Testing

### JWT Token Testing

- [ ] **Test 001: Valid JWT Token**
  - **Action**: Login with valid credentials and obtain JWT token
  - **Expected**: Token is issued with correct claims (sub, email, role, exp, iat)
  - **Validation**: Decode token and verify all claims are present and accurate

- [ ] **Test 002: Expired JWT Token**
  - **Action**: Wait for token to expire (3 hours) or manually set expiration
  - **Expected**: API returns 401 Unauthorized
  - **Error Message**: "Token expired" or "Unauthorized"

- [ ] **Test 003: Invalid JWT Signature**
  - **Action**: Modify token signature or use different signing key
  - **Expected**: API returns 401 Unauthorized
  - **Error Message**: "Invalid token signature"

- [ ] **Test 004: Missing JWT Token**
  - **Action**: Send request to protected endpoint without Authorization header or cookie
  - **Expected**: API returns 401 Unauthorized
  - **Error Message**: "No authorization token provided"

- [ ] **Test 005: Malformed JWT Token**
  - **Action**: Send malformed token (not 3 dot-separated parts)
  - **Expected**: API returns 401 Unauthorized
  - **Error Message**: "Invalid token format"

- [ ] **Test 006: Token with Invalid Issuer**
  - **Action**: Create token with wrong issuer claim
  - **Expected**: API returns 401 Unauthorized

- [ ] **Test 007: Token with Invalid Audience**
  - **Action**: Create token with wrong audience claim
  - **Expected**: API returns 401 Unauthorized

### Refresh Token Testing

- [ ] **Test 101: Valid Refresh Token**
  - **Action**: Use valid refresh token to get new access token
  - **Expected**: New access token and refresh token issued
  - **Validation**: Old refresh token is revoked

- [ ] **Test 102: Expired Refresh Token**
  - **Action**: Use refresh token older than 7 days
  - **Expected**: 401 Unauthorized, "Refresh token expired"

- [ ] **Test 103: Reused Refresh Token**
  - **Action**: Attempt to use same refresh token twice
  - **Expected**: 401 Unauthorized, all tokens for user revoked (security breach detection)

- [ ] **Test 104: Revoked Refresh Token**
  - **Action**: Use refresh token after logout
  - **Expected**: 401 Unauthorized, "Token has been revoked"

- [ ] **Test 105: Refresh Token for Different User**
  - **Action**: Steal another user's refresh token and attempt use
  - **Expected**: 401 Unauthorized (user ID mismatch)

### Password Testing

- [ ] **Test 201: Strong Password**
  - **Action**: Register with password meeting all requirements (min 6 chars, uppercase, lowercase, digit)
  - **Expected**: Registration successful

- [ ] **Test 202: Weak Password - Too Short**
  - **Action**: Register with password < 6 characters
  - **Expected**: 400 Bad Request, "Password must be at least 6 characters"

- [ ] **Test 203: Weak Password - No Uppercase**
  - **Action**: Register with password missing uppercase letter
  - **Expected**: 400 Bad Request, "Password must contain uppercase letter"

- [ ] **Test 204: Weak Password - No Lowercase**
  - **Action**: Register with password missing lowercase letter
  - **Expected**: 400 Bad Request, "Password must contain lowercase letter"

- [ ] **Test 205: Weak Password - No Digit**
  - **Action**: Register with password missing digit
  - **Expected**: 400 Bad Request, "Password must contain digit"

- [ ] **Test 206: Common Password**
  - **Action**: Register with common password like "Password1"
  - **Expected**: 400 Bad Request (if common password check implemented)

- [ ] **Test 207: Password in Breach Database**
  - **Action**: Use password from HaveIBeenPwned database
  - **Expected**: Warning or rejection (if breach check implemented)

### Account Lockout Testing

- [ ] **Test 301: Account Lockout After 5 Failed Attempts**
  - **Action**: Attempt login with wrong password 5 times within 15 minutes
  - **Expected**: Account locked for 30 minutes, 423 Locked response

- [ ] **Test 302: Login After Lockout Period**
  - **Action**: Wait 30 minutes after lockout, attempt login with correct password
  - **Expected**: Login successful, lockout cleared

- [ ] **Test 303: Lockout Counter Reset**
  - **Action**: Fail 3 times, succeed once, fail 5 more times
  - **Expected**: Counter resets after successful login

- [ ] **Test 304: Admin Unlock Account**
  - **Action**: Admin manually unlocks locked account
  - **Expected**: User can login immediately

### MFA Testing (Admin Roles)

- [ ] **Test 401: MFA Required for Admin Login**
  - **Action**: Login as Admin user
  - **Expected**: MFA challenge issued before token

- [ ] **Test 402: Valid TOTP Code**
  - **Action**: Enter correct 6-digit TOTP code
  - **Expected**: Authentication successful, token issued

- [ ] **Test 403: Invalid TOTP Code**
  - **Action**: Enter incorrect TOTP code
  - **Expected**: 401 Unauthorized, "Invalid MFA code"

- [ ] **Test 404: Expired MFA Challenge**
  - **Action**: Wait 5 minutes after MFA challenge issued
  - **Expected**: 401 Unauthorized, "MFA challenge expired"

- [ ] **Test 405: MFA Bypass Attempt**
  - **Action**: Attempt to skip MFA step and request token directly
  - **Expected**: 403 Forbidden, "MFA verification required"

- [ ] **Test 406: Backup Code Usage**
  - **Action**: Use backup code instead of TOTP
  - **Expected**: Authentication successful, backup code invalidated

### Password Reset Testing

- [ ] **Test 501: Valid Password Reset Request**
  - **Action**: Request password reset for valid email
  - **Expected**: Reset email sent, token generated with 15-minute expiration

- [ ] **Test 502: Password Reset with Valid Token**
  - **Action**: Use valid reset token to change password
  - **Expected**: Password changed, all JWT tokens revoked, confirmation email sent

- [ ] **Test 503: Expired Reset Token**
  - **Action**: Use reset token after 15 minutes
  - **Expected**: 400 Bad Request, "Reset token expired"

- [ ] **Test 504: Reused Reset Token**
  - **Action**: Use same reset token twice
  - **Expected**: 400 Bad Request, "Token already used"

- [ ] **Test 505: Reset Token for Wrong Email**
  - **Action**: Use token for email A with email B
  - **Expected**: 400 Bad Request, "Invalid token"

- [ ] **Test 506: Rate Limit Password Reset**
  - **Action**: Request password reset 4 times in 1 hour
  - **Expected**: 429 Too Many Requests after 3rd attempt

### Cookie Security Testing

- [ ] **Test 601: HttpOnly Flag Set**
  - **Action**: Inspect Set-Cookie header
  - **Expected**: HttpOnly flag present (prevents JavaScript access)

- [ ] **Test 602: Secure Flag Set in Production**
  - **Action**: Check cookie in HTTPS environment
  - **Expected**: Secure flag present (HTTPS only)

- [ ] **Test 603: SameSite Attribute**
  - **Action**: Inspect Set-Cookie header
  - **Expected**: SameSite=Strict or SameSite=Lax (CSRF protection)

- [ ] **Test 604: Cookie Expiration**
  - **Action**: Check Max-Age or Expires attribute
  - **Expected**: Matches JWT expiration (3 hours)

- [ ] **Test 605: JavaScript Cookie Access**
  - **Action**: Attempt document.cookie in browser console
  - **Expected**: auth_token cookie NOT accessible

---

## Authorization Testing

### Role-Based Access Control (RBAC)

- [ ] **Test 701: Student Access to Own Records**
  - **Action**: Student requests their own student record
  - **Expected**: 200 OK, record returned

- [ ] **Test 702: Student Access to Other Records**
  - **Action**: Student requests another student's record
  - **Expected**: 403 Forbidden, "Access denied"

- [ ] **Test 703: Teacher Access to Assigned Class**
  - **Action**: Teacher requests student list for assigned class
  - **Expected**: 200 OK, students returned

- [ ] **Test 704: Teacher Access to Unassigned Class**
  - **Action**: Teacher requests student list for unassigned class
  - **Expected**: 403 Forbidden, "Not authorized for this class"

- [ ] **Test 705: Admin Access to All Records**
  - **Action**: Admin requests any student/teacher record in their school
  - **Expected**: 200 OK, record returned

- [ ] **Test 706: Admin Access to Other School**
  - **Action**: Admin at School A requests record from School B
  - **Expected**: 403 Forbidden, "Cross-school access denied"

- [ ] **Test 707: SuperAdmin Full Access**
  - **Action**: SuperAdmin requests any record from any school
  - **Expected**: 200 OK, record returned

- [ ] **Test 708: Principal School-Level Access**
  - **Action**: Principal requests school-wide report
  - **Expected**: 200 OK, report for their school only

### Resource-Based Authorization

- [ ] **Test 801: Student Update Own Profile**
  - **Action**: Student updates their own profile
  - **Expected**: 200 OK, profile updated

- [ ] **Test 802: Student Update Other Profile**
  - **Action**: Student attempts to update another student's profile
  - **Expected**: 403 Forbidden

- [ ] **Test 803: Teacher Mark Attendance for Assigned Class**
  - **Action**: Teacher marks attendance for their class
  - **Expected**: 200 OK, attendance recorded

- [ ] **Test 804: Teacher Mark Attendance for Other Class**
  - **Action**: Teacher marks attendance for class not assigned to them
  - **Expected**: 403 Forbidden

- [ ] **Test 805: IDOR Attack - Modify Student ID in Request**
  - **Action**: Student sends update request with different student ID
  - **Expected**: 403 Forbidden (despite valid token)

### Multi-Tenant Isolation

- [ ] **Test 901: School Isolation - Student Records**
  - **Action**: Query students without school filter
  - **Expected**: Only students from user's school returned

- [ ] **Test 902: School Isolation - Class Management**
  - **Action**: Admin creates class with wrong school ID
  - **Expected**: 400 Bad Request, "School ID mismatch"

- [ ] **Test 903: Cross-School Data Leak**
  - **Action**: Enumerate student IDs from different school
  - **Expected**: 403 Forbidden or 404 Not Found (no data leaked)

---

## Input Validation Testing

### SQL Injection Testing

- [ ] **Test 1001: SQL Injection in Search Field**
  - **Action**: Search for students with `' OR '1'='1`
  - **Expected**: Safe handling, no SQL injection (EF Core parameterizes)

- [ ] **Test 1002: SQL Injection in Filter**
  - **Action**: Filter by name: `Robert'; DROP TABLE Students; --`
  - **Expected**: Treated as literal string, no SQL execution

- [ ] **Test 1003: Union-Based SQL Injection**
  - **Action**: Search: `' UNION SELECT * FROM Users --`
  - **Expected**: No data leak, safe query execution

### XSS (Cross-Site Scripting) Testing

- [ ] **Test 1101: Stored XSS in Chat Message**
  - **Action**: Send message: `<script>alert('XSS')</script>`
  - **Expected**: Script tags removed or encoded, no execution

- [ ] **Test 1102: Stored XSS in Announcement**
  - **Action**: Create announcement with `<img src=x onerror=alert(1)>`
  - **Expected**: Malicious tags removed, safe rendering

- [ ] **Test 1103: Reflected XSS in Search**
  - **Action**: Search with XSS payload in query parameter
  - **Expected**: Output encoded, no script execution

- [ ] **Test 1104: DOM-Based XSS**
  - **Action**: Inject XSS via URL fragment
  - **Expected**: Frontend sanitizes before rendering

- [ ] **Test 1105: XSS via SVG Upload**
  - **Action**: Upload SVG file with embedded JavaScript
  - **Expected**: SVG sanitized or rejected

### File Upload Validation

- [ ] **Test 1201: Valid Image Upload**
  - **Action**: Upload valid JPEG/PNG image (< 5 MB)
  - **Expected**: 200 OK, file uploaded

- [ ] **Test 1202: File Too Large**
  - **Action**: Upload file > 5 MB
  - **Expected**: 400 Bad Request, "File too large"

- [ ] **Test 1203: Invalid File Type**
  - **Action**: Upload .exe or .php file
  - **Expected**: 400 Bad Request, "Invalid file type"

- [ ] **Test 1204: File Extension Mismatch**
  - **Action**: Rename malicious.exe to image.jpg and upload
  - **Expected**: 400 Bad Request (magic number validation)

- [ ] **Test 1205: Path Traversal in Filename**
  - **Action**: Upload file named `../../etc/passwd.jpg`
  - **Expected**: Filename sanitized, no path traversal

- [ ] **Test 1206: XXE (XML External Entity) Attack**
  - **Action**: Upload XML file with external entity
  - **Expected**: XML parsing disabled or entity expansion prevented

### Input Length Validation

- [ ] **Test 1301: Excessive Name Length**
  - **Action**: Submit student with 500-character first name
  - **Expected**: 400 Bad Request, "First name too long (max 50)"

- [ ] **Test 1302: Excessive Message Length**
  - **Action**: Send chat message > 1000 characters
  - **Expected**: 400 Bad Request, "Message too long"

- [ ] **Test 1303: Email Length Validation**
  - **Action**: Submit email > 100 characters
  - **Expected**: 400 Bad Request

### Data Type Validation

- [ ] **Test 1401: Invalid GUID Format**
  - **Action**: Request `/api/Student/invalid-guid`
  - **Expected**: 400 Bad Request, "Invalid student ID format"

- [ ] **Test 1402: Invalid Enum Value**
  - **Action**: Submit gender as "InvalidValue"
  - **Expected**: 400 Bad Request, "Invalid gender value"

- [ ] **Test 1403: Future Date of Birth**
  - **Action**: Register student with DOB in future
  - **Expected**: 400 Bad Request, "Date of birth cannot be in the future"

- [ ] **Test 1404: Invalid Email Format**
  - **Action**: Register with email "notanemail"
  - **Expected**: 400 Bad Request, "Invalid email format"

---

## Session Management Testing

### Token Revocation

- [ ] **Test 1501: Token Revocation on Logout**
  - **Action**: Logout, then use same token
  - **Expected**: 401 Unauthorized

- [ ] **Test 1502: Token Revocation on Password Change**
  - **Action**: Change password, use old token
  - **Expected**: 401 Unauthorized (all tokens revoked)

- [ ] **Test 1503: Token Revocation on Role Change**
  - **Action**: Admin changes user role, user uses old token
  - **Expected**: 401 Unauthorized or 403 Forbidden

- [ ] **Test 1504: Blacklist Cleanup**
  - **Action**: Verify expired tokens removed from blacklist
  - **Expected**: Blacklist only contains non-expired entries

### Session Fixation

- [ ] **Test 1601: Session Fixation Attack**
  - **Action**: Set token before login, verify it changes after
  - **Expected**: New token issued on login (old one invalid)

---

## Encryption Testing

### Password Hashing

- [ ] **Test 1701: Password BCrypt Hashing**
  - **Action**: Register user and check database
  - **Expected**: Password stored as BCrypt hash (starts with `$2a$` or `$2b$`)

- [ ] **Test 1702: Same Password Different Hashes**
  - **Action**: Register two users with same password
  - **Expected**: Different hash values (salt used)

- [ ] **Test 1703: Password Never Logged**
  - **Action**: Check application logs after login
  - **Expected**: No plain-text passwords in logs

### Chat Message Encryption

- [ ] **Test 1801: Messages Encrypted at Rest**
  - **Action**: Send chat message and check database
  - **Expected**: Message content encrypted in database

- [ ] **Test 1802: Encrypted Message Decryption**
  - **Action**: Retrieve message via API
  - **Expected**: Message decrypted correctly for authorized users

- [ ] **Test 1803: Encryption Key Not Exposed**
  - **Action**: Check API responses and logs
  - **Expected**: Encryption keys never exposed

### TLS/HTTPS

- [ ] **Test 1901: HTTPS Enforced**
  - **Action**: Attempt HTTP request
  - **Expected**: Redirect to HTTPS (307 or 308)

- [ ] **Test 1902: TLS Version**
  - **Action**: Check SSL/TLS version
  - **Expected**: TLS 1.2 or TLS 1.3 only

- [ ] **Test 1903: Weak Cipher Suites**
  - **Action**: Test with SSL Labs or similar
  - **Expected**: No weak ciphers (RC4, DES, etc.)

---

## Rate Limiting Testing

### Authentication Rate Limiting

- [ ] **Test 2001: Login Rate Limit**
  - **Action**: Attempt 6 logins in 1 minute from same IP
  - **Expected**: 429 Too Many Requests after 5th attempt

- [ ] **Test 2002: Password Reset Rate Limit**
  - **Action**: Request 4 password resets in 1 hour
  - **Expected**: 429 Too Many Requests after 3rd attempt

- [ ] **Test 2003: Registration Rate Limit**
  - **Action**: Register 11 users in 1 hour from same IP
  - **Expected**: 429 Too Many Requests after 10th attempt

### API Rate Limiting

- [ ] **Test 2101: Authenticated API Rate Limit**
  - **Action**: Make 101 API requests in 1 minute
  - **Expected**: 429 Too Many Requests after 100th request

- [ ] **Test 2102: Rate Limit Headers**
  - **Action**: Check response headers
  - **Expected**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` present

- [ ] **Test 2103: Rate Limit Reset**
  - **Action**: Wait for rate limit window to expire
  - **Expected**: Counter resets, new requests allowed

### Chat Rate Limiting

- [ ] **Test 2201: Message Flood Protection**
  - **Action**: Send 31 messages in 1 minute
  - **Expected**: Error after 30th message, "Rate limit exceeded"

- [ ] **Test 2202: Per-User Rate Limit**
  - **Action**: Two users sending messages simultaneously
  - **Expected**: Each user has independent rate limit

---

## CORS Testing

### CORS Configuration

- [ ] **Test 2301: Allowed Origin**
  - **Action**: Send OPTIONS request from https://sms.edu
  - **Expected**: 200 OK with CORS headers

- [ ] **Test 2302: Disallowed Origin**
  - **Action**: Send OPTIONS request from https://evil.com
  - **Expected**: CORS headers absent or origin rejected

- [ ] **Test 2303: Credentials Allowed**
  - **Action**: Check `Access-Control-Allow-Credentials` header
  - **Expected**: Header present with value `true`

- [ ] **Test 2304: Preflight Caching**
  - **Action**: Check `Access-Control-Max-Age` header
  - **Expected**: Set to 86400 (24 hours)

- [ ] **Test 2305: Wildcard Origin in Production**
  - **Action**: Check production CORS config
  - **Expected**: No `*` wildcard, specific origins only

---

## Security Headers Testing

### Required Headers

- [ ] **Test 2401: X-Content-Type-Options**
  - **Action**: Check response headers
  - **Expected**: `X-Content-Type-Options: nosniff`

- [ ] **Test 2402: X-Frame-Options**
  - **Action**: Check response headers
  - **Expected**: `X-Frame-Options: DENY` or `SAMEORIGIN`

- [ ] **Test 2403: X-XSS-Protection**
  - **Action**: Check response headers
  - **Expected**: `X-XSS-Protection: 1; mode=block`

- [ ] **Test 2404: Strict-Transport-Security (HSTS)**
  - **Action**: Check HTTPS response headers
  - **Expected**: `Strict-Transport-Security: max-age=31536000; includeSubDomains`

- [ ] **Test 2405: Content-Security-Policy**
  - **Action**: Check response headers
  - **Expected**: CSP header present with restrictive policy

- [ ] **Test 2406: Referrer-Policy**
  - **Action**: Check response headers
  - **Expected**: `Referrer-Policy: strict-origin-when-cross-origin`

- [ ] **Test 2407: Permissions-Policy**
  - **Action**: Check response headers
  - **Expected**: `Permissions-Policy` restricting camera/microphone

### CSP Testing

- [ ] **Test 2501: Inline Script Blocked**
  - **Action**: Inject inline `<script>` tag
  - **Expected**: Browser blocks execution (CSP violation)

- [ ] **Test 2502: External Script from Untrusted Domain**
  - **Action**: Load script from non-whitelisted CDN
  - **Expected**: Blocked by CSP

- [ ] **Test 2503: CSP Violation Reporting**
  - **Action**: Trigger CSP violation
  - **Expected**: Violation reported to configured endpoint (if report-uri set)

---

## SignalR Security Testing

### Hub Authentication

- [ ] **Test 2601: Unauthenticated Hub Connection**
  - **Action**: Connect to /chatHub without token
  - **Expected**: Connection rejected, 401 Unauthorized

- [ ] **Test 2602: Valid Token in Query String**
  - **Action**: Connect with valid token in query string
  - **Expected**: Connection successful

- [ ] **Test 2603: Expired Token in Hub Connection**
  - **Action**: Connect with expired token
  - **Expected**: Connection rejected

### Hub Authorization

- [ ] **Test 2701: Unauthorized Room Access**
  - **Action**: Join chat room without access token
  - **Expected**: Hub method throws exception, "Access denied"

- [ ] **Test 2702: Invalid Room Access Token**
  - **Action**: Join room with forged access token
  - **Expected**: Exception thrown, user not added to room

- [ ] **Test 2703: Cross-Room Message Injection**
  - **Action**: Send message to Room A while in Room B
  - **Expected**: Message rejected or sent to wrong room prevented

### Hub Rate Limiting

- [ ] **Test 2801: Message Flood in Hub**
  - **Action**: Send 31 messages in 1 minute via SignalR
  - **Expected**: Exception after 30th message

---

## Compliance Testing

### FERPA Compliance

- [ ] **Test 2901: Student Data Access Logging**
  - **Action**: View student record
  - **Expected**: Access logged with user ID, timestamp, IP

- [ ] **Test 2902: Unauthorized Student Data Access**
  - **Action**: Attempt to access student from different school
  - **Expected**: 403 Forbidden, access attempt logged

- [ ] **Test 2903: Data Export with Consent**
  - **Action**: Export student data
  - **Expected**: Consent verified before export

### COPPA Compliance

- [ ] **Test 3001: Parental Consent for Under-13**
  - **Action**: Register student under 13 without parental consent
  - **Expected**: 400 Bad Request, "Parental consent required"

- [ ] **Test 3002: Valid Parental Consent**
  - **Action**: Register student under 13 with valid consent
  - **Expected**: Registration successful

### GDPR Compliance

- [ ] **Test 3101: Right to Access (GDPR Article 15)**
  - **Action**: Request all personal data via GDPR endpoint
  - **Expected**: 200 OK, all user data returned

- [ ] **Test 3102: Right to Erasure (GDPR Article 17)**
  - **Action**: Request data deletion
  - **Expected**: Data deleted or anonymized, confirmation sent

- [ ] **Test 3103: Data Portability (GDPR Article 20)**
  - **Action**: Export data in machine-readable format
  - **Expected**: JSON export with all user data

- [ ] **Test 3104: Consent Management**
  - **Action**: Withdraw consent for data processing
  - **Expected**: Processing stopped, data deleted per policy

---

## Summary

**Total Test Cases:** 151

**Test Coverage:**
- Authentication: 29 tests
- Authorization: 20 tests  
- Input Validation: 21 tests
- Session Management: 6 tests
- Encryption: 9 tests
- Rate Limiting: 12 tests
- CORS: 5 tests
- Security Headers: 10 tests
- SignalR: 8 tests
- Compliance: 7 tests

**Testing Frequency:**
- **Critical Tests**: Run on every deployment
- **High Priority**: Run weekly
- **Medium Priority**: Run monthly
- **Compliance Tests**: Run quarterly + before audits

**Recommended Tools:**
- OWASP ZAP or Burp Suite for security testing
- Postman/Newman for API testing
- k6 for load/rate limit testing
- Browser DevTools for header inspection
- SQL injection scanners

---

**Related Documentation:**
- [Security Implementation Guide](SECURITY_IMPLEMENTATION_GUIDE.md)
- [Penetration Testing Guide](PENETRATION_TESTING.md)
- [Test Strategy](../testing/TEST_STRATEGY.md)
