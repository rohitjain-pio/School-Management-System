# Database Schema
## Complete Database Design with Multi-Tenant Isolation

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 20 minutes  
**Status:** 🟡 Partially Implemented

---

## 🎯 **Schema Design Principles**

### **Core Principles**

1. **Multi-Tenant Isolation:** Every tenant-specific table has `SchoolId`
2. **Performance:** Strategic indexes on `SchoolId` and foreign keys
3. **Audit Trail:** Track all modifications with timestamps and user IDs
4. **Soft Deletes:** Never hard-delete, use `IsDeleted` flag
5. **Normalization:** 3NF (Third Normal Form) for data integrity
6. **Scalability:** Designed for 100+ schools, 100K+ students

---

## 📊 **Entity Relationship Diagram**

```
┌──────────────┐         ┌──────────────┐
│   Schools    │         │  AspNetUsers │
│──────────────│         │──────────────│
│ Id (PK)      │←────┐   │ Id (PK)      │
│ Name         │     │   │ Email        │
│ Address      │     │   │ PasswordHash │
│ Status       │     │   │ SchoolId (FK)│
└──────────────┘     │   │ Role         │
                     │   └──────────────┘
                     │
     ┌───────────────┼────────────────┬────────────────┐
     │               │                │                │
┌────▼─────┐   ┌────▼─────┐   ┌─────▼────┐   ┌──────▼──────┐
│ Students │   │ Teachers │   │ Parents  │   │   Classes   │
│──────────│   │──────────│   │──────────│   │─────────────│
│ Id (PK)  │   │ Id (PK)  │   │ Id (PK)  │   │ Id (PK)     │
│SchoolId  │   │SchoolId  │   │SchoolId  │   │ SchoolId    │
│ Name     │   │ Name     │   │ Name     │   │ Name        │
│ ClassId  │   │ Subject  │   │ Phone    │   │ TeacherId   │
└────┬─────┘   └──────────┘   └──────────┘   └─────────────┘
     │
     │         ┌──────────────┐         ┌──────────────┐
     └────────→│  Attendance  │         │    Grades    │
               │──────────────│         │──────────────│
               │ Id (PK)      │         │ Id (PK)      │
               │ SchoolId     │         │ SchoolId     │
               │ StudentId    │         │ StudentId    │
               │ Date         │         │ SubjectId    │
               │ Status       │         │ Marks        │
               └──────────────┘         └──────────────┘
```

---

## 🗄️ **Core Tables**

### **1. Schools (Platform-Wide)**

```sql
CREATE TABLE Schools (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    PinCode NVARCHAR(10),
    Phone NVARCHAR(15),
    Email NVARCHAR(100),
    Website NVARCHAR(200),
    
    -- Subscription
    SubscriptionStatus NVARCHAR(50) DEFAULT 'Active', -- Active, Suspended, OnHold, Deleted
    SubscriptionStartDate DATETIME2,
    SubscriptionEndDate DATETIME2,
    LastPaymentDate DATETIME2,
    GracePeriodEndDate DATETIME2, -- 30 days after due date
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy UNIQUEIDENTIFIER
);

CREATE INDEX IX_Schools_Status ON Schools(SubscriptionStatus) WHERE IsDeleted = 0;
CREATE INDEX IX_Schools_Email ON Schools(Email) WHERE IsDeleted = 0;
```

### **2. AspNetUsers (Identity - Modified)**

```sql
-- Part of ASP.NET Core Identity, extended with custom fields
ALTER TABLE AspNetUsers ADD SchoolId UNIQUEIDENTIFIER NULL;
ALTER TABLE AspNetUsers ADD CONSTRAINT FK_AspNetUsers_Schools 
    FOREIGN KEY (SchoolId) REFERENCES Schools(Id);

CREATE INDEX IX_AspNetUsers_SchoolId ON AspNetUsers(SchoolId);

-- Note: SchoolId can be NULL for:
-- 1. SuperAdmin (no specific school)
-- 2. Parents with multiple children in different schools
```

### **3. Students**

