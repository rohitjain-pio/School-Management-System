# Compliance & Privacy
## Data Protection for Indian Schools

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 12 minutes  
**Status:** 🟡 In Progress

---

## 🎯 **Compliance Overview**

### **Applicable Regulations**

1. **Digital Personal Data Protection Act (DPDP), 2023** - India's primary data protection law
2. **Information Technology Act, 2000** - Cybersecurity obligations
3. **Right to Education Act, 2009** - Student data handling requirements
4. **Indian Evidence Act** - Electronic records admissibility
5. **State Education Board Regulations** - Varies by state (CBSE, ICSE, State Boards)

---

## 📜 **DPDP Act 2023 Compliance**

### **Key Requirements**

**1. Lawful Basis for Processing**

Student data processing is based on:
- **Consent:** Parents provide explicit consent during enrollment
- **Legitimate Interest:** School operations (attendance, grades, communication)
- **Legal Obligation:** Record-keeping mandated by education boards

**2. Data Minimization**

```typescript
// ✅ GOOD: Collect only necessary data
interface StudentRegistrationForm {
  firstName: string;           // Required
  lastName: string;            // Required
  dateOfBirth: Date;           // Required (age verification)
  parentEmail: string;         // Required (communication)
  parentPhone: string;         // Required (emergency contact)
  // Optional fields (collected only if needed)
  aadhaarNumber?: string;      // Optional, for govt schemes
  bloodGroup?: string;         // Optional, for medical emergencies
}

// ❌ BAD: Collect unnecessary data
interface StudentRegistrationForm {
  caste: string;               // Not required for basic operations
  religion: string;            // Discriminatory, not needed
  parentIncome: number;        // Privacy-invasive, rarely needed
}
```

**3. Purpose Limitation**

```csharp
/// <summary>
/// Student data can only be used for specified purposes.
/// </summary>
public enum DataProcessingPurpose
{
    AcademicManagement,      // Grades, attendance, assignments
    Communication,           // SMS, email to parents
    FeeManagement,          // Payment processing
    SafetyAndSecurity,      // Emergency contacts, health records
    LegalCompliance,        // Board exam submissions, government reports
    Analytics               // Aggregate reports (no PII)
}

// ❌ PROHIBITED: Using student data for marketing, third-party sales
```

**4. Data Retention Policy**

```sql
-- Retention periods as per DPDP Act
CREATE TABLE DataRetentionPolicy (
    DataType NVARCHAR(100) PRIMARY KEY,
    RetentionPeriod NVARCHAR(50),
    DeletionMethod NVARCHAR(100),
    LegalBasis NVARCHAR(200)
);

INSERT INTO DataRetentionPolicy VALUES
('Student Academic Records', '5 years after graduation', 'Soft delete, then purge', 'Board requirements'),
('Attendance Records', '3 years', 'Soft delete, then purge', 'Audit requirements'),
('Financial Records', '7 years', 'Archive, then purge', 'Income Tax Act'),
('Chat Messages', '1 year', 'Automatic deletion', 'Data minimization'),
('Audit Logs', '5 years', 'Archive to cold storage', 'Security compliance'),
('Parent Contact Info', 'Until student graduates + 1 year', 'Soft delete', 'Communication needs');
```

**5. Right to Access (Data Portability)**

```csharp
/// <summary>
/// Allows parents/students to download all their data.
/// Required by DPDP Act Section 11.
/// </summary>
[HttpGet("export")]
[Authorize(Roles = "Student,Parent")]
public async Task<IActionResult> ExportPersonalData()
{
    var userId = GetUserId();
    
    var exportData = new
    {
        PersonalInfo = await _userService.GetUserDataAsync(userId),
        AcademicRecords = await _studentService.GetAcademicHistoryAsync(userId),
        AttendanceRecords = await _attendanceService.GetAttendanceHistoryAsync(userId),
        GradeReports = await _gradeService.GetGradeHistoryAsync(userId),
        CommunicationHistory = await _chatService.GetMessageHistoryAsync(userId),
        ExportedAt = DateTime.UtcNow
    };
    
    var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
    var bytes = Encoding.UTF8.GetBytes(json);
    
    return File(bytes, "application/json", $"personal_data_{userId}_{DateTime.UtcNow:yyyyMMdd}.json");
}
```

