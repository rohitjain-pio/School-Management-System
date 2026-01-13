# Disaster Recovery
## Backup, Restore & Business Continuity

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 12 minutes  
**Status:** 🔴 Not Implemented (CRITICAL)

---

## 🎯 **Recovery Objectives**

### **RPO & RTO Targets**

| Scenario | RPO (Data Loss) | RTO (Downtime) | Priority |
|----------|-----------------|----------------|----------|
| **Database Corruption** | 15 minutes | 1 hour | 🔴 Critical |
| **Accidental Deletion** | 24 hours | 2 hours | 🟠 High |
| **Regional Outage** | 1 hour | 4 hours | 🟠 High |
| **Ransomware Attack** | 24 hours | 8 hours | 🟡 Medium |
| **Complete Data Center Loss** | 1 hour | 24 hours | 🟢 Low |

**RPO (Recovery Point Objective):** Maximum acceptable data loss  
**RTO (Recovery Time Objective):** Maximum acceptable downtime

---

## 💾 **Backup Strategy**

### **1. Database Backups**

**Automated Backup Schedule:**

```sql
-- Full Backup (Daily at 2 AM)
BACKUP DATABASE SchoolManagementDB
TO DISK = 'D:\Backups\Full\SchoolDB_Full_20260113.bak'
WITH COMPRESSION, STATS = 10;

-- Differential Backup (Every 6 hours)
BACKUP DATABASE SchoolManagementDB
TO DISK = 'D:\Backups\Diff\SchoolDB_Diff_20260113_0800.bak'
WITH DIFFERENTIAL, COMPRESSION;

-- Transaction Log Backup (Every 15 minutes)
BACKUP LOG SchoolManagementDB
TO DISK = 'D:\Backups\Log\SchoolDB_Log_20260113_0815.trn'
WITH COMPRESSION;
```

**Retention Policy:**
- **Full Backups:** 30 days
- **Differential Backups:** 7 days
- **Log Backups:** 7 days
- **Monthly Archives:** 1 year (for compliance)

### **2. Azure SQL Database Automated Backups**

**Built-in Protection:**
```
┌─────────────────────────────────────┐
│   Azure SQL Database Backups        │
├─────────────────────────────────────┤
│ • Full backup: Weekly (automated)   │
│ • Differential: Every 12-24 hours   │
│ • Log backup: Every 5-10 minutes    │
│                                     │
│ Point-in-Time Restore:              │
│ • Any time within last 7-35 days    │
│ • Granularity: Down to the second   │
│                                     │
│ Geo-Redundant:                      │
│ • Replicated to paired region       │
│ • West India → South India          │
└─────────────────────────────────────┘
```

**Enable Long-Term Retention:**
```powershell
# PowerShell command
Set-AzSqlDatabaseBackupLongTermRetentionPolicy `
    -ResourceGroupName "schoolmanagement-rg" `
    -ServerName "schoolmanagement-sqlserver" `
    -DatabaseName "SchoolManagementDB" `
    -WeeklyRetention "P4W" `   # 4 weeks
    -MonthlyRetention "P12M" `  # 12 months
    -YearlyRetention "P3Y" `    # 3 years
    -WeekOfYear 1
```

### **3. File Storage Backups**

**Azure Blob Storage Versioning:**

```csharp
// Enable blob versioning
BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
BlobServiceProperties properties = await blobServiceClient.GetPropertiesAsync();
properties.IsVersioningEnabled = true;
await blobServiceClient.SetPropertiesAsync(properties);

// Soft delete (30 days retention)
properties.DeleteRetentionPolicy = new RetentionPolicy
{
    Enabled = true,
    Days = 30
};
```

**Backup to Secondary Storage:**
```bash
# Azure CLI - Copy blobs to backup storage account
az storage blob copy start-batch \
    --source-container school-files \
    --source-account-name schoolmanagementstorage \
    --destination-container backup-files \
    --destination-account-name schoolmanagementbackup \
    --pattern "*"
