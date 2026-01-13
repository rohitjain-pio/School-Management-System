# User Workflows
## Journey Maps for Each User Role

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 15 minutes  
**Status:** ✅ Current

---

## 🎯 **Purpose**

This document maps out complete user journeys for each role, from login to typical daily tasks. Use this to understand user needs and design intuitive interfaces.

---

## 👑 **SuperAdmin Workflows**

### **Daily Routine**

```
8:00 AM  → Login to SuperAdmin Dashboard
8:05 AM  → Check platform statistics (schools active, total users, errors)
8:15 AM  → Review overnight alerts (if any)
8:30 AM  → Process new school signup request
9:00 AM  → Onboard new school
10:00 AM → Investigate support ticket (School X can't see students)
11:00 AM → Review billing reports
12:00 PM → Lunch
1:00 PM  → Update platform settings
2:00 PM  → Monitor system performance
3:00 PM  → Review audit logs for security
4:00 PM  → Plan feature rollout
5:00 PM  → End of day summary
```

### **Workflow 1: Onboard New School**

```
Step 1: SuperAdmin Dashboard
  ↓
Step 2: Click "Add New School"
  ↓
Step 3: Fill School Details Form
  - School Name
  - Address, City, State, Pin Code
  - Contact Phone, Email
  - Admin Name & Email
  ↓
Step 4: Submit Form
  ↓
Step 5: System Creates:
  - School record in database
  - Admin user account
  - Default settings for school
  - Email sent to admin with credentials
  ↓
Step 6: Confirmation Screen
  - School ID
  - Admin credentials (temporary password)
  - Status: Active
  ↓
Step 7: SuperAdmin notifies school principal
```

**Time:** < 5 minutes  
**API Calls:**
- POST /api/schools
- POST /api/users (create admin)
- POST /api/email/send-credentials

### **Workflow 2: Support Investigation**

```
Scenario: School reports "Can't see any students"

Step 1: Ticket arrives via email/chat
  ↓
Step 2: Login to SuperAdmin Dashboard
  ↓
Step 3: Navigate to "All Schools" → Search for school
  ↓
Step 4: Click "Silent Access" button
  ↓
Step 5: System logs: "SuperAdmin accessed School X at [timestamp]"
  ↓
Step 6: SuperAdmin sees school's view (students, classes, etc.)
  ↓
Step 7: Investigate:
  - Check filters (Are students deleted? Wrong class filter?)
  - Check user permissions
  - Check database (SchoolId correct?)
  ↓
Step 8: Find issue: Admin applied "Grade 12" filter, school has no Grade 12
  ↓
Step 9: Reset filter or guide admin
  ↓
Step 10: Document resolution in ticket
  ↓
Step 11: Logout from school view
```

**Time:** 10-30 minutes  
**Critical:** All SuperAdmin access logged in AuditLogs table

---

## 🏫 **School Admin Workflows**

### **Daily Routine**

```
7:30 AM  → Login to school dashboard
7:35 AM  → Check today's announcements
7:40 AM  → Review attendance summary (who's absent today)
8:00 AM  → School starts - monitor live attendance marking
9:00 AM  → Approve leave requests
10:00 AM → Add 5 new students (admissions)
11:00 AM → Assign students to classes
12:00 PM → Generate monthly attendance report
1:00 PM  → Lunch
2:00 PM  → Create new teacher account
3:00 PM  → Update class schedules
4:00 PM  → Review today's activities
4:30 PM  → Logout
```

### **Workflow 1: Bulk Student Import**

```
Step 1: Admin Dashboard → "Students" → "Bulk Import"
  ↓
Step 2: Download CSV Template
  ↓
Step 3: Fill template in Excel:
  FirstName, LastName, DOB, Gender, Class, Section, ...
  Raj, Kumar, 2010-05-15, Male, 10, A, ...
  Priya, Sharma, 2010-07-20, Female, 10, A, ...
  (50 rows)
  ↓
Step 4: Upload CSV file
  ↓
Step 5: System validates:
  - Required fields present?
  - Valid dates?
  - Classes exist?
  - Duplicate admission numbers?
  ↓
Step 6: Preview screen shows:
  ✅ 45 valid records
  ⚠️ 3 warnings (missing phone numbers)
  ❌ 2 errors (invalid dates)
  ↓
Step 7: Admin fixes errors in CSV, re-uploads
  ↓
Step 8: All valid → Click "Import"
  ↓
Step 9: Progress bar: "Importing 45 students..."
  ↓
Step 10: Success message:
  "45 students imported successfully"
  "Credentials sent to parents via SMS/Email"
```