**6. Right to Erasure ("Right to be Forgotten")**

```csharp
/// <summary>
/// Deletes user data upon request (subject to legal retention requirements).
/// DPDP Act Section 12.
/// </summary>
[HttpDelete("delete-account")]
[Authorize(Roles = "Parent")]
public async Task<IActionResult> RequestDataDeletion([FromBody] DataDeletionRequest request)
{
    var userId = GetUserId();
    
    // Check if retention period allows deletion
    var retentionStatus = await _complianceService.CheckRetentionRequirementsAsync(userId);
    
    if (retentionStatus.MustRetain)
    {
        return BadRequest(new
        {
            message = "Data cannot be deleted yet due to legal retention requirements.",
            retentionPeriod = retentionStatus.RemainingPeriod,
            reason = retentionStatus.LegalBasis
        });
    }
    
    // Schedule deletion (30-day grace period)
    await _dataProtectionService.ScheduleDeletionAsync(userId, DateTime.UtcNow.AddDays(30));
    
    _logger.LogWarning("User {UserId} requested data deletion. Scheduled for {DeletionDate}", 
        userId, DateTime.UtcNow.AddDays(30));
    
    return Ok(new
    {
        message = "Your data will be permanently deleted in 30 days. You can cancel this request during this period.",
        scheduledDeletion = DateTime.UtcNow.AddDays(30)
    });
}
```

**7. Data Breach Notification**

```csharp
/// <summary>
/// Notify Data Protection Board of India within 72 hours of breach discovery.
/// DPDP Act Section 8.
/// </summary>
public class DataBreachHandler
{
    public async Task HandleBreachAsync(DataBreachIncident incident)
    {
        // 1. Log incident
        _logger.LogCritical("DATA BREACH: {Description}. Affected users: {Count}", 
            incident.Description, incident.AffectedUserCount);
        
        // 2. Notify internal team immediately
        await _alertService.SendUrgentAlertAsync("CRITICAL: Data Breach Detected", incident);
        
        // 3. Assess impact
        var riskLevel = await _riskAssessment.EvaluateBreachAsync(incident);
        
        // 4. If high risk, notify Data Protection Board (within 72 hours)
        if (riskLevel >= RiskLevel.High)
        {
            await _complianceService.NotifyDataProtectionBoardAsync(new BreachNotification
            {
                IncidentDate = incident.DiscoveredAt,
                Description = incident.Description,
                AffectedDataTypes = incident.DataTypes,
                EstimatedAffectedUsers = incident.AffectedUserCount,
                MitigationSteps = incident.ResponseActions,
                NotificationDate = DateTime.UtcNow
            });
        }
        
        // 5. Notify affected users (email + SMS)
        if (incident.AffectedUserCount < 1000)  // Bulk notification if large-scale
        {
            foreach (var userId in incident.AffectedUsers)
            {
                await _notificationService.SendBreachNotificationAsync(userId, incident);
            }
        }
        
        // 6. Implement remediation
        await _securityService.ExecuteBreachResponsePlanAsync(incident);
    }
}
```

---

## 🔒 **Data Security Measures**

### **1. Encryption**

**Data at Rest:**
```csharp
// Database: Transparent Data Encryption (TDE)
-- Enable TDE on SQL Server
USE master;
GO

CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'StrongPassword@123!';
GO

CREATE CERTIFICATE TDECert WITH SUBJECT = 'TDE Certificate for School DB';
GO

USE SchoolManagementDB;
GO

CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE TDECert;
GO

ALTER DATABASE SchoolManagementDB
SET ENCRYPTION ON;
GO

// Sensitive fields: AES-256 encryption
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        
        return Convert.ToBase64String(msEncrypt.ToArray());
    }
    
    public string Decrypt(string cipherText)
    {
        // Decryption logic
    }
}

// Encrypt sensitive fields before saving
student.AadhaarNumber = _encryption.Encrypt(dto.AadhaarNumber);
student.MedicalHistory = _encryption.Encrypt(dto.MedicalHistory);
```