```sql
CREATE TABLE Students (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    -- Personal Info
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10), -- Male, Female, Other
    BloodGroup NVARCHAR(5),
    
    -- Contact
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    
    -- Academic
    AdmissionNumber NVARCHAR(50) NOT NULL,
    AdmissionDate DATE NOT NULL,
    ClassId UNIQUEIDENTIFIER,
    RollNumber NVARCHAR(20),
    
    -- Encrypted PII
    AadhaarNumber NVARCHAR(500), -- Encrypted
    FatherName NVARCHAR(100),
    FatherPhone NVARCHAR(500), -- Encrypted
    MotherName NVARCHAR(100),
    MotherPhone NVARCHAR(500), -- Encrypted
    MedicalHistory NVARCHAR(MAX), -- Encrypted
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy UNIQUEIDENTIFIER,
    
    CONSTRAINT FK_Students_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id),
    CONSTRAINT FK_Students_Classes FOREIGN KEY (ClassId) REFERENCES Classes(Id)
);

-- CRITICAL: Multi-tenant isolation index
CREATE INDEX IX_Students_SchoolId ON Students(SchoolId) 
    INCLUDE (FirstName, LastName, ClassId) 
    WHERE IsDeleted = 0;

CREATE INDEX IX_Students_ClassId ON Students(ClassId) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX IX_Students_AdmissionNumber 
    ON Students(SchoolId, AdmissionNumber) WHERE IsDeleted = 0;
```

### **4. Teachers**

```sql
CREATE TABLE Teachers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL, -- Link to AspNetUsers
    
    -- Personal Info
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    
    -- Contact
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15),
    Address NVARCHAR(500),
    
    -- Professional
    EmployeeCode NVARCHAR(50) NOT NULL,
    JoiningDate DATE NOT NULL,
    Qualification NVARCHAR(200),
    Specialization NVARCHAR(200),
    Experience INT, -- Years
    Salary DECIMAL(18,2),
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    
    CONSTRAINT FK_Teachers_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id),
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_Teachers_SchoolId ON Teachers(SchoolId) WHERE IsDeleted = 0;
CREATE INDEX IX_Teachers_UserId ON Teachers(UserId);
CREATE UNIQUE INDEX IX_Teachers_EmployeeCode 
    ON Teachers(SchoolId, EmployeeCode) WHERE IsDeleted = 0;
```

### **5. Classes**

```sql
CREATE TABLE Classes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    Name NVARCHAR(50) NOT NULL, -- "Class 10-A"
    Grade INT NOT NULL, -- 1-12
    Section NVARCHAR(10), -- A, B, C
    
    ClassTeacherId UNIQUEIDENTIFIER, -- Homeroom teacher
    MaxStudents INT DEFAULT 40,
    AcademicYear NVARCHAR(20), -- "2025-2026"
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    
    CONSTRAINT FK_Classes_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id),
    CONSTRAINT FK_Classes_Teachers FOREIGN KEY (ClassTeacherId) REFERENCES Teachers(Id)
);

CREATE INDEX IX_Classes_SchoolId ON Classes(SchoolId) WHERE IsDeleted = 0;
CREATE INDEX IX_Classes_Grade ON Classes(SchoolId, Grade) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX IX_Classes_Name 
    ON Classes(SchoolId, Name, AcademicYear) WHERE IsDeleted = 0;
```

### **6. Subjects**

```sql
CREATE TABLE Subjects (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    Name NVARCHAR(100) NOT NULL, -- "Mathematics", "Physics"
    Code NVARCHAR(20), -- "MATH101"
    Description NVARCHAR(500),
    Grade INT, -- Which grade (can be NULL for all grades)
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    
    CONSTRAINT FK_Subjects_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id)
);

CREATE INDEX IX_Subjects_SchoolId ON Subjects(SchoolId) WHERE IsDeleted = 0;
```

### **7. ClassSubjects (Many-to-Many)**

```sql
CREATE TABLE ClassSubjects (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassId UNIQUEIDENTIFIER NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER, -- Teacher assigned to this subject for this class
    
    CONSTRAINT FK_ClassSubjects_Classes FOREIGN KEY (ClassId) REFERENCES Classes(Id),
    CONSTRAINT FK_ClassSubjects_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
    CONSTRAINT FK_ClassSubjects_Teachers FOREIGN KEY (TeacherId) REFERENCES Teachers(Id)
);

CREATE UNIQUE INDEX IX_ClassSubjects_Unique 
    ON ClassSubjects(ClassId, SubjectId);
```

