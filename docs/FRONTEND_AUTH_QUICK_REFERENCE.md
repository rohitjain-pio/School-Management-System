# Frontend Authentication - Quick Reference

## 🚀 Quick Start

### 1. Using Authentication

```tsx
import { useAuth } from '@/context/AuthContext';

function MyComponent() {
  const { isAuthenticated, user, login, logout } = useAuth();
  
  // Check if user is logged in
  if (!isAuthenticated) {
    return <LoginPrompt />;
  }
  
  // Access user data
  return <div>Welcome, {user?.username}!</div>;
}
```

### 2. Role-Based Rendering

```tsx
import RoleGate from '@/components/RoleGate';
import { useRole } from '@/hooks/useRole';

function Dashboard() {
  const { isAdmin, hasAnyRole } = useRole();
  
  return (
    <div>
      {/* Method 1: Using RoleGate component */}
      <RoleGate roles="Admin">
        <AdminPanel />
      </RoleGate>
      
      {/* Method 2: Using useRole hook */}
      {isAdmin() && <button>Delete All</button>}
      
      {/* Multiple roles (OR logic) */}
      <RoleGate roles={["Admin", "Teacher"]}>
        <GradingPanel />
      </RoleGate>
      
      {/* Multiple roles (AND logic) */}
      <RoleGate roles={["Admin", "Teacher"]} requireAll>
        <SupervisorPanel />
      </RoleGate>
    </div>
  );
}
```

### 3. Password Management

```tsx
// Forgot Password
import ForgotPasswordForm from '@/popups/Auth/ForgotPasswordForm';

<ForgotPasswordForm 
  onClose={() => setShow(false)} 
/>

// Reset Password (from email link)
// URL: /reset-password?email=user@school.edu&token=abc123
import ResetPasswordForm from '@/popups/Auth/ResetPasswordForm';

<ResetPasswordForm />

// Change Password (logged in users)
import ChangePasswordForm from '@/popups/Auth/ChangePasswordForm';

<ChangePasswordForm 
  onClose={() => setShow(false)} 
/>
```

### 4. Login Form (Updated)

```tsx
import LoginForm from '@/popups/Auth/LoginForm';

<LoginForm
  onClose={() => setShowLogin(false)}
  onSwitch={() => setShowRegister(true)}
  onForgotPassword={() => setShowForgot(true)}  // NEW!
/>
```

---

## 📚 Available Hooks

### useAuth()

```tsx
const {
  isAuthenticated,  // boolean
  loading,          // boolean
  user,             // User | null
  login,            // (username: string, password: string) => Promise<void>
  logout,           // () => Promise<void>
  checkAuth,        // () => Promise<void>
  refreshToken,     // () => Promise<void>
  hasRole,          // (role: string) => boolean
  hasAnyRole,       // (roles: string[]) => boolean
} = useAuth();
```

### useRole()

```tsx
const {
  userRoles,        // string[]
  hasRole,          // (role: string) => boolean
  hasAnyRole,       // (roles: string[]) => boolean
  hasAllRoles,      // (roles: string[]) => boolean
  isAdmin,          // () => boolean
  isTeacher,        // () => boolean
  isStudent,        // () => boolean
  isParent,         // () => boolean
} = useRole();
```

---

## 🔐 Role Names (Backend)

- `Admin` - Full system access
- `Teacher` - Teaching features
- `Student` - Student features
- `Parent` - Parent features

**Note:** Role names are case-sensitive!

---

## 🛠️ Common Patterns

### Protected Route with Role

```tsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';

function ProtectedRoute({ children, requiredRole }) {
  const { isAuthenticated, user } = useAuth();
  
  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }
  
  if (requiredRole && !user?.roles?.includes(requiredRole)) {
    return <Navigate to="/dashboard" />;
  }
  
  return children;
}
```

### Conditional Navigation

```tsx
import { useAuth } from '@/context/AuthContext';
import { useRole } from '@/hooks/useRole';

function Navigation() {
  const { isAuthenticated, logout } = useAuth();
  const { isAdmin } = useRole();
  
  return (
    <nav>
      {isAuthenticated ? (
        <>
          <Link to="/dashboard">Dashboard</Link>
          {isAdmin() && <Link to="/admin">Admin</Link>}
          <button onClick={logout}>Logout</button>
        </>
      ) : (
        <Link to="/login">Login</Link>
      )}
    </nav>
  );
}
```

### Loading State