```

---

## 🔄 **Recovery Procedures**

### **Scenario 1: Accidental Data Deletion**

**Problem:** Admin accidentally deleted all students in Class 10-A

**Recovery Steps:**

```sql
-- 1. Verify soft-delete records
SELECT * FROM Students 
WHERE IsDeleted = 1 
  AND DeletedAt > DATEADD(hour, -1, GETUTCDATE());

-- 2. Restore soft-deleted records
UPDATE Students
SET IsDeleted = 0,
    DeletedAt = NULL,
    DeletedBy = NULL
WHERE ClassId = @class10AId
  AND IsDeleted = 1
  AND DeletedAt > DATEADD(hour, -1, GETUTCDATE());

-- 3. Verify restoration
SELECT COUNT(*) FROM Students WHERE ClassId = @class10AId AND IsDeleted = 0;
```

**Time:** 5-10 minutes  
**Data Loss:** None (soft delete)

### **Scenario 2: Database Corruption**

**Problem:** Database files corrupted, won't start

**Recovery Steps:**

```sql
-- 1. Restore latest full backup
RESTORE DATABASE SchoolManagementDB
FROM DISK = 'D:\Backups\Full\SchoolDB_Full_20260113.bak'
WITH NORECOVERY, REPLACE, STATS = 10;

-- 2. Restore latest differential backup
RESTORE DATABASE SchoolManagementDB
FROM DISK = 'D:\Backups\Diff\SchoolDB_Diff_20260113_1800.bak'
WITH NORECOVERY, STATS = 10;

-- 3. Restore all log backups since differential
RESTORE LOG SchoolManagementDB
FROM DISK = 'D:\Backups\Log\SchoolDB_Log_20260113_1815.trn'
WITH NORECOVERY;

RESTORE LOG SchoolManagementDB
FROM DISK = 'D:\Backups\Log\SchoolDB_Log_20260113_1830.trn'
WITH RECOVERY;

-- 4. Verify database integrity
DBCC CHECKDB(SchoolManagementDB) WITH NO_INFOMSGS;

-- 5. Bring database online
ALTER DATABASE SchoolManagementDB SET ONLINE;
```

**Time:** 30-60 minutes  
**Data Loss:** Max 15 minutes (last log backup)

### **Scenario 3: Point-in-Time Restore (Azure)**

**Problem:** Need to restore database to yesterday 3:00 PM

**Azure Portal:**
1. Go to Azure SQL Database
2. Click "Restore"
3. Select restore point: 2026-01-12 15:00:00
4. New database name: SchoolManagementDB_Restored
5. Click "Restore"

**PowerShell:**
```powershell
Restore-AzSqlDatabase `
    -ResourceGroupName "schoolmanagement-rg" `
    -ServerName "schoolmanagement-sqlserver" `
    -TargetDatabaseName "SchoolManagementDB_Restored" `
    -Edition "Standard" `
    -ServiceObjectiveName "S2" `
    -ResourceId "/subscriptions/.../SchoolManagementDB" `
    -PointInTime "2026-01-12T15:00:00Z"
```

**Time:** 10-30 minutes  
**Data Loss:** None (exact point in time)

---

## 🌍 **Disaster Recovery Scenarios**

### **Scenario 4: Regional Outage (West India Down)**

**Problem:** Azure West India datacenter is down

**Failover to South India:**

```
┌─────────────────────────────────────┐
│   PRIMARY REGION (West India) ❌    │
│   • App Service: DOWN               │
│   • SQL Database: DOWN              │
│   • Storage: DOWN                   │
└─────────────────────────────────────┘
              │
              │ Automatic Failover
              ▼
┌─────────────────────────────────────┐
│  SECONDARY REGION (South India) ✅  │
│   • App Service: Standby→Active     │
│   • SQL Database: Geo-Replica→Primary│
│   • Storage: GRS→Available          │
└─────────────────────────────────────┘
```

