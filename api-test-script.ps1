# API Testing Script for School Management System
# This script tests all API endpoints and generates a detailed report

$API_BASE = "http://localhost:7266/api"
$testResults = @()
$authToken = $null
$schoolId = $null
$userId = $null

# Helper function to make API calls
function Invoke-APITest {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [bool]$RequiresAuth = $false,
        [string]$ExpectedStatus = "200"
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($RequiresAuth -and $authToken) {
        $headers["Authorization"] = "Bearer $authToken"
    }
    
    try {
        $params = @{
            Uri = "$API_BASE/$Endpoint"
            Method = $Method
            Headers = $headers
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params -SkipCertificateCheck -ErrorAction Stop
        
        $result = @{
            Name = $Name
            Endpoint = $Endpoint
            Method = $Method
            Status = "✅ PASS"
            StatusCode = $response.StatusCode
            Response = $response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
            Error = $null
        }
        
        Write-Host "✅ $Name - PASSED ($($response.StatusCode))" -ForegroundColor Green
        
    } catch {
        $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "N/A" }
        $errorBody = $null
        
        if ($_.Exception.Response) {
            try {
                $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
                $errorBody = $reader.ReadToEnd() | ConvertFrom-Json -ErrorAction SilentlyContinue
            } catch {}
        }
        
        $result = @{
            Name = $Name
            Endpoint = $Endpoint
            Method = $Method
            Status = "❌ FAIL"
            StatusCode = $statusCode
            Response = $errorBody
            Error = $_.Exception.Message
        }
        
        Write-Host "❌ $Name - FAILED ($statusCode)" -ForegroundColor Red
    }
    
    $testResults += $result
    return $result
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  SMS API Testing - Starting..." -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ============= AUTH API TESTS =============
Write-Host "`n[1/10] Testing Authentication APIs..." -ForegroundColor Yellow

# Test 1: Register (might fail if user exists - that's OK)
$registerResult = Invoke-APITest `
    -Name "Auth - Register New Admin" `
    -Method "POST" `
    -Endpoint "Auth/register" `
    -Body @{
        userName = "apitestadmin"
        email = "apitestadmin@sms.com"
        password = "Test@12345"
        role = "Admin"
        schoolId = $null
    }

# Test 2: Login
$loginResult = Invoke-APITest `
    -Name "Auth - Login" `
    -Method "POST" `
    -Endpoint "Auth/login" `
    -Body @{
        userName = "apitestadmin"
        password = "Test@12345"
    }

# Extract auth details from login if successful
if ($loginResult.Response.user) {
    $userId = $loginResult.Response.user.id
    $schoolId = $loginResult.Response.user.schoolId
    Write-Host "  → User ID: $userId" -ForegroundColor Cyan
    Write-Host "  → School ID: $schoolId" -ForegroundColor Cyan
}

# Test 3: Get Current User (requires auth via cookie)
$meResult = Invoke-APITest `
    -Name "Auth - Get Current User (/me)" `
    -Method "GET" `
    -Endpoint "Auth/me" `
    -RequiresAuth $true

# ============= SCHOOL API TESTS =============
Write-Host "`n[2/10] Testing School APIs..." -ForegroundColor Yellow

# Test 4: Get All Schools (Public)
$schoolsResult = Invoke-APITest `
    -Name "School - Get All Schools" `
    -Method "GET" `
    -Endpoint "School"

# Test 5: Search Schools (Public)
$searchResult = Invoke-APITest `
    -Name "School - Search Schools" `
    -Method "GET" `
    -Endpoint "School/search?schoolName=test"

# ============= CLASS API TESTS =============
Write-Host "`n[3/10] Testing Class APIs..." -ForegroundColor Yellow

# Test 6: Get All Classes (Requires Auth + SchoolAdmin)
$classesResult = Invoke-APITest `
    -Name "Class - Get All Classes" `
    -Method "GET" `
    -Endpoint "Class" `
    -RequiresAuth $true

# Test 7: Create Class (Requires Auth)
$createClassResult = Invoke-APITest `
    -Name "Class - Create New Class" `
    -Method "POST" `
    -Endpoint "Class" `
    -RequiresAuth $true `
    -Body @{
        className = "API Test Class"
        year = 2026
        section = "A"
    }

# ============= STUDENT API TESTS =============
Write-Host "`n[4/10] Testing Student APIs..." -ForegroundColor Yellow

# Test 8: Get All Students (Requires Auth)
$studentsResult = Invoke-APITest `
    -Name "Student - Get All Students" `
    -Method "GET" `
    -Endpoint "Student" `
    -RequiresAuth $true

# ============= TEACHER API TESTS =============
Write-Host "`n[5/10] Testing Teacher APIs..." -ForegroundColor Yellow

# Test 9: Get All Teachers (Requires Auth)
$teachersResult = Invoke-APITest `
    -Name "Teacher - Get All Teachers" `
    -Method "GET" `
    -Endpoint "Teacher" `
    -RequiresAuth $true

# ============= ANNOUNCEMENT API TESTS =============
Write-Host "`n[6/10] Testing Announcement APIs..." -ForegroundColor Yellow

# Test 10: Get All Announcements (Requires Auth)
$announcementsResult = Invoke-APITest `
    -Name "Announcement - Get All Announcements" `
    -Method "GET" `
    -Endpoint "Announcement" `
    -RequiresAuth $true

# ============= ATTENDANCE API TESTS =============
Write-Host "`n[7/10] Testing Attendance APIs..." -ForegroundColor Yellow

# Test 11: Get All Attendance (Requires Auth)
$attendanceResult = Invoke-APITest `
    -Name "Attendance - Get All Student Attendance" `
    -Method "GET" `
    -Endpoint "Attendance" `
    -RequiresAuth $true

# ============= TEACHER ATTENDANCE API TESTS =============
Write-Host "`n[8/10] Testing Teacher Attendance APIs..." -ForegroundColor Yellow

# Test 12: Get All Teacher Attendance (Requires Auth)
$teacherAttendanceResult = Invoke-APITest `
    -Name "TeacherAttendance - Get All Teacher Attendance" `
    -Method "GET" `
    -Endpoint "TeacherAttendance/GetTeacherAttendance" `
    -RequiresAuth $true

# ============= CHAT ROOM API TESTS =============
Write-Host "`n[9/10] Testing ChatRooms APIs..." -ForegroundColor Yellow

# Test 13: Get All Chat Rooms (Requires Auth)
$chatRoomsResult = Invoke-APITest `
    -Name "ChatRooms - Get All Rooms" `
    -Method "GET" `
    -Endpoint "ChatRooms" `
    -RequiresAuth $true

# ============= COMBINE/DASHBOARD API TESTS =============
Write-Host "`n[10/10] Testing Dashboard/Combine APIs..." -ForegroundColor Yellow

# Test 14: Get Home Combined Details (Public)
$homeCombinedResult = Invoke-APITest `
    -Name "Combine - Get Home Stats (Public)" `
    -Method "GET" `
    -Endpoint "Combine"

# ============= GENERATE REPORT =============
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Test Results Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "✅ PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "❌ FAIL" }).Count
$totalCount = $testResults.Count

Write-Host "Total Tests: $totalCount" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Success Rate: $([math]::Round(($passCount/$totalCount)*100, 2))%" -ForegroundColor Cyan

# Save detailed results to JSON
$reportPath = "D:\Projects\SMS\School-Management-System\api-test-results.json"
$testResults | ConvertTo-Json -Depth 10 | Out-File $reportPath
Write-Host "`nDetailed results saved to: $reportPath" -ForegroundColor Green

# Display failed tests
$failedTests = $testResults | Where-Object { $_.Status -eq "❌ FAIL" }
if ($failedTests) {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "  Failed Tests Details" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
    
    foreach ($test in $failedTests) {
        Write-Host "Test: $($test.Name)" -ForegroundColor Yellow
        Write-Host "  Endpoint: $($test.Method) $($test.Endpoint)" -ForegroundColor White
        Write-Host "  Status Code: $($test.StatusCode)" -ForegroundColor Red
        Write-Host "  Error: $($test.Error)" -ForegroundColor Red
        if ($test.Response) {
            Write-Host "  Response: $($test.Response | ConvertTo-Json -Compress)" -ForegroundColor Gray
        }
        Write-Host ""
    }
}

Write-Host "`n✅ API Testing Complete!`n" -ForegroundColor Green
