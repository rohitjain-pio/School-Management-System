# Error Monitor - Quick Start Guide

## 🚀 Getting Started

The error monitor automatically captures and displays errors from both frontend and backend in development mode.

## 📍 Location

Look for a **red floating button** in the **bottom-right corner** of your screen (development mode only).

## 🎯 Quick Actions

| Action | How To |
|--------|--------|
| **Open Monitor** | Click the red floating button |
| **View Errors** | Errors automatically appear in modal |
| **Filter by Category** | Click tabs: All, Database, Backend, Frontend, Network, Validation, Auth |
| **Search Errors** | Type in search box at top of modal |
| **Auto-Refresh** | Click "Auto-refresh ON" button (polls every 5s) |
| **Manual Refresh** | Click "Refresh" button |
| **Clear All Errors** | Click "Clear All" button |
| **Export Errors** | Click "Export JSON" button |
| **View Stack Trace** | Click "Show Stack Trace" on any error |
| **Close Modal** | Click X or click outside modal |

## 🎨 Category Colors

- 🟣 **Database** - Purple badge
- 🔴 **Backend** - Red badge
- 🔵 **Frontend** - Blue badge
- 🟠 **Network** - Orange badge
- 🟡 **Validation** - Yellow badge
- 🌸 **Auth** - Pink badge

## 🧪 Testing the Error Monitor

### Method 1: Using Test Endpoints (Backend)

Open your browser console and run:

```javascript
// Test different error types
await fetch('http://localhost:5000/api/Debug/errors/test?type=backend', { method: 'POST' });
await fetch('http://localhost:5000/api/Debug/errors/test?type=database', { method: 'POST' });
await fetch('http://localhost:5000/api/Debug/errors/test?type=validation', { method: 'POST' });
await fetch('http://localhost:5000/api/Debug/errors/test?type=auth', { method: 'POST' });
```

### Method 2: Trigger Real Errors

```javascript
// Frontend error - JavaScript exception
throw new Error("Test frontend error");

// Frontend error - Promise rejection
Promise.reject(new Error("Test promise rejection"));

// Network error - Invalid endpoint
fetch('http://localhost:5000/api/InvalidEndpoint');

// Auth error - No token
fetch('http://localhost:5000/api/Auth/me');

// Validation error - Invalid data
fetch('http://localhost:5000/api/Students', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ invalidData: true })
});
```

### Method 3: React Component Error

Add this to any React component:

```tsx
const [triggerError, setTriggerError] = useState(false);

if (triggerError) {
  throw new Error("Test component error");
}

// In your JSX:
<button onClick={() => setTriggerError(true)}>
  Trigger Error
</button>
```

## 📊 Understanding Error Cards

Each error card shows:

```
┌─────────────────────────────────────────┐
│ 🟣 Database  ⏰ 1/12/2026, 3:45:23 PM │
│ @ StudentRepository.cs                  │
├─────────────────────────────────────────┤
│ Failed to connect to database          │
│                                         │
│ 📋 Metadata:                           │
│   Path: /api/Students                  │
│   Method: GET                          │
│   ExceptionType: SqlException          │
│                                         │
│ [Show Stack Trace]                     │
└─────────────────────────────────────────┘
```

## 🔍 Search Examples

| Search For | Finds |
|------------|-------|
| `database` | All errors mentioning "database" |
| `token` | Auth/token-related errors |
| `StudentRepository` | Errors from StudentRepository |
| `POST` | All POST request errors |
| `validation` | Validation errors |

## 💡 Tips & Tricks

### Tip 1: Use Auto-Refresh During Active Development
Toggle auto-refresh ON to see backend errors as they happen (updates every 5 seconds).

### Tip 2: Export Before Clearing
Always export errors to JSON before clearing if you need to analyze them later.

### Tip 3: Check Stack Traces
Click "Show Stack Trace" to see the exact line where the error occurred.

### Tip 4: Filter by Category
Use category tabs to focus on specific error types (e.g., only Database errors).

### Tip 5: Search for Patterns
Use search to find all errors from a specific file or containing specific keywords.

## ⚠️ Important Notes

### Development Only
The error monitor **only works in development mode**:
- `npm run dev` ✅ (visible)
- `npm run build` ❌ (hidden)

### Memory Limits
- Frontend stores max **500 errors**
- Backend stores max **1000 errors**
- Oldest errors auto-deleted when limit reached

### Auto-Refresh Impact
- Polls backend every 5 seconds when ON
- Minimal performance impact
- Turn OFF when not actively monitoring

## 🐛 Troubleshooting

### "No errors found" but I know there are errors
1. Check auto-refresh is ON
2. Click "Refresh" button manually
3. Verify backend is running (`http://localhost:5000`)
4. Check browser console for fetch errors

### Button not visible
1. Confirm you're in development mode (`npm run dev`)
2. Check browser console for errors
3. Verify ErrorMonitorButton is rendered in App.tsx

### Backend errors not showing
1. Confirm backend is running
2. Check `/api/Debug/errors` endpoint returns 200 (not 404)
3. Verify ErrorLoggingMiddleware is registered in Program.cs

### Clear button not working
1. Check network tab for DELETE request
2. Verify backend Debug controller is accessible
3. Check browser console for errors

## 📱 Mobile/Responsive

The error monitor is fully responsive:
- **Desktop**: Modal with tabs and search
- **Tablet**: Slightly smaller modal
- **Mobile**: Full-screen overlay

## 🎓 Best Practices

1. ✅ **Keep monitoring during active development**
2. ✅ **Review errors before committing code**
3. ✅ **Export complex errors for team review**
4. ✅ **Clear errors after fixing issues**
5. ✅ **Use categories to prioritize fixes**
6. ❌ **Don't ignore validation errors**
7. ❌ **Don't leave auto-refresh ON overnight**

## 🔗 Related Resources

- [Full Implementation Docs](./ERROR_MONITOR_IMPLEMENTATION.md)
- [Frontend Auth Guide](./FRONTEND_AUTH_IMPLEMENTATION.md)
- [Backend API Docs](../prompts/Apidocumentation.md)

---

**Happy Debugging! 🐞✨**