**Steps:**

1. **Database Failover:**
```powershell
# Initiate manual failover
Switch-AzSqlDatabaseFailoverGroup `
    -ResourceGroupName "schoolmanagement-rg" `
    -ServerName "schoolmanagement-sqlserver-secondary" `
    -FailoverGroupName "schoolmanagement-fg"
```

2. **Update DNS:**
```powershell
# Point app to secondary region
az network dns record-set a update `
    --resource-group schoolmanagement-rg `
    --zone-name schoolms.com `
    --name api `
    --set aRecords[0].ipv4Address=52.172.x.x  # Secondary IP
```

3. **Verify Application:**
```bash
curl https://api.schoolms.com/health
# Should return 200 OK from secondary region
```

**Time:** 15-30 minutes  
**Data Loss:** < 1 hour (geo-replication lag)

### **Scenario 5: Ransomware Attack**

**Problem:** Database encrypted by ransomware, ransom demanded

**DO NOT PAY RANSOM! Follow recovery plan:**

**Immediate Actions (First 30 minutes):**

```bash
# 1. Isolate infected systems
# Disconnect from network, disable services

# 2. Assess damage
# Check which systems are encrypted

# 3. Notify authorities
# Report to cybercrime department

# 4. Notify customers (if data breached)
# Email/SMS to all schools
```

**Recovery (Next 4 hours):**

```sql
-- 1. Restore from backup BEFORE infection
-- Identify last known good backup (2 days ago)
RESTORE DATABASE SchoolManagementDB
FROM DISK = 'D:\Backups\Full\SchoolDB_Full_20260111.bak'
WITH REPLACE, RECOVERY;

-- 2. Apply recent backups (if clean)
-- Carefully restore log backups, checking for malicious activity

-- 3. Verify data integrity
DBCC CHECKDB(SchoolManagementDB);

-- 4. Reset all passwords
UPDATE AspNetUsers SET PasswordHash = @newHashedPassword;

-- 5. Revoke all refresh tokens
DELETE FROM RefreshTokens;

-- 6. Force password reset for all users
UPDATE AspNetUsers SET MustChangePassword = 1;
```

**Post-Recovery:**

```bash
# 1. Patch all systems
apt update && apt upgrade -y

# 2. Install antivirus/EDR
# Deploy endpoint protection

# 3. Enable additional monitoring
# Audit all database changes

# 4. Train staff
# Security awareness training
```

**Time:** 4-8 hours  
**Data Loss:** Up to 48 hours (last clean backup)

---

## 🔐 **Backup Security**

### **Encrypted Backups**

```sql
-- Create master key
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'StrongPassword123!';

-- Create certificate
CREATE CERTIFICATE BackupCert 
WITH SUBJECT = 'Backup Certificate';

-- Encrypted backup
BACKUP DATABASE SchoolManagementDB
TO DISK = 'D:\Backups\SchoolDB_Encrypted.bak'
WITH COMPRESSION,
     ENCRYPTION (
         ALGORITHM = AES_256,
         SERVER CERTIFICATE = BackupCert
     );
```

### **Off-Site Storage**

**Azure Backup to Different Region:**

```bash
# Copy backups to geo-redundant storage
az storage blob upload-batch \
    --source D:\Backups \
    --destination backup-container \
    --account-name schoolmanagementbackup \
    --account-key $STORAGE_KEY \
    --pattern "*.bak"
```

**AWS S3 (Alternative Off-Site):**

```bash
# Upload to AWS S3 for geographic diversity
aws s3 sync D:\Backups s3://schoolmanagement-backups \
    --storage-class GLACIER \
    --sse AES256