**Data in Transit:**
```nginx
# HTTPS only (TLS 1.3)
server {
    listen 443 ssl http2;
    ssl_certificate /etc/ssl/certs/schoolms.crt;
    ssl_certificate_key /etc/ssl/private/schoolms.key;
    
    ssl_protocols TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers on;
    
    # Force HTTPS redirect
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
}
```

### **2. Access Controls**

```csharp
// Role-based access to sensitive data
[Authorize(Roles = "Admin")]
[RequirePermission("ViewAadhaar")]
public async Task<IActionResult> GetStudentAadhaar(Guid studentId)
{
    // Audit log access to sensitive data
    await _auditService.LogSensitiveDataAccessAsync(new AuditLog
    {
        UserId = GetUserId(),
        Action = "ViewAadhaar",
        EntityId = studentId,
        Timestamp = DateTime.UtcNow,
        Severity = "Warning"
    });
    
    var student = await _studentService.GetStudentByIdAsync(studentId);
    ValidateSchoolOwnership(student.SchoolId, "Student");
    
    // Decrypt and return
    var aadhaar = _encryption.Decrypt(student.AadhaarNumber);
    
    return Ok(new { aadhaar });
}
```

### **3. Anonymization for Analytics**

```sql
-- Anonymized data for analytics (no PII)
CREATE VIEW StudentAnalytics_Anonymized AS
SELECT 
    LEFT(CAST(Id AS NVARCHAR(36)), 8) as StudentHash,  -- Partial hash instead of ID
    YEAR(DateOfBirth) as BirthYear,  -- Year only, not full DOB
    Gender,
    Class,
    AVG(AttendancePercentage) as AvgAttendance,
    AVG(GradePercentage) as AvgGrade,
    SchoolId
FROM Students
GROUP BY LEFT(CAST(Id AS NVARCHAR(36)), 8), YEAR(DateOfBirth), Gender, Class, SchoolId;
```

---

## 👶 **Minor Data Protection**

### **Parental Consent**

**Required for students under 18:**

```typescript
interface ParentalConsentForm {
  studentName: string;
  parentName: string;
  parentRelation: 'Mother' | 'Father' | 'Guardian';
  
  // Consent checkboxes
  consentToDataCollection: boolean;  // Academic records, attendance
  consentToCommunication: boolean;   // SMS, email notifications
  consentToPhotography: boolean;     // School events, website
  consentToThirdPartyServices: boolean;  // Payment gateways, SMS providers
  
  // Signature
  digitalSignature: string;
  signatureDate: Date;
  ipAddress: string;  // For audit trail
}

// Verify consent before processing minor's data
public async Task<bool> HasParentalConsentAsync(Guid studentId, DataProcessingPurpose purpose)
{
    var student = await _studentService.GetStudentByIdAsync(studentId);
    
    // Check if student is minor (under 18)
    if (CalculateAge(student.DateOfBirth) >= 18)
        return true;  // Adult, no parental consent needed
    
    // Check consent record
    var consent = await _consentService.GetConsentAsync(studentId);
    
    return purpose switch
    {
        DataProcessingPurpose.AcademicManagement => consent.ConsentToDataCollection,
        DataProcessingPurpose.Communication => consent.ConsentToCommunication,
        DataProcessingPurpose.Photography => consent.ConsentToPhotography,
        _ => false
    };
}
```

---

## 📋 **Privacy Policy (Template)**

**Must be displayed prominently and accepted during registration:**

