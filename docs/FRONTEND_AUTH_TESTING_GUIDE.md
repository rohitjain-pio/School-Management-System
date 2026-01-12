# Frontend Authentication Testing Guide

## Overview

This guide provides a complete testing checklist for all frontend authentication features implemented to match the backend security features.

---

## Prerequisites

1. **Backend Running:**
   ```powershell
   cd Backend
   dotnet run --project SMSPrototype1
   # Should be running on https://localhost:7266
   ```

2. **Frontend Running:**
   ```powershell
   cd Frontend
   npm run dev
   # Should be running on http://localhost:5173
   ```

3. **Environment Variables:**
   ```bash
   # Frontend/.env.local
   VITE_API_URL=http://localhost:7266
   ```

4. **Test User Accounts:**
   Create test users with different roles via Swagger or database seeding.

---

## Test Scenarios

### 1. Authentication Flow

#### TC-001: Successful Login
**Steps:**
1. Open frontend (http://localhost:5173)
2. Click "Login" button
3. Enter valid username and password
4. Submit form

**Expected:**
- ✅ User is redirected to /dashboard
- ✅ Success toast notification appears
- ✅ User data is populated in AuthContext
- ✅ Navigation shows user's username
- ✅ Logout button is visible

#### TC-002: Failed Login (Invalid Credentials)
**Steps:**
1. Open login form
2. Enter invalid username/password
3. Submit form

**Expected:**
- ✅ Error message displayed
- ✅ Toast error notification
- ✅ User remains on login page
- ✅ Form is not cleared (for retry)

#### TC-003: Failed Login (Empty Fields)
**Steps:**
1. Open login form
2. Leave username/password empty
3. Submit form

**Expected:**
- ✅ Validation errors appear
- ✅ Form does not submit
- ✅ Fields highlighted in red

#### TC-004: Logout
**Steps:**
1. Login as valid user
2. Click logout button

**Expected:**
- ✅ User is logged out
- ✅ Redirected to home page
- ✅ AuthContext cleared (isAuthenticated = false, user = null)
- ✅ Login button visible again
- ✅ Token blacklisted on backend

---

### 2. Token Management

#### TC-005: Token Auto-Refresh
**Setup:** Login and wait 2.5 hours (or modify refresh interval for testing)

**Steps:**
1. Login
2. Wait for auto-refresh interval (2.5 hours default)
3. Check browser console

**Expected:**
- ✅ Console log: "Token refreshed successfully" every 2.5 hours
- ✅ User remains authenticated
- ✅ No interruption to user experience

**Testing Tip:** Temporarily modify refresh interval in useTokenRefresh.ts:
```typescript
const REFRESH_INTERVAL = 30 * 1000; // 30 seconds for testing
```

#### TC-006: Session Timeout Warning
**Setup:** Login and wait until 5 minutes before token expiry

**Steps:**
1. Login
2. Wait until 2 hours 55 minutes
3. Check for warning toast

**Expected:**
- ✅ Warning toast appears: "Your session will expire in 5 minutes"
- ✅ Toast duration: 10 seconds
- ✅ User can continue working

#### TC-007: Token Expiration (Auto-Logout)
**Setup:** Login and wait full 3 hours

**Steps:**
1. Login
2. Wait 3 hours (token expiration)

**Expected:**
- ✅ User is automatically logged out
- ✅ Error toast: "Your session has expired"
- ✅ Redirected to home/login page

---

### 3. Password Management

#### TC-008: Forgot Password - Request Reset Email
**Steps:**
1. Open login form
2. Click "Forgot password?" link
3. Enter valid email address
4. Submit

**Expected:**
- ✅ Success message: "Password reset link sent! Check your email."
- ✅ Success toast notification
- ✅ Form shows confirmation message
- ✅ Backend sends email (check logs)

#### TC-009: Forgot Password - Invalid Email
**Steps:**
1. Open forgot password form
2. Enter invalid email format (e.g., "notanemail")
3. Submit

**Expected:**
- ✅ Validation error: "Please enter a valid email address"
- ✅ Form does not submit

#### TC-010: Reset Password - With Valid Token
**Setup:** Request password reset, get token from email

**Steps:**
1. Open reset password link from email
   - URL format: `/reset-password?email=user@school.edu&token=abc123...`
2. Enter new password (must meet complexity requirements)
3. Confirm new password
4. Submit

**Expected:**
- ✅ Success message: "Password reset successfully!"
- ✅ Redirected to login page after 3 seconds
- ✅ Can login with new password

#### TC-011: Reset Password - Expired Token
**Setup:** Request reset, wait 15+ minutes

**Steps:**
1. Open reset password link (15+ minutes old)
2. Enter new password
3. Submit

**Expected:**
- ✅ Error: "Token has expired" or "Invalid token"
- ✅ Error toast notification
- ✅ Option to request new reset link

#### TC-012: Reset Password - Weak Password
**Steps:**
1. Open reset password form
2. Enter weak password (e.g., "12345")
3. Submit

**Expected:**
- ✅ Validation error listing missing requirements
- ✅ Example: "Password must have: At least 8 characters, One uppercase letter, One number, One special character"

#### TC-013: Reset Password - Passwords Don't Match
**Steps:**
1. Open reset password form
2. Enter "NewPass123!" in new password
3. Enter "DifferentPass123!" in confirm password
4. Submit

**Expected:**
- ✅ Validation error: "Passwords do not match"
- ✅ Form does not submit

#### TC-014: Change Password - Success
**Setup:** Login as authenticated user

**Steps:**
1. Navigate to profile or settings
2. Open "Change Password" form
3. Enter current password
4. Enter new password (meeting complexity requirements)
5. Confirm new password
6. Submit

**Expected:**
- ✅ Success toast: "Password changed successfully! Please login again."
- ✅ User is automatically logged out
- ✅ Redirected to home/login page
- ✅ Can login with new password

#### TC-015: Change Password - Wrong Current Password
**Steps:**
1. Open change password form
2. Enter incorrect current password
3. Enter new password
4. Submit

**Expected:**
- ✅ Error: "Current password is incorrect"
- ✅ Form does not clear
- ✅ User can retry

#### TC-016: Change Password - Same as Current
**Steps:**
1. Open change password form
2. Enter correct current password
3. Enter same password as new password
4. Submit

**Expected:**
- ✅ Error: "New password must be different from current password"

---

### 4. Role-Based Access Control

#### TC-017: Admin Role - See Admin Features
**Setup:** Login as user with Admin role

**Steps:**
1. Login
2. Check navigation/dashboard

**Expected:**
- ✅ Admin-only links visible (e.g., "Admin Panel")
- ✅ Admin-only components rendered
- ✅ `useRole().isAdmin()` returns true

#### TC-018: Non-Admin - Cannot See Admin Features
**Setup:** Login as user WITHOUT Admin role (e.g., Student)

**Steps:**
1. Login
2. Check navigation/dashboard

**Expected:**
- ✅ Admin-only links NOT visible
- ✅ Admin-only components NOT rendered
- ✅ `useRole().isAdmin()` returns false

#### TC-019: RoleGate - Single Role
**Setup:** Login as Teacher

**Test Component:**
```tsx
<RoleGate roles="Teacher">
  <div>Teacher Content</div>
</RoleGate>
```

**Expected:**
- ✅ Content visible for Teacher
- ✅ Content hidden for non-Teacher

#### TC-020: RoleGate - Multiple Roles (OR logic)
**Setup:** Login as Admin or Teacher

**Test Component:**
```tsx
<RoleGate roles={["Admin", "Teacher"]}>
  <div>Admin OR Teacher Content</div>
</RoleGate>
```

**Expected:**
- ✅ Content visible for Admin
- ✅ Content visible for Teacher
- ✅ Content hidden for Student/Parent

#### TC-021: RoleGate - Multiple Roles (AND logic)
**Setup:** Login as user with both Admin AND Teacher roles

**Test Component:**
```tsx
<RoleGate roles={["Admin", "Teacher"]} requireAll>
  <div>Admin AND Teacher Content</div>
</RoleGate>
```

**Expected:**
- ✅ Content visible only if user has BOTH roles
- ✅ Content hidden if user has only one role

#### TC-022: useRole Hook - hasAnyRole
**Test:**
```tsx
const { hasAnyRole } = useRole();
console.log(hasAnyRole(['Admin', 'Teacher'])); // true if Admin OR Teacher
```

**Expected:**
- ✅ Returns true if user has any of the specified roles
- ✅ Returns false if user has none

#### TC-023: useRole Hook - hasAllRoles
**Test:**
```tsx
const { hasAllRoles } = useRole();
console.log(hasAllRoles(['Admin', 'Teacher'])); // true only if both
```

**Expected:**
- ✅ Returns true only if user has ALL specified roles
- ✅ Returns false if missing any role

---

### 5. State Persistence

#### TC-024: Page Reload - User Persists
**Steps:**
1. Login
2. Navigate to /dashboard
3. Refresh page (F5)

**Expected:**
- ✅ User remains authenticated
- ✅ User data still populated
- ✅ No redirect to login

#### TC-025: New Tab - User Persists
**Steps:**
1. Login in Tab 1
2. Open new tab (Tab 2)
3. Navigate to app URL in Tab 2

**Expected:**
- ✅ User is authenticated in Tab 2
- ✅ Same user data in both tabs

#### TC-026: Logout in One Tab - Affects All
**Steps:**
1. Login in Tab 1 and Tab 2
2. Logout in Tab 1
3. Check Tab 2

**Expected:**
- ✅ Tab 2 also shows logged out state (after next request)
- ✅ Token is blacklisted

---

### 6. Error Handling

#### TC-027: Network Error During Login
**Steps:**
1. Stop backend server
2. Try to login

**Expected:**
- ✅ Error message: "Network error" or "Failed to login"
- ✅ Toast error notification
- ✅ User-friendly error message (not stack trace)

#### TC-028: Backend Validation Error
**Setup:** Submit invalid data that passes frontend validation but fails backend

**Expected:**
- ✅ Backend error message displayed to user
- ✅ No crashes or blank screens
- ✅ Form can be corrected and resubmitted

#### TC-029: Expired Refresh Token
**Setup:** Login, wait 7 days for refresh token to expire

**Expected:**
- ✅ User is logged out
- ✅ Error message (optional)
- ✅ Redirected to login page

---

### 7. Security

#### TC-030: HttpOnly Cookies
**Steps:**
1. Login
2. Open browser DevTools → Application → Cookies
3. Check for JWT cookies

**Expected:**
- ✅ Cookies have `HttpOnly` flag
- ✅ Cannot access cookies via `document.cookie` in console
- ✅ Cookies are sent automatically with requests

#### TC-031: CORS - Credentials Included
**Steps:**
1. Check Network tab during login
2. Inspect request headers

**Expected:**
- ✅ Request includes `credentials: 'include'`
- ✅ Cookies sent with cross-origin requests
- ✅ Backend allows credentials

#### TC-032: Token Blacklist on Logout
**Steps:**
1. Login
2. Copy JWT token from cookies (if visible in network tab)
3. Logout
4. Try to use old token directly in API request

**Expected:**
- ✅ Old token is rejected
- ✅ 401 Unauthorized response
- ✅ Token is blacklisted on backend

---

## Performance Testing

### PT-001: Login Performance
**Steps:**
1. Clear cache
2. Measure time from form submit to dashboard load

**Expected:**
- ✅ < 2 seconds on fast network
- ✅ < 5 seconds on slow 3G

### PT-002: Token Refresh Performance
**Steps:**
1. Login
2. Monitor refresh operations in console

**Expected:**
- ✅ Refresh happens in background (no UI blocking)
- ✅ < 1 second refresh time
- ✅ No duplicate refresh requests

---

## Accessibility Testing

### AT-001: Keyboard Navigation
**Steps:**
1. Use Tab key to navigate login form
2. Use Enter to submit

**Expected:**
- ✅ Can navigate all form fields
- ✅ Focus indicators visible
- ✅ Enter key submits form

### AT-002: Screen Reader Compatibility
**Steps:**
1. Use screen reader (NVDA, JAWS, VoiceOver)
2. Navigate login form

**Expected:**
- ✅ Labels announced correctly
- ✅ Errors announced
- ✅ Form fields identifiable

---

## Browser Compatibility

Test on:
- ✅ Chrome (latest)
- ✅ Firefox (latest)
- ✅ Edge (latest)
- ✅ Safari (latest)

---

## Mobile Testing

### MT-001: Responsive Login Form
**Steps:**
1. Open on mobile device or DevTools mobile view
2. Test login form

**Expected:**
- ✅ Form fits on screen
- ✅ Inputs are touch-friendly
- ✅ Keyboard doesn't obscure inputs

### MT-002: Token Refresh on Mobile
**Steps:**
1. Login on mobile
2. Lock device and wait

**Expected:**
- ✅ Token refreshes when app returns to foreground
- ✅ User remains authenticated

---

## Automated Testing (Optional)

### Example: Playwright Test

```typescript
import { test, expect } from '@playwright/test';

test('successful login', async ({ page }) => {
  await page.goto('http://localhost:5173');
  await page.click('text=Login');
  
  await page.fill('input[name="username"]', 'testuser');
  await page.fill('input[name="password"]', 'TestPass123!');
  await page.click('button[type="submit"]');
  
  await expect(page).toHaveURL(/.*dashboard/);
  await expect(page.locator('text=Welcome')).toBeVisible();
});

test('failed login shows error', async ({ page }) => {
  await page.goto('http://localhost:5173');
  await page.click('text=Login');
  
  await page.fill('input[name="username"]', 'invalid');
  await page.fill('input[name="password"]', 'wrong');
  await page.click('button[type="submit"]');
  
  await expect(page.locator('text=Invalid credentials')).toBeVisible();
});
```

---

## Test Data

### Test Users

Create these test users for comprehensive testing:

```sql
-- Admin user
INSERT INTO ApplicationUsers (UserName, Email, PasswordHash, SchoolId)
VALUES ('admin', 'admin@school.edu', '<hash>', 1);

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<admin-id>', '<admin-role-id>');

-- Teacher user
INSERT INTO ApplicationUsers (UserName, Email, PasswordHash, SchoolId)
VALUES ('teacher1', 'teacher@school.edu', '<hash>', 1);

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<teacher-id>', '<teacher-role-id>');

-- Student user
INSERT INTO ApplicationUsers (UserName, Email, PasswordHash, SchoolId)
VALUES ('student1', 'student@school.edu', '<hash>', 1);

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<student-id>', '<student-role-id>');
```

**Password:** Use `TestPass123!` (meets complexity requirements)

---

## Regression Testing

After any auth-related changes, rerun:
- All Authentication Flow tests (TC-001 to TC-004)
- Token Management tests (TC-005 to TC-007)
- Role-Based Access tests (TC-017 to TC-023)

---

## Bug Reporting Template

```
**Test Case:** TC-XXX
**Environment:** 
  - OS: Windows 11
  - Browser: Chrome 120
  - Frontend: http://localhost:5173
  - Backend: https://localhost:7266

**Steps to Reproduce:**
1. ...
2. ...
3. ...

**Expected Result:**
...

**Actual Result:**
...

**Screenshots/Logs:**
[Attach console logs, network requests, screenshots]

**Additional Context:**
...
```

---

## Summary

### Critical Tests (Must Pass)
- ✅ TC-001: Successful Login
- ✅ TC-004: Logout
- ✅ TC-005: Token Auto-Refresh
- ✅ TC-014: Change Password
- ✅ TC-017: Admin Role Access
- ✅ TC-024: Page Reload Persistence
- ✅ TC-030: HttpOnly Cookies

### High Priority
- All password management tests (TC-008 to TC-016)
- All role-based access tests (TC-017 to TC-023)

### Medium Priority
- Error handling tests (TC-027 to TC-029)
- Performance tests (PT-001, PT-002)

---

**Testing checklist complete!** Use this guide to ensure all authentication features work correctly.