```

---

## 🧪 **Backup Testing**

### **Monthly Restore Test**

**Procedure:**

```bash
# 1. Restore to test environment
RESTORE DATABASE SchoolManagementDB_Test
FROM DISK = 'D:\Backups\Full\SchoolDB_Full_20260113.bak'
WITH MOVE 'SchoolManagementDB_Data' TO 'D:\TestDB\SchoolDB_Data.mdf',
     MOVE 'SchoolManagementDB_Log' TO 'D:\TestDB\SchoolDB_Log.ldf';

# 2. Verify data integrity
DBCC CHECKDB(SchoolManagementDB_Test);

# 3. Run smoke tests
SELECT COUNT(*) FROM Schools; -- Should match production
SELECT COUNT(*) FROM Students WHERE IsDeleted = 0;
SELECT COUNT(*) FROM Teachers WHERE IsDeleted = 0;

# 4. Test application connection
# Update connection string to test DB
# Run automated tests

# 5. Document results
# Log: "Backup restore test successful - 45 minutes"

# 6. Clean up
DROP DATABASE SchoolManagementDB_Test;
```

**Schedule:** First Monday of every month at 2 AM

---

## 📋 **Disaster Recovery Plan Template**

### **Emergency Contact List**

| Role | Name | Phone | Email |
|------|------|-------|-------|
| **SuperAdmin** | (Your name) | +91-XXXXX-XXXXX | admin@schoolms.com |
| **Database Admin** | (DBA if hired) | +91-XXXXX-XXXXX | dba@schoolms.com |
| **Azure Support** | Microsoft | +91-80-46526000 | azure-support@microsoft.com |
| **Incident Commander** | (You) | +91-XXXXX-XXXXX | emergency@schoolms.com |

### **Communication Templates**

**To Schools (Outage Notification):**
```
Subject: Service Disruption - [Date] [Time]

Dear School Administrators,

We are currently experiencing a service disruption affecting the School Management System. Our team is actively working to resolve the issue.

Current Status: [Brief description]
Expected Resolution: [Time estimate]
Workaround: [If available]

We apologize for the inconvenience. Updates will be provided every 30 minutes.

For urgent queries: emergency@schoolms.com
```

**To Schools (Recovery Complete):**
```
Subject: Service Restored - [Date] [Time]

Dear School Administrators,

The School Management System has been fully restored and is now operational.

Downtime Duration: [X hours Y minutes]
Data Loss: [None / Details]
Action Required: [None / Password reset / etc.]

Thank you for your patience.
```

---

## ✅ **Disaster Recovery Checklist**

**Preparation:**
- [ ] Automated backups configured (daily full, hourly log)
- [ ] Geo-replication enabled (West India → South India)
- [ ] Long-term retention configured (monthly for 1 year)
- [ ] File storage versioning enabled
- [ ] Backup encryption enabled
- [ ] Off-site backup copies (different cloud provider)
- [ ] Backup restore tested (monthly)
- [ ] Recovery procedures documented
- [ ] Emergency contacts updated
- [ ] Communication templates prepared

**During Incident:**
- [ ] Assess severity and impact
- [ ] Activate disaster recovery plan
- [ ] Notify incident commander
- [ ] Isolate affected systems (if security incident)
- [ ] Communicate to customers (every 30 min)
- [ ] Begin recovery procedures
- [ ] Document all actions taken
- [ ] Verify recovery success
- [ ] Conduct post-incident review

**Post-Incident:**
- [ ] Root cause analysis completed
- [ ] Corrective actions identified
- [ ] Update DR plan based on lessons learned
- [ ] Train team on improvements
- [ ] Update incident log

---

## 📚 **Next Steps**

1. **Monitoring:** [17_MONITORING_LOGGING.md](./17_MONITORING_LOGGING.md)
2. **Compliance:** [15_COMPLIANCE_PRIVACY.md](./15_COMPLIANCE_PRIVACY.md)
3. **Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)

---

**Document Status:** ✅ Complete  
**Implementation:** 🔴 NOT IMPLEMENTED (CRITICAL)  
**Priority:** Implement automated backups before production launch