**Time:** 5-10 minutes for 50 students  
**API:** POST /api/students/bulk-import

### **Workflow 2: Create New Teacher**

```
Step 1: Admin Dashboard → "Teachers" → "Add Teacher"
  ↓
Step 2: Fill Teacher Form:
  - Personal: Name, DOB, Gender, Phone, Email
  - Professional: Employee Code, Joining Date, Qualification
  - Subjects: Mathematics, Physics
  - Classes: 10-A, 10-B
  ↓
Step 3: Submit
  ↓
Step 4: System:
  - Creates Teacher record
  - Creates User account (Teacher role)
  - Generates temporary password
  - Sends credentials to teacher's email
  ↓
Step 5: Confirmation: "Teacher created. Login sent to email."
  ↓
Step 6: Teacher receives email:
  "Welcome to XYZ School!
   Login: teacher@xyzschool.com
   Password: TempPass123! (change on first login)"
```

**Time:** 2-3 minutes  
**API:** POST /api/teachers

---

## 👨‍🏫 **Teacher Workflows**

### **Daily Routine**

```
8:00 AM  → Login
8:05 AM  → Check today's schedule (which classes)
8:15 AM  → Review yesterday's attendance (any pending?)
8:30 AM  → Class 10-A starts → Mark attendance
9:30 AM  → Class 10-B starts → Mark attendance
10:30 AM → Break
11:00 AM → Enter grades for Unit Test 1 (Mathematics)
12:00 PM → Respond to parent messages in chat
1:00 PM  → Lunch
2:00 PM  → Class 11-A → Mark attendance
3:00 PM  → Post announcement: "Homework due Friday"
3:30 PM  → Review student performance analytics
4:00 PM  → Logout
```

### **Workflow 1: Mark Attendance (Bulk)**

```
Step 1: Teacher Dashboard → "Attendance" → "Mark Attendance"
  ↓
Step 2: Select:
  - Date: Today (2026-01-13)
  - Class: 10-A
  ↓
Step 3: Student list appears (40 students):
  
  ┌────────────────────────────────────────┐
  │ Roll  Name           Status   Remarks  │
  ├────────────────────────────────────────┤
  │  1    Raj Kumar      [Present] [____]  │
  │  2    Priya Sharma   [Present] [____]  │
  │  3    Amit Patel     [Absent]  [Sick]  │
  │  4    Sneha Gupta    [Present] [____]  │
  │ ...                                     │
  └────────────────────────────────────────┘
  
  Quick Actions:
  [Mark All Present] [Mark Selected Absent]
  ↓
Step 4: Teacher clicks "Mark All Present"
  ↓
Step 5: Manually change status for absent students:
  - Roll 3: Absent (Remarks: "Sick leave")
  - Roll 15: Late (Remarks: "Bus delay")
  ↓
Step 6: Click "Submit Attendance"
  ↓
Step 7: Confirmation: "Attendance marked for 40 students"
  ↓
Step 8: SMS sent to parents of absent students
```

**Time:** 2-3 minutes per class  
**API:** POST /api/attendance (bulk)

### **Workflow 2: Enter Grades**

```
Step 1: Teacher Dashboard → "Grades" → "Enter Grades"
  ↓
Step 2: Select:
  - Class: 10-A
  - Subject: Mathematics
  - Exam: Unit Test 1
  - Max Marks: 50
  ↓
Step 3: Student list with input fields:
  
  ┌──────────────────────────────────────┐
  │ Roll  Name           Marks (out of 50)│
  ├──────────────────────────────────────┤
  │  1    Raj Kumar      [45__]    A+     │
  │  2    Priya Sharma   [48__]    A+     │
  │  3    Amit Patel     [35__]    B      │
  │  4    Sneha Gupta    [42__]    A      │
  │ ...                                   │
  └──────────────────────────────────────┘
  
  Auto-calculate grade based on marks
  ↓
Step 4: Enter marks for all 40 students
  ↓
Step 5: System validates:
  ✅ All marks ≤ max marks
  ✅ No negative marks
  ⚠️ Warning: 3 students scored < 35% (below passing)
  ↓
Step 6: Click "Submit Grades"
  ↓
Step 7: Confirmation: "Grades entered for 40 students"
  ↓
Step 8: Notification sent to students and parents
```

**Time:** 5-10 minutes for 40 students  
**API:** POST /api/grades (bulk)

---

