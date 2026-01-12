# Frontend Authentication Implementation Guide

## Overview

This document outlines the complete frontend authentication implementation matching the backend security features. The frontend now includes comprehensive authentication, authorization, password management, and role-based access control.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Authentication Service](#authentication-service)
3. [Auth Context](#auth-context)
4. [UI Components](#ui-components)
5. [Hooks & Utilities](#hooks--utilities)
6. [Role-Based Access Control](#role-based-access-control)
7. [Token Management](#token-management)
8. [Usage Examples](#usage-examples)
9. [Integration Points](#integration-points)

---

## Architecture Overview

### Core Components

```
Frontend/src/
├── services/
│   └── authService.ts          # API service for all auth operations
├── context/
│   └── AuthContext.tsx          # Global auth state management
├── hooks/
│   ├── useTokenRefresh.ts       # Automatic token refresh
│   └── useRole.ts               # Role-based utilities
├── components/
│   ├── RoleGate.tsx             # Role-based rendering
│   └── ProtectedRoute.tsx       # Route protection (existing)
└── popups/Auth/
    ├── LoginForm.tsx            # Login UI (updated)
    ├── RegisterForm.tsx         # Registration UI (existing)
    ├── ForgotPasswordForm.tsx   # Password reset request
    ├── ResetPasswordForm.tsx    # Password reset confirmation
    └── ChangePasswordForm.tsx   # Change password for logged-in users
```

### Authentication Flow

```
User Login → AuthService.login() → JWT Token (HttpOnly Cookie)
           → AuthContext updates → App re-renders with authenticated state
           → Token Auto-Refresh (every 2.5 hours)
           → Session Timeout Warning (5 min before expiry)
```

---

## Authentication Service

**File:** [Frontend/src/services/authService.ts](Frontend/src/services/authService.ts)

### Features

- Centralized API communication for all authentication operations
- Type-safe interfaces for all auth operations
- Comprehensive error handling
- Cookie-based authentication (HttpOnly cookies)

### Available Methods

```typescript
class AuthService {
  // Authentication
  login(credentials: LoginCredentials): Promise<User>
  logout(): Promise<void>
  getCurrentUser(): Promise<User>
  checkAuth(): Promise<User | null>
  
  // Token Management
  refreshToken(): Promise<void>
  
  // Registration
  register(data: RegisterData): Promise<void>
  
  // Password Management
  requestPasswordReset(email: string): Promise<void>
  resetPassword(data: ResetPasswordData): Promise<void>
  changePassword(data: ChangePasswordData): Promise<void>
}
```

### Interfaces

```typescript
interface User {
  id: string;
  username: string;
  email: string;
  schoolId?: string;
  roles: string[];
}

interface LoginCredentials {
  userName: string;
  password: string;
}

interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
}

interface ResetPasswordData {
  email: string;
  token: string;
  newPassword: string;
}
```

---

## Auth Context

**File:** [Frontend/src/context/AuthContext.tsx](Frontend/src/context/AuthContext.tsx)

### Features

- Global authentication state management
- Automatic token refresh (every 2.5 hours)
- Session timeout warnings (5 minutes before expiry)
- Role-based access helpers
- React Context API for state sharing

### Context API

```typescript
interface AuthContextType {
  // State
  isAuthenticated: boolean;
  loading: boolean;
  user: User | null;
  
  // Methods
  login(username: string, password: string): Promise<void>;
  logout(): Promise<void>;
  checkAuth(): Promise<void>;
  refreshToken(): Promise<void>;
  
  // Role Helpers
  hasRole(role: string): boolean;
  hasAnyRole(roles: string[]): boolean;
}
```

### Usage

```tsx
import { useAuth } from '@/context/AuthContext';

function MyComponent() {
  const { isAuthenticated, user, login, logout, hasRole } = useAuth();
  
  if (!isAuthenticated) {
    return <LoginPrompt />;
  }
  
  return (
    <div>
      <p>Welcome, {user?.username}!</p>
      {hasRole('Admin') && <AdminPanel />}
    </div>
  );
}
```

---

## UI Components

### 1. LoginForm.tsx (Updated)

**File:** [Frontend/src/popups/Auth/LoginForm.tsx](Frontend/src/popups/Auth/LoginForm.tsx)

**Changes:**
- Now uses `authService.login()` instead of direct fetch
- Added "Forgot Password?" link
- Simplified error handling
- Integrated with new AuthContext

**Props:**
```typescript
interface Props {
  onClose: () => void;
  onSwitch: () => void;
  onForgotPassword?: () => void;  // NEW
}
```

### 2. ForgotPasswordForm.tsx (New)

**File:** [Frontend/src/popups/Auth/ForgotPasswordForm.tsx](Frontend/src/popups/Auth/ForgotPasswordForm.tsx)

**Features:**
- Email validation
- Sends password reset link via backend API
- Success confirmation message
- 15-minute token expiration notice

**Backend Endpoint:** `POST /api/Auth/request-password-reset`

### 3. ResetPasswordForm.tsx (New)

**File:** [Frontend/src/popups/Auth/ResetPasswordForm.tsx](Frontend/src/popups/Auth/ResetPasswordForm.tsx)

**Features:**
- Reads token and email from URL query params
- Password strength validation (8+ chars, uppercase, lowercase, number, special char)
- Confirm password matching
- Success redirect to login

**Backend Endpoint:** `POST /api/Auth/reset-password`

**URL Format:** `/reset-password?email=user@school.edu&token=abc123...`

### 4. ChangePasswordForm.tsx (New)

**File:** [Frontend/src/popups/Auth/ChangePasswordForm.tsx](Frontend/src/popups/Auth/ChangePasswordForm.tsx)

**Features:**
- Requires current password
- Password strength validation
- Prevents reusing current password
- Auto-logout after successful change (security best practice)
- Show/hide password toggles

**Backend Endpoint:** `POST /api/Auth/change-password`

---

## Hooks & Utilities

### 1. useTokenRefresh Hook

**File:** [Frontend/src/hooks/useTokenRefresh.ts](Frontend/src/hooks/useTokenRefresh.ts)

#### Features

**Token Auto-Refresh:**
- Automatically refreshes JWT every 2.5 hours
- Tokens expire in 3 hours (30-minute safety buffer)
- Prevents concurrent refresh attempts
- Silently handles refresh failures

**Session Timeout Warning:**
- Shows toast warning 5 minutes before session expires
- Executes optional callback on timeout
- Helps users save work before logout

#### Usage

```typescript
import { useTokenRefresh, useSessionTimeout } from '@/hooks/useTokenRefresh';

// In AuthContext (already integrated)
useTokenRefresh(isAuthenticated);
useSessionTimeout(isAuthenticated, logout);
```

### 2. useRole Hook

**File:** [Frontend/src/hooks/useRole.ts](Frontend/src/hooks/useRole.ts)

#### Features

- Check single role: `hasRole('Admin')`
- Check any role: `hasAnyRole(['Admin', 'Teacher'])`
- Check all roles: `hasAllRoles(['Admin', 'Teacher'])`
- Predefined helpers: `isAdmin()`, `isTeacher()`, `isStudent()`, `isParent()`

#### Usage

```typescript
import { useRole } from '@/hooks/useRole';

function MyComponent() {
  const { hasRole, isAdmin, hasAnyRole } = useRole();
  
  return (
    <div>
      {isAdmin() && <button>Delete All</button>}
      {hasAnyRole(['Admin', 'Teacher']) && <button>Grade Students</button>}
      {hasRole('Student') && <p>Your GPA: 3.8</p>}
    </div>
  );
}
```

---

## Role-Based Access Control

### RoleGate Component

**File:** [Frontend/src/components/RoleGate.tsx](Frontend/src/components/RoleGate.tsx)

#### Features

- Declarative role-based rendering
- Supports single or multiple roles
- `requireAll` mode (AND logic) or default `requireAny` (OR logic)
- Optional fallback UI for unauthorized users

#### Usage Examples

```tsx
import RoleGate from '@/components/RoleGate';

// Render only for Admin
<RoleGate roles="Admin">
  <AdminPanel />
</RoleGate>

// Render for Admin OR Teacher
<RoleGate roles={["Admin", "Teacher"]}>
  <GradingPanel />
</RoleGate>

// Render for Admin AND Teacher (must have both)
<RoleGate roles={["Admin", "Teacher"]} requireAll>
  <SupervisorPanel />
</RoleGate>

// Show fallback if unauthorized
<RoleGate roles="Admin" fallback={<p>Access Denied</p>}>
  <AdminPanel />
</RoleGate>
```

---

## Token Management

### How It Works

1. **Login:** Backend sets HttpOnly cookie with JWT (3-hour expiry)
2. **Refresh Token:** Backend also sets refresh token cookie (7-day expiry)
3. **Auto-Refresh:** Frontend refreshes JWT every 2.5 hours automatically
4. **Logout:** Both tokens are cleared and blacklisted

### Token Lifecycle

```
Login → JWT (3 hours) + Refresh Token (7 days)
        ↓
        Auto-refresh every 2.5 hours
        ↓
        Warning at 2:55 (5 min before expiry)
        ↓
        Logout at 3:00 OR manual logout
```

### Security Features

- HttpOnly cookies (JavaScript cannot access tokens)
- Secure flag in production (HTTPS only)
- SameSite=Strict (CSRF protection)
- Token blacklist on logout
- Automatic cleanup of expired refresh tokens

---

## Usage Examples

### Complete Authentication Flow

```tsx
// App.tsx
import { AuthProvider } from '@/context/AuthContext';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<LandingPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          
          {/* Protected routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
          
          {/* Admin-only routes */}
          <Route element={<ProtectedRoute requiredRole="Admin" />}>
            <Route path="/admin" element={<AdminPanel />} />
          </Route>
        </Routes>
      </Router>
    </AuthProvider>
  );
}
```

### Login with Forgot Password

```tsx
// LandingPage.tsx
import { useState } from 'react';
import LoginForm from '@/popups/Auth/LoginForm';
import ForgotPasswordForm from '@/popups/Auth/ForgotPasswordForm';

function LandingPage() {
  const [showLogin, setShowLogin] = useState(false);
  const [showForgot, setShowForgot] = useState(false);
  
  return (
    <div>
      <button onClick={() => setShowLogin(true)}>Login</button>
      
      {showLogin && (
        <Modal onClose={() => setShowLogin(false)}>
          <LoginForm
            onClose={() => setShowLogin(false)}
            onSwitch={() => {/* switch to register */}}
            onForgotPassword={() => {
              setShowLogin(false);
              setShowForgot(true);
            }}
          />
        </Modal>
      )}
      
      {showForgot && (
        <Modal onClose={() => setShowForgot(false)}>
          <ForgotPasswordForm
            onClose={() => setShowForgot(false)}
            onSuccess={() => setShowForgot(false)}
          />
        </Modal>
      )}
    </div>
  );
}
```

### Change Password in Profile

```tsx
// ProfilePage.tsx
import { useState } from 'react';
import ChangePasswordForm from '@/popups/Auth/ChangePasswordForm';

function ProfilePage() {
  const [showChangePassword, setShowChangePassword] = useState(false);
  
  return (
    <div>
      <h1>My Profile</h1>
      <button onClick={() => setShowChangePassword(true)}>
        Change Password
      </button>
      
      {showChangePassword && (
        <Modal onClose={() => setShowChangePassword(false)}>
          <ChangePasswordForm
            onClose={() => setShowChangePassword(false)}
          />
        </Modal>
      )}
    </div>
  );
}
```

### Role-Based Navigation

```tsx
// Navigation.tsx
import { useAuth } from '@/context/AuthContext';
import { useRole } from '@/hooks/useRole';

function Navigation() {
  const { isAuthenticated, user, logout } = useAuth();
  const { isAdmin, hasAnyRole } = useRole();
  
  return (
    <nav>
      <Link to="/">Home</Link>
      
      {isAuthenticated ? (
        <>
          <Link to="/dashboard">Dashboard</Link>
          
          {hasAnyRole(['Admin', 'Teacher']) && (
            <Link to="/grades">Grades</Link>
          )}
          
          {isAdmin() && (
            <Link to="/admin">Admin Panel</Link>
          )}
          
          <Link to="/profile">Profile ({user?.username})</Link>
          <button onClick={logout}>Logout</button>
        </>
      ) : (
        <Link to="/login">Login</Link>
      )}
    </nav>
  );
}
```

### Reset Password Page

```tsx
// ResetPasswordPage.tsx
import ResetPasswordForm from '@/popups/Auth/ResetPasswordForm';

function ResetPasswordPage() {
  // URL: /reset-password?email=user@school.edu&token=abc123...
  
  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <ResetPasswordForm />
    </div>
  );
}
```

---

## Integration Points

### Backend Endpoints Used

| Endpoint | Method | Purpose | Component |
|----------|--------|---------|-----------|
| `/api/Auth/login` | POST | User login | LoginForm, authService |
| `/api/Auth/logout` | POST | User logout | AuthContext, authService |
| `/api/Auth/me` | GET | Get current user | AuthContext, authService |
| `/api/Auth/refresh` | POST | Refresh JWT token | useTokenRefresh, authService |
| `/api/Auth/register` | POST | New user registration | RegisterForm, authService |
| `/api/Auth/request-password-reset` | POST | Request reset email | ForgotPasswordForm, authService |
| `/api/Auth/reset-password` | POST | Reset password with token | ResetPasswordForm, authService |
| `/api/Auth/change-password` | POST | Change password (authenticated) | ChangePasswordForm, authService |

### Environment Variables

```bash
# .env or .env.local
VITE_API_URL=http://localhost:7266
```

### Cookie Configuration

Backend must set cookies with:
- HttpOnly=true (JavaScript cannot access)
- Secure=true (HTTPS only in production)
- SameSite=Strict (CSRF protection)
- Path=/
- Domain=(your domain)

---

## Security Best Practices

### 1. Password Requirements

All password forms enforce:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### 2. Token Security

- JWT stored in HttpOnly cookies (not localStorage)
- Refresh tokens have longer expiry (7 days)
- Access tokens expire quickly (3 hours)
- Auto-refresh before expiry (2.5 hours)
- Tokens blacklisted on logout

### 3. Session Management

- Session timeout warning (5 min before expiry)
- Auto-logout on token expiration
- Force logout after password change
- Concurrent refresh prevention

### 4. Error Handling

- User-friendly error messages
- Backend error messages displayed
- Toast notifications for success/error
- Form validation before submission

---

## Next Steps

### Recommended Enhancements

1. **Email Verification:**
   - Add email verification on registration
   - Resend verification email functionality

2. **Two-Factor Authentication (2FA):**
   - TOTP support
   - Backup codes
   - SMS verification (optional)

3. **Account Lockout UI:**
   - Show lockout countdown
   - Contact admin option

4. **Audit Log Viewer:**
   - Admin-only audit log viewer
   - Filter by user, action, date
   - Export audit logs

5. **Remember Me:**
   - Optional "Remember Me" checkbox
   - Extended refresh token expiry (30 days)

6. **Session Management:**
   - View active sessions
   - Revoke specific sessions
   - "Logout All Devices"

7. **Role Management UI:**
   - Admin interface to assign/revoke roles
   - Role hierarchy visualization

8. **Password History:**
   - Prevent reusing last N passwords
   - Password expiration policy

---

## Troubleshooting

### Common Issues

**Issue:** "Token refresh failed" in console  
**Solution:** Check that refresh token cookie is being sent. Verify backend `/api/Auth/refresh` endpoint is working.

**Issue:** User logged out unexpectedly  
**Solution:** Check token expiration. Ensure auto-refresh is working. Verify no CORS issues.

**Issue:** "Not authenticated" after login  
**Solution:** Check that cookies are being set. Verify `credentials: 'include'` in fetch requests.

**Issue:** Role-based features not showing  
**Solution:** Verify `user.roles` array is populated. Check role names match exactly (case-sensitive).

**Issue:** Password reset link invalid  
**Solution:** Token expires in 15 minutes. Request new reset link. Verify URL format.

---

## Testing Checklist

### Authentication

- [ ] Login with valid credentials
- [ ] Login with invalid credentials (shows error)
- [ ] Logout clears user state
- [ ] Auto-refresh token before expiry
- [ ] Session timeout warning appears
- [ ] Logout on token expiration

### Password Management

- [ ] Request password reset email
- [ ] Reset password with valid token
- [ ] Reset password with expired token (shows error)
- [ ] Change password (logged in)
- [ ] Auto-logout after password change
- [ ] Password validation (strength requirements)

### Role-Based Access

- [ ] Admin sees admin-only features
- [ ] Teacher sees teacher features
- [ ] Student sees student features
- [ ] Unauthorized users don't see restricted content
- [ ] RoleGate component works correctly
- [ ] useRole hook returns correct values

### Edge Cases

- [ ] Concurrent login on multiple tabs
- [ ] Network error during login
- [ ] Invalid token in reset URL
- [ ] Session timeout while user is active
- [ ] Refresh token expired (7 days)
- [ ] Account locked out (too many failed attempts)

---

## Summary

The frontend now has complete authentication parity with the backend:

✅ **Authentication:** Login, logout, registration, session management  
✅ **Authorization:** Role-based access control, RoleGate component, useRole hook  
✅ **Password Management:** Forgot password, reset password, change password  
✅ **Token Management:** Auto-refresh, session timeout, secure cookies  
✅ **Security:** HttpOnly cookies, CSRF protection, password validation  
✅ **User Experience:** Loading states, error handling, success feedback  

All backend security features are now accessible and integrated in the frontend UI.