### **8. Attendance**

```sql
CREATE TABLE Attendance (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ClassId UNIQUEIDENTIFIER NOT NULL,
    Date DATE NOT NULL,
    
    Status NVARCHAR(20) NOT NULL, -- Present, Absent, Late, HalfDay, OnLeave
    Remarks NVARCHAR(500),
    
    -- Metadata
    MarkedAt DATETIME2 DEFAULT GETUTCDATE(),
    MarkedBy UNIQUEIDENTIFIER, -- Teacher who marked
    
    CONSTRAINT FK_Attendance_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id),
    CONSTRAINT FK_Attendance_Students FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Attendance_Classes FOREIGN KEY (ClassId) REFERENCES Classes(Id)
);

CREATE INDEX IX_Attendance_SchoolId_Date ON Attendance(SchoolId, Date);
CREATE INDEX IX_Attendance_StudentId_Date ON Attendance(StudentId, Date);
CREATE UNIQUE INDEX IX_Attendance_Unique 
    ON Attendance(StudentId, Date); -- One record per student per day
```

### **9. Grades**

```sql
CREATE TABLE Grades (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    
    ExamType NVARCHAR(50) NOT NULL, -- Quarterly, HalfYearly, Annual, Unit1, Unit2
    MaxMarks DECIMAL(5,2) NOT NULL,
    MarksObtained DECIMAL(5,2) NOT NULL,
    Grade NVARCHAR(5), -- A+, A, B+, etc.
    Remarks NVARCHAR(500),
    
    AcademicYear NVARCHAR(20), -- "2025-2026"
    Term NVARCHAR(20), -- "Term 1", "Term 2"
    
    -- Metadata
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    
    CONSTRAINT FK_Grades_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id),
    CONSTRAINT FK_Grades_Students FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Grades_Subjects FOREIGN KEY (SubjectId) REFERENCES Subjects(Id),
    CONSTRAINT CHK_Grades_Marks CHECK (MarksObtained <= MaxMarks AND MarksObtained >= 0)
);

CREATE INDEX IX_Grades_SchoolId ON Grades(SchoolId);
CREATE INDEX IX_Grades_StudentId ON Grades(StudentId);
CREATE UNIQUE INDEX IX_Grades_Unique 
    ON Grades(StudentId, SubjectId, ExamType, AcademicYear, Term);
```

---

## 💬 **Chat & Communication Tables**

### **10. ChatRooms**

```sql
CREATE TABLE ChatRooms (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SchoolId UNIQUEIDENTIFIER NOT NULL,
    
    Name NVARCHAR(100),
    Type NVARCHAR(50) NOT NULL, -- OneToOne, Group, Class, Announcement
    
    -- For role-based rooms
    AllowedRoles NVARCHAR(200), -- "Teacher,Parent" or NULL for all
    ClassId UNIQUEIDENTIFIER, -- If class-specific
    
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_ChatRooms_Schools FOREIGN KEY (SchoolId) REFERENCES Schools(Id)
);

CREATE INDEX IX_ChatRooms_SchoolId ON ChatRooms(SchoolId) WHERE IsActive = 1;
```

### **11. ChatMessages**

```sql
CREATE TABLE ChatMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoomId UNIQUEIDENTIFIER NOT NULL,
    
    SenderId UNIQUEIDENTIFIER NOT NULL,
    Message NVARCHAR(MAX) NOT NULL, -- Encrypted
    
    MessageType NVARCHAR(20) DEFAULT 'Text', -- Text, Image, File, System
    AttachmentUrl NVARCHAR(500),
    
    SentAt DATETIME2 DEFAULT GETUTCDATE(),
    IsEdited BIT DEFAULT 0,
    EditedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    DeletedAt DATETIME2,
    
    CONSTRAINT FK_ChatMessages_Rooms FOREIGN KEY (RoomId) REFERENCES ChatRooms(Id),
    CONSTRAINT FK_ChatMessages_Users FOREIGN KEY (SenderId) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_ChatMessages_RoomId_SentAt 
    ON ChatMessages(RoomId, SentAt DESC) WHERE IsDeleted = 0;
```

---

## 🔐 **Security Tables**

### **12. RefreshTokens**