## 🎓 **Student Workflows**

### **Daily Routine**

```
7:30 AM  → Login on phone/laptop
7:35 AM  → Check today's schedule (classes, assignments)
8:00 AM  → School starts (in-person)
3:30 PM  → School ends
4:00 PM  → Login again → Check attendance (was I marked present?)
4:05 PM  → Check grades (any new results?)
4:10 PM  → View homework assignments
4:30 PM  → Chat with teacher: "Doubt in Chapter 5"
5:00 PM  → Logout
```

### **Workflow 1: View Grades**

```
Step 1: Student Dashboard → "My Grades"
  ↓
Step 2: Grade Summary appears:
  
  ┌──────────────────────────────────────────┐
  │ Subject      Exam Type    Marks   Grade  │
  ├──────────────────────────────────────────┤
  │ Mathematics  Unit Test 1  45/50   A+     │
  │ Physics      Unit Test 1  38/50   B+     │
  │ Chemistry    Unit Test 1  42/50   A      │
  │ English      Unit Test 1  40/50   A      │
  │                                           │
  │ Overall Average: 82.5%    Grade: A       │
  └──────────────────────────────────────────┘
  
  ↓
Step 3: Click on subject for detailed view
  ↓
Step 4: See breakdown:
  - Unit Test 1: 45/50
  - Assignment 1: 18/20
  - Project: 28/30
  - Teacher Remarks: "Excellent work. Keep it up!"
```

**Time:** 1 minute  
**API:** GET /api/students/my-grades

### **Workflow 2: Check Attendance**

```
Step 1: Student Dashboard → "My Attendance"
  ↓
Step 2: Calendar view (current month):
  
  Jan 2026
  S  M  T  W  T  F  S
           1  2  3  4
  5  6  7  8  9 10 11
 12 ✅ ✅ ✅ ✅ ✅ 18
 19 ✅ ✅ ❌ ✅ ✅ 25
 26 ✅ ✅ ✅
  
  ✅ Present  ❌ Absent  ⏰ Late  📅 Holiday
  
  ↓
Step 3: Summary:
  - Total School Days: 20
  - Days Present: 18
  - Days Absent: 2 (Jan 21 - Sick, Jan 29 - Family function)
  - Attendance %: 90%
```

**Time:** 30 seconds  
**API:** GET /api/students/my-attendance

---

## 👨‍👩‍👧 **Parent Workflows**

### **Daily Routine**

```
7:00 AM  → Wake up
7:30 AM  → Login to check if child reached school (future: GPS tracking)
9:00 AM  → Notification: "Raj marked present in Class 10-A"
12:00 PM → Check lunch menu (future feature)
3:30 PM  → School ends
4:00 PM  → Login → Check if any announcements
4:15 PM  → View today's homework
5:00 PM  → Chat with teacher: "Raj having difficulty in Math Chapter 5"
7:00 PM  → Help child with homework
10:00 PM → Logout
```

### **Workflow 1: View Child's Progress**

```
Step 1: Parent Dashboard (shows all children)
  
  ┌───────────────────────────────────────┐
  │ My Children:                          │
  │                                       │
  │ 👦 Raj Kumar (Class 10-A)             │
  │    Attendance: 90%  |  Average: 82%   │
  │    [View Details]                     │
  │                                       │
  │ 👧 Riya Kumar (Class 7-B)             │
  │    Attendance: 95%  |  Average: 88%   │
  │    [View Details]                     │
  └───────────────────────────────────────┘
  
  ↓
Step 2: Click "View Details" for Raj
  ↓
Step 3: Detailed Dashboard:
  
  Attendance (This Month): 90%
  ┌─────────────────────────────┐
  │ Present: 18 days            │
  │ Absent: 2 days              │
  │ Late: 0 days                │
  └─────────────────────────────┘
  
  Recent Grades:
  ┌─────────────────────────────┐
  │ Math:      45/50  A+        │
  │ Physics:   38/50  B+  ⚠️    │
  │ Chemistry: 42/50  A         │
  └─────────────────────────────┘
  
  ⚠️ Note: Physics score below average
  
  ↓
Step 4: Click on Physics to see teacher remarks
  ↓
Step 5: "Needs improvement in Chapter 5: Optics"
  ↓
Step 6: Parent decides to arrange tutor or speak with teacher
```

**Time:** 2 minutes  
**API:** 
- GET /api/parents/children
- GET /api/students/{id}/grades
- GET /api/students/{id}/attendance

### **Workflow 2: Message Teacher**