```
PRIVACY POLICY - [School Name] Management System

Last Updated: January 13, 2026

1. DATA CONTROLLER
   [School Name], [Address]
   Contact: [Email], [Phone]
   Data Protection Officer: [Name]

2. DATA WE COLLECT
   Personal Information:
   - Student: Name, Date of Birth, Gender, Contact Details, Photo
   - Parent: Name, Relation, Contact Details, Aadhaar (optional)
   
   Academic Information:
   - Attendance records, Grades, Assignments, Exam results
   
   Technical Information:
   - Login times, IP addresses, Device information
   
3. PURPOSE OF PROCESSING
   We process your data to:
   - Manage academic operations (attendance, grades, exams)
   - Communicate important updates (SMS, email, notifications)
   - Ensure student safety and security
   - Comply with education board requirements
   - Generate reports for parents and authorities

4. LEGAL BASIS
   - Parental consent (for minors)
   - Contractual necessity (enrollment agreement)
   - Legal obligation (education board regulations)

5. DATA SHARING
   We may share data with:
   - Education Boards (CBSE, ICSE, State Boards) - for exam submissions
   - Government Authorities - upon legal request
   - Payment Processors (Razorpay) - for fee payments
   - SMS/Email Providers (Twilio, SendGrid) - for communication
   
   We DO NOT sell data to third parties for marketing.

6. DATA RETENTION
   - Academic records: 5 years after graduation
   - Financial records: 7 years
   - Communication logs: 1 year
   - Audit logs: 5 years

7. YOUR RIGHTS
   Under DPDP Act 2023, you have the right to:
   ✓ Access your data (download a copy)
   ✓ Correct inaccurate data
   ✓ Request deletion (subject to legal retention)
   ✓ Withdraw consent (may affect services)
   ✓ File complaint with Data Protection Board

8. SECURITY MEASURES
   - Data encrypted at rest (AES-256) and in transit (TLS 1.3)
   - Access controls based on roles
   - Regular security audits
   - 24/7 monitoring for breaches

9. INTERNATIONAL TRANSFERS
   Data is stored in India only. We do not transfer data outside India.

10. CONTACT US
    For privacy concerns:
    Email: privacy@[school].com
    Phone: [Phone Number]
    
    Data Protection Officer: [Name]
    Email: dpo@[school].com

By clicking "I Accept", you acknowledge that you have read and understood this Privacy Policy.
```

---

## 🌍 **GDPR Considerations (If Applicable)**

**For schools with European students/staff:**

**Key Differences from DPDP:**

| Requirement | DPDP Act (India) | GDPR (EU) |
|-------------|------------------|-----------|
| Consent Age | Under 18 | Under 16 (or 13-16 per member state) |
| DPO Required | For significant processing | Organizations > 250 employees |
| Breach Notification | 72 hours | 72 hours |
| Fines | ₹250 crores max | €20M or 4% revenue (whichever higher) |
| Data Localization | Encouraged | Not required |
| Right to Portability | Yes | Yes |

**GDPR-Specific Implementation:**

```csharp
// GDPR: Explicit consent tracking
public class GDPRConsentRecord
{
    public Guid UserId { get; set; }
    public string ConsentText { get; set; }  // Exact text shown to user
    public DateTime ConsentDate { get; set; }
    public string ConsentVersion { get; set; }  // Track policy changes
    public bool Accepted { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
}

// GDPR: Data Processing Agreement with third parties
public class ThirdPartyProcessor
{
    public string Name { get; set; }  // e.g., "Razorpay"
    public string Purpose { get; set; }  // "Payment processing"
    public List<string> DataShared { get; set; }  // ["Name", "Email", "Amount"]
    public string DataProtectionMeasures { get; set; }
    public DateTime AgreementDate { get; set; }
    public string AgreementUrl { get; set; }  // Link to DPA
}
```

---

## 📊 **Compliance Monitoring Dashboard**

**Track compliance metrics:**