```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedByIp NVARCHAR(50),
    
    RevokedAt DATETIME2,
    RevokedByIp NVARCHAR(50),
    ReplacedByToken NVARCHAR(500),
    ReasonRevoked NVARCHAR(200),
    
    IsExpired AS (CASE WHEN ExpiresAt < GETUTCDATE() THEN 1 ELSE 0 END),
    IsRevoked AS (CASE WHEN RevokedAt IS NOT NULL THEN 1 ELSE 0 END),
    IsActive AS (CASE WHEN RevokedAt IS NULL AND ExpiresAt > GETUTCDATE() THEN 1 ELSE 0 END),
    
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
```

### **13. AuditLogs**

```sql
CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY, -- Use BIGINT for high volume
    
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    UserId UNIQUEIDENTIFIER,
    SchoolId UNIQUEIDENTIFIER,
    
    Action NVARCHAR(100) NOT NULL, -- "CreateStudent", "DeleteClass", "SuperAdminAccess"
    EntityType NVARCHAR(100), -- "Student", "Class"
    EntityId UNIQUEIDENTIFIER,
    
    OldValues NVARCHAR(MAX), -- JSON
    NewValues NVARCHAR(MAX), -- JSON
    
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    
    Severity NVARCHAR(20) DEFAULT 'Info', -- Info, Warning, Critical
);

CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp DESC);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_SchoolId ON AuditLogs(SchoolId);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
```

---

## 📈 **Performance Optimization**

### **Index Strategy**

**1. SchoolId Indexes (CRITICAL):**
```sql
-- Every multi-tenant table MUST have this
CREATE INDEX IX_TableName_SchoolId ON TableName(SchoolId) 
    INCLUDE (frequently_queried_columns) 
    WHERE IsDeleted = 0;
```

**2. Composite Indexes:**
```sql
-- For queries like: WHERE SchoolId = @id AND ClassId = @classId
CREATE INDEX IX_Students_School_Class 
    ON Students(SchoolId, ClassId) 
    INCLUDE (FirstName, LastName, RollNumber)
    WHERE IsDeleted = 0;
```

**3. Date Range Queries:**
```sql
-- For attendance reports: WHERE SchoolId = @id AND Date BETWEEN @start AND @end
CREATE INDEX IX_Attendance_School_Date 
    ON Attendance(SchoolId, Date) 
    INCLUDE (StudentId, Status);
```

### **Query Optimization Examples**

**BAD (Table Scan):**
```sql
-- Missing SchoolId filter = scans all schools' data
SELECT * FROM Students WHERE FirstName = 'Raj';
```

**GOOD (Index Seek):**
```sql
-- Uses IX_Students_SchoolId index
SELECT * FROM Students 
WHERE SchoolId = @schoolId AND FirstName = 'Raj';
```

---

## 🔄 **Migration Scripts**

### **Add SchoolId to Existing Tables**

```sql
-- Step 1: Add column (nullable initially)
ALTER TABLE Students ADD SchoolId UNIQUEIDENTIFIER NULL;

-- Step 2: Create default school for existing data
DECLARE @DefaultSchoolId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Schools (Id, Name, SubscriptionStatus)
VALUES (@DefaultSchoolId, 'Default School (To Be Configured)', 'Active');

-- Step 3: Update existing records
UPDATE Students SET SchoolId = @DefaultSchoolId WHERE SchoolId IS NULL;

-- Step 4: Make column required
ALTER TABLE Students ALTER COLUMN SchoolId UNIQUEIDENTIFIER NOT NULL;

-- Step 5: Add foreign key
ALTER TABLE Students ADD CONSTRAINT FK_Students_Schools 
    FOREIGN KEY (SchoolId) REFERENCES Schools(Id);

-- Step 6: Add index
CREATE INDEX IX_Students_SchoolId ON Students(SchoolId) WHERE IsDeleted = 0;
```

---

## 📚 **Next Steps**

1. **API Design:** [05_API_ARCHITECTURE.md](./05_API_ARCHITECTURE.md)
2. **Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)
3. **Testing:** [12_TESTING_STRATEGY.md](./12_TESTING_STRATEGY.md)

---

**Document Status:** ✅ Complete  
**Implementation:** 🟡 Partially Complete (Tables exist, indexes need optimization)