```tsx
import { useAuth } from '@/context/AuthContext';

function App() {
  const { loading, isAuthenticated } = useAuth();
  
  if (loading) {
    return <div>Loading...</div>;
  }
  
  return isAuthenticated ? <Dashboard /> : <LandingPage />;
}
```

---

## 🎯 Backend Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/Auth/login` | POST | Login |
| `/api/Auth/logout` | POST | Logout |
| `/api/Auth/me` | GET | Get current user |
| `/api/Auth/refresh` | POST | Refresh token |
| `/api/Auth/register` | POST | Register |
| `/api/Auth/request-password-reset` | POST | Request reset email |
| `/api/Auth/reset-password` | POST | Reset password |
| `/api/Auth/change-password` | POST | Change password |

---

## ⚙️ Configuration

### Environment Variables

```bash
# Frontend/.env.local
VITE_API_URL=http://localhost:7266
```

### App Setup

```tsx
// main.tsx or App.tsx
import { AuthProvider } from '@/context/AuthContext';

function App() {
  return (
    <AuthProvider>
      <Router>
        {/* Your routes */}
      </Router>
    </AuthProvider>
  );
}
```

---

## 🔄 Token Auto-Refresh

Automatically refreshes JWT every 2.5 hours (tokens expire in 3 hours).

- **No action required** - happens automatically in AuthContext
- **Session warning** - appears 5 min before expiry
- **Auto-logout** - when token expires

---

## ✅ Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

---

## 🐛 Troubleshooting

### User logged out unexpectedly
- Token expired (3 hours)
- Check browser console for errors
- Verify backend is running

### "Not authenticated" error
- Check cookies are enabled
- Verify `VITE_API_URL` is correct
- Check CORS settings on backend

### Roles not working
- Verify `user.roles` array is populated
- Role names are case-sensitive
- Check user was assigned roles in backend

### Password reset link invalid
- Token expires in 15 minutes
- Request new reset link
- Verify URL format: `/reset-password?email=...&token=...`

---

## 📦 Import Paths

```tsx
// Context
import { useAuth } from '@/context/AuthContext';

// Hooks
import { useRole } from '@/hooks/useRole';
import { useTokenRefresh } from '@/hooks/useTokenRefresh';

// Components
import RoleGate from '@/components/RoleGate';

// Services
import { authService } from '@/services/authService';

// Forms
import LoginForm from '@/popups/Auth/LoginForm';
import RegisterForm from '@/popups/Auth/RegisterForm';
import ForgotPasswordForm from '@/popups/Auth/ForgotPasswordForm';
import ResetPasswordForm from '@/popups/Auth/ResetPasswordForm';
import ChangePasswordForm from '@/popups/Auth/ChangePasswordForm';
```

---

## 🎨 Example: Complete Login Flow

```tsx
import { useState } from 'react';
import LoginForm from '@/popups/Auth/LoginForm';
import RegisterForm from '@/popups/Auth/RegisterForm';
import ForgotPasswordForm from '@/popups/Auth/ForgotPasswordForm';

function LandingPage() {
  const [activeForm, setActiveForm] = useState<'login' | 'register' | 'forgot' | null>(null);
  
  return (
    <div>
      <button onClick={() => setActiveForm('login')}>Login</button>
      
      {activeForm === 'login' && (
        <Modal onClose={() => setActiveForm(null)}>
          <LoginForm
            onClose={() => setActiveForm(null)}
            onSwitch={() => setActiveForm('register')}
            onForgotPassword={() => setActiveForm('forgot')}
          />
        </Modal>
      )}
      
      {activeForm === 'register' && (
        <Modal onClose={() => setActiveForm(null)}>
          <RegisterForm
            onClose={() => setActiveForm(null)}
            onSwitch={() => setActiveForm('login')}
          />
        </Modal>
      )}
      
      {activeForm === 'forgot' && (
        <Modal onClose={() => setActiveForm(null)}>
          <ForgotPasswordForm
            onClose={() => setActiveForm(null)}
          />
        </Modal>
      )}
    </div>
  );
}
```

---

## 📖 Full Documentation

For complete documentation, see:
- [FRONTEND_AUTH_IMPLEMENTATION.md](./FRONTEND_AUTH_IMPLEMENTATION.md)
- [FRONTEND_UPDATES_SUMMARY.md](./FRONTEND_UPDATES_SUMMARY.md)
- [BACKEND_UPDATES_SUMMARY.md](./BACKEND_UPDATES_SUMMARY.md)

---

**Quick reference complete!** 🚀 Ready to use all authentication features.