```sql
-- Compliance Dashboard Queries

-- 1. Consent Coverage
SELECT 
    COUNT(*) as TotalMinors,
    SUM(CASE WHEN HasParentalConsent = 1 THEN 1 ELSE 0 END) as WithConsent,
    (SUM(CASE WHEN HasParentalConsent = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)) as ConsentPercentage
FROM Students
WHERE DATEDIFF(YEAR, DateOfBirth, GETDATE()) < 18;

-- 2. Data Retention Compliance
SELECT 
    DataType,
    COUNT(*) as RecordsCount,
    MIN(CreatedAt) as OldestRecord,
    MAX(CreatedAt) as NewestRecord,
    RetentionPeriod
FROM DataRecords dr
JOIN DataRetentionPolicy drp ON dr.Type = drp.DataType
WHERE dr.ShouldBeDeleted = 1
GROUP BY DataType, RetentionPeriod;

-- 3. Breach Incidents
SELECT 
    COUNT(*) as TotalBreaches,
    SUM(CASE WHEN NotifiedWithin72Hours = 1 THEN 1 ELSE 0 END) as CompliantNotifications,
    AVG(DATEDIFF(HOUR, DiscoveredAt, NotifiedAt)) as AvgNotificationTimeHours
FROM DataBreachIncidents
WHERE DiscoveredAt > DATEADD(YEAR, -1, GETDATE());

-- 4. Data Access Audit
SELECT 
    u.Email as UserEmail,
    COUNT(*) as SensitiveDataAccesses,
    STRING_AGG(al.Action, ', ') as Actions
FROM AuditLogs al
JOIN AspNetUsers u ON al.UserId = u.Id
WHERE al.Severity = 'Warning'
  AND al.Action IN ('ViewAadhaar', 'ViewMedicalHistory', 'ExportPersonalData')
  AND al.Timestamp > DATEADD(MONTH, -1, GETDATE())
GROUP BY u.Email
HAVING COUNT(*) > 50  -- Flag users with excessive access
ORDER BY COUNT(*) DESC;
```

---

## ✅ **Compliance Checklist**

**Before Production Launch:**

```
Legal Documentation:
[ ] Privacy Policy drafted and reviewed by lawyer
[ ] Terms of Service finalized
[ ] Parental consent forms prepared
[ ] Data Processing Agreements with third parties signed
[ ] DPO appointed (if required)

Technical Implementation:
[ ] Encryption enabled (TDE + field-level)
[ ] HTTPS enforced (TLS 1.3)
[ ] Access controls implemented (role-based)
[ ] Audit logging active (all sensitive data access)
[ ] Data retention policy automated

User Rights:
[ ] Data export functionality working
[ ] Data deletion workflow implemented
[ ] Consent management system active
[ ] Breach notification process documented

Training:
[ ] Staff trained on DPDP Act requirements
[ ] Data handling procedures documented
[ ] Incident response plan tested
[ ] DPO contact information publicized

Ongoing:
[ ] Quarterly privacy audits scheduled
[ ] Annual policy review planned
[ ] User consent re-validation (every 2 years)
[ ] Compliance dashboard monitored weekly
```

---

## 🚨 **Common Compliance Pitfalls**

**Avoid these mistakes:**

1. ❌ **Collecting unnecessary data** → Collect only what's needed
2. ❌ **No parental consent for minors** → Get explicit consent during enrollment
3. ❌ **Storing passwords in plain text** → Use BCrypt (work factor 12)
4. ❌ **Sharing data without user knowledge** → Disclose all third parties
5. ❌ **No data retention policy** → Automate deletion after retention period
6. ❌ **Ignoring data subject requests** → Respond within 30 days
7. ❌ **No breach response plan** → Notify within 72 hours
8. ❌ **Exporting user data to unencrypted files** → Always encrypt exports

---

## 📚 **Additional Resources**

- **DPDP Act 2023 Full Text:** https://www.meity.gov.in/writereaddata/files/Digital%20Personal%20Data%20Protection%20Act%202023.pdf
- **Data Protection Board (India):** https://www.dataprotection.gov.in/
- **CBSE Data Guidelines:** https://cbse.gov.in/
- **ISO 27001 Certification:** https://www.iso.org/isoiec-27001-information-security.html

---

**Document Status:** ✅ Complete  
**Legal Review:** 🔴 Required before production  
**Priority:** Mandatory compliance before handling student data