```
Step 1: Parent Dashboard → "Messages"
  ↓
Step 2: Start new conversation:
  - Select Child: Raj Kumar
  - Select Teacher: Mrs. Singh (Mathematics Teacher)
  ↓
Step 3: Type message:
  "Hello Mrs. Singh, Raj is having difficulty understanding 
   Chapter 5: Trigonometry. Could you provide extra practice 
   problems? Thank you."
  ↓
Step 4: Click "Send"
  ↓
Step 5: Message delivered
  ↓
Step 6: Teacher receives notification
  ↓
Step 7: Teacher replies:
  "Of course! I'll share some worksheets. Also, Raj can stay 
   back after school on Wednesday for extra help."
  ↓
Step 8: Parent receives notification → Reads reply
```

**Time:** 2 minutes  
**API:** 
- POST /api/chat/rooms (create 1-on-1 room)
- POST /api/chat/rooms/{id}/messages

---

## 🔄 **Cross-Role Workflows**

### **Workflow: Leave Request (Parent → Teacher → Admin)**

```
STEP 1: Parent submits leave request
  ↓
  Parent Dashboard → "Leave Request"
  Child: Raj Kumar
  From: Jan 20, 2026
  To: Jan 22, 2026
  Reason: Family Wedding
  [Submit]
  ↓
STEP 2: Notification sent to Class Teacher
  ↓
STEP 3: Teacher reviews request
  ↓
  Teacher Dashboard → "Pending Requests"
  Raj Kumar - Leave Request (3 days)
  [Approve] [Reject]
  ↓
STEP 4: Teacher clicks "Approve"
  ↓
STEP 5: System:
  - Updates attendance records (marked as "On Leave")
  - Notifies Admin
  - Notifies Parent (approved)
  ↓
STEP 6: Parent receives notification:
  "Leave approved for Raj Kumar (Jan 20-22)"
```

**Time:** 5 minutes (parent) + 1 minute (teacher)  
**Participants:** 3 (Parent, Teacher, Admin)

---

## 📊 **Reporting Workflows**

### **Admin: Generate Monthly Report**

```
Step 1: Admin Dashboard → "Reports"
  ↓
Step 2: Select Report Type:
  - Attendance Report
  - Grade Report
  - Financial Report
  - Custom Report
  ↓
Step 3: Select "Attendance Report"
  ↓
Step 4: Configure:
  - Month: January 2026
  - Class: All Classes
  - Format: PDF
  ↓
Step 5: Click "Generate"
  ↓
Step 6: Progress: "Generating report... 30%... 60%... 100%"
  ↓
Step 7: Report ready:
  
  📄 Monthly Attendance Report - January 2026
  
  Overall Statistics:
  - Total Students: 500
  - Average Attendance: 92%
  - Total School Days: 22
  
  Class-wise Breakdown:
  Class 10-A: 95% (40 students)
  Class 10-B: 90% (38 students)
  ...
  
  Students with < 75% attendance (need attention):
  1. Raj Kumar (10-A) - 68%
  2. Priya Sharma (9-B) - 72%
  
  [Download PDF] [Email to Principal] [Print]
```

**Time:** 30 seconds (generation time)  
**API:** GET /api/reports/attendance?month=2026-01&format=pdf

---

## ✅ **UI/UX Best Practices**

### **For All Roles:**

1. **Dashboard First:** Show most important info on landing page
2. **Quick Actions:** Common tasks accessible in 1-2 clicks
3. **Search Everything:** Global search bar (students, teachers, classes)
4. **Notifications:** Real-time alerts for important events
5. **Mobile Responsive:** All workflows work on phones
6. **Offline Support:** Cache common data for slow networks
7. **Bulk Operations:** Import/export CSV for large datasets
8. **Undo Actions:** Allow undo for non-critical operations
9. **Auto-save:** Save drafts automatically
10. **Keyboard Shortcuts:** Power users can use Ctrl+S, Ctrl+F, etc.

---

## 📚 **Next Steps**

1. **Database Design:** [04_DATABASE_SCHEMA.md](./04_DATABASE_SCHEMA.md)
2. **API Reference:** [05_API_ARCHITECTURE.md](./05_API_ARCHITECTURE.md)
3. **Implementation:** [10_SECURITY_IMPLEMENTATION.md](./10_SECURITY_IMPLEMENTATION.md)

---

**Document Status:** ✅ Complete  
**Last Updated:** January 13, 2026  
**Review:** Quarterly (gather user feedback)
