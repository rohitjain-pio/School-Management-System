# Code Review Checklist: Frontend (React/TypeScript)
**Review Type:** Pull Request  
**Estimated Time:** 10-15 minutes per PR  
**Severity Scale:** P0 (Critical) → P5 (Nice to have)

---

## 🔴 P0: CRITICAL - Must Fix Before Merge

### Security: XSS Prevention

```powershell
# Automated check
cd Frontend
Select-String -Path "src/**/*.tsx" -Pattern "dangerouslySetInnerHTML"
```

- [ ] **No dangerouslySetInnerHTML without sanitization**
  - ❌ Violation = XSS vulnerability
  ```typescript
  // ❌ WRONG - XSS vulnerability
  <div dangerouslySetInnerHTML={{ __html: userInput }} />
  
  // ✅ CORRECT - Sanitized
  import DOMPurify from 'dompurify';
  <div dangerouslySetInnerHTML={{ 
    __html: DOMPurify.sanitize(userInput) 
  }} />
  
  // ✅ BEST - Let React escape
  <div>{userInput}</div>
  ```

- [ ] **No innerHTML or outerHTML usage**
  ```typescript
  // ❌ WRONG
  element.innerHTML = userInput;
  
  // ✅ CORRECT
  element.textContent = userInput;
  ```

---

### Type Safety

- [ ] **No `any` types** (use specific types or `unknown`)
  ```typescript
  // ❌ WRONG
  const handleSubmit = (data: any) => { }
  
  // ✅ CORRECT
  const handleSubmit = (data: CreateStudentDto) => { }
  ```

- [ ] **All API responses typed**
  ```typescript
  // ✅ CORRECT
  const response = await api.get<Student[]>('/students');
  const students: Student[] = response.data;
  ```

- [ ] **Props interfaces defined**
  ```typescript
  interface StudentFormProps {
    studentId?: string;
    onSuccess?: () => void;
  }
  
  export function StudentForm({ studentId, onSuccess }: StudentFormProps) {
    // ...
  }
  ```

---

### Authentication

- [ ] **Protected routes wrapped with auth guard**
  ```typescript
  // ✅ REQUIRED
  <Route 
    path="/students" 
    element={
      <ProtectedRoute>
        <StudentList />
      </ProtectedRoute>
    } 
  />
  ```

- [ ] **JWT token stored securely** (not localStorage)
  - Use: httpOnly cookies or secure sessionStorage
  - ❌ Avoid: `localStorage.setItem('token', jwt)` (XSS risk)

- [ ] **Unauthorized responses handled** (redirect to login)
  ```typescript
  // In API interceptor
  if (error.response?.status === 401) {
    authService.logout();
    navigate('/login');
  }
  ```

---

## 🟠 P1: HIGH - Must Fix Before Merge

### Form Validation

- [ ] **All forms use React Hook Form + Zod**
  ```typescript
  import { useForm } from 'react-hook-form';
  import { zodResolver } from '@hookform/resolvers/zod';
  
  const { register, handleSubmit, formState: { errors } } = useForm({
    resolver: zodResolver(createStudentSchema), // ✅ REQUIRED
  });
  ```

- [ ] **Validation schema matches backend rules**
  ```typescript
  export const createStudentSchema = z.object({
    firstName: z.string()
      .min(1, 'First name is required')
      .max(100, 'First name cannot exceed 100 characters'), // ✅ Match backend
  });
  ```

- [ ] **Error messages displayed to user**
  ```typescript
  {errors.firstName && (
    <p className="text-sm text-destructive">{errors.firstName.message}</p>
  )}
  ```

- [ ] **Submit button disabled during submission**
  ```typescript
  <Button type="submit" disabled={isSubmitting}>
    {isSubmitting ? 'Saving...' : 'Save'}
  </Button>
  ```

---

### API Integration (TanStack Query)

- [ ] **All API calls use TanStack Query** (no raw fetch/axios)
  ```typescript
  // ✅ CORRECT
  const { data, isLoading, error } = useQuery({
    queryKey: ['students', schoolId],
    queryFn: studentService.getAll,
  });
  
  // ❌ WRONG - Direct API call
  useEffect(() => {
    fetch('/api/students').then(res => res.json()).then(setStudents);
  }, []);
  ```

- [ ] **Query keys properly structured**
  ```typescript
  // ✅ CORRECT - Hierarchical
  ['students'] // All students
  ['students', { schoolId }] // School-specific
  ['students', studentId] // Single student
  
  // ❌ WRONG - Flat
  ['getStudents']
  ['student123']
  ```

- [ ] **Mutations invalidate relevant queries**
  ```typescript
  const mutation = useMutation({
    mutationFn: studentService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] }); // ✅ REQUIRED
    },
  });
  ```

- [ ] **Loading states shown**
  ```typescript
  if (isLoading) {
    return <LoadingSpinner />;
  }
  ```

- [ ] **Error states shown**
  ```typescript
  if (error) {
    return <ErrorMessage message={error.message} />;
  }
  ```

---

### UI/UX Standards

- [ ] **Empty states provided** (no data message)
  ```typescript
  if (!data || data.length === 0) {
    return (
      <EmptyState 
        title="No Students Found"
        description="Get started by adding your first student."
        action={<Button onClick={onAdd}>Add Student</Button>}
      />
    );
  }
  ```

- [ ] **Responsive design** (mobile-friendly)
  ```typescript
  // ✅ Use Tailwind responsive classes
  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  ```

- [ ] **Accessible labels** (for screen readers)
  ```typescript
  <Label htmlFor="firstName">First Name</Label>
  <Input id="firstName" {...register('firstName')} />
  ```

---

## 🟡 P2: MEDIUM - Fix Within 24 Hours

### Performance

- [ ] **Code splitting for large components**
  ```typescript
  import { lazy, Suspense } from 'react';
  
  const StudentList = lazy(() => import('@/components/students/student-list'));
  
  <Suspense fallback={<LoadingSpinner />}>
    <StudentList />
  </Suspense>
  ```

- [ ] **Memoization for expensive computations**
  ```typescript
  const sortedStudents = useMemo(
    () => students.sort((a, b) => a.lastName.localeCompare(b.lastName)),
    [students]
  );
  ```

- [ ] **Debounced search inputs**
  ```typescript
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch = useDebounce(searchTerm, 500);
  
  useQuery({
    queryKey: ['students', debouncedSearch],
    queryFn: () => studentService.search(debouncedSearch),
  });
  ```

- [ ] **Pagination for large lists** (> 50 items)
  ```typescript
  const { data } = useQuery({
    queryKey: ['students', page],
    queryFn: () => studentService.getAll({ pageNumber: page, pageSize: 50 }),
  });
  ```

---

### Code Quality

- [ ] **No unused imports**
  ```powershell
  # ESLint should catch this
  npm run lint
  ```

- [ ] **Consistent naming conventions**
  - Components: PascalCase (`StudentForm`)
  - Functions: camelCase (`handleSubmit`)
  - Constants: UPPER_SNAKE_CASE (`API_BASE_URL`)
  - Files: kebab-case (`student-form.tsx`)

- [ ] **No console.log in production code**
  ```typescript
  // ❌ WRONG
  console.log('Student data:', student);
  
  // ✅ CORRECT - Remove or use proper logging
  // (or use environment check)
  if (import.meta.env.DEV) {
    console.log('Student data:', student);
  }
  ```

---

### State Management

- [ ] **useState for local component state**
  ```typescript
  const [isOpen, setIsOpen] = useState(false);
  ```

- [ ] **TanStack Query for server state** (not useState)
  ```typescript
  // ✅ CORRECT
  const { data: students } = useQuery({
    queryKey: ['students'],
    queryFn: studentService.getAll,
  });
  
  // ❌ WRONG
  const [students, setStudents] = useState([]);
  useEffect(() => {
    studentService.getAll().then(setStudents);
  }, []);
  ```

- [ ] **useContext for global app state** (theme, auth)
  ```typescript
  const { user } = useAuth(); // ✅ Context hook
  ```

---

### Error Handling

- [ ] **Toast notifications for API errors**
  ```typescript
  const mutation = useMutation({
    mutationFn: studentService.create,
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });
  ```

- [ ] **Form validation errors shown inline**
  ```typescript
  {errors.email && (
    <p className="text-sm text-destructive">{errors.email.message}</p>
  )}
  ```

- [ ] **Network errors handled gracefully**
  ```typescript
  if (error) {
    return (
      <Card className="border-destructive">
        <CardContent>
          <p>Failed to load students. Please try again.</p>
          <Button onClick={() => refetch()}>Retry</Button>
        </CardContent>
      </Card>
    );
  }
  ```

---

## 🟢 P3: LOW - Fix in Next Sprint

### Testing

- [ ] **Component tests for complex logic**
  ```typescript
  import { render, screen } from '@testing-library/react';
  import { StudentForm } from './student-form';
  
  test('renders form fields', () => {
    render(<StudentForm />);
    expect(screen.getByLabelText('First Name')).toBeInTheDocument();
  });
  ```

- [ ] **Integration tests for form submission**
  ```typescript
  test('creates student on submit', async () => {
    const user = userEvent.setup();
    render(<StudentForm />);
    
    await user.type(screen.getByLabelText('First Name'), 'John');
    await user.click(screen.getByRole('button', { name: 'Save' }));
    
    await waitFor(() => {
      expect(screen.getByText('Student created')).toBeInTheDocument();
    });
  });
  ```

---

### Accessibility

- [ ] **ARIA labels on interactive elements**
  ```typescript
  <button aria-label="Delete student">
    <TrashIcon />
  </button>
  ```

- [ ] **Keyboard navigation support**
  ```typescript
  <Dialog onOpenChange={setOpen}>
    <DialogTrigger asChild>
      <Button>Open</Button>
    </DialogTrigger>
    {/* Dialog handles Escape key automatically */}
  </Dialog>
  ```

- [ ] **Focus management in modals/dialogs**
  - Focus should trap inside modal
  - Focus returns to trigger on close

---

### Documentation

- [ ] **JSDoc comments for complex functions**
  ```typescript
  /**
   * Calculates student's GPA based on enrolled courses
   * @param enrollments - Array of student enrollments
   * @returns GPA value (0.0 - 4.0)
   */
  function calculateGPA(enrollments: Enrollment[]): number {
    // ...
  }
  ```

- [ ] **Storybook stories for reusable components** (optional)

---

## 📋 Review Workflow

### 1. Automated Checks (3 minutes)
```powershell
cd Frontend

# Run linter
npm run lint

# Run type checker
npm run type-check

# Run tests
npm run test

# Build (check for errors)
npm run build
```

### 2. Manual Review (7 minutes)
- [ ] Open each changed component
- [ ] Check for XSS vulnerabilities
- [ ] Verify type safety (no `any`)
- [ ] Check responsive design (resize browser)
- [ ] Test form validation (submit invalid data)
- [ ] Test error states (disconnect network)

### 3. Browser Testing (5 minutes)
- [ ] Test in Chrome DevTools mobile view
- [ ] Check console for errors
- [ ] Verify network requests (API calls)
- [ ] Test keyboard navigation
- [ ] Run Lighthouse audit (Performance + Accessibility)

---

## 🚨 Common Issues Found in PRs

| Issue | Frequency | Severity | Fix Time |
|-------|-----------|----------|----------|
| Missing loading state | 35% | P1 | 3 min |
| No error handling | 30% | P1 | 5 min |
| Using `any` type | 40% | P0 | 5 min |
| No empty state | 25% | P1 | 10 min |
| Not using TanStack Query | 20% | P1 | 15 min |
| Missing form validation | 15% | P1 | 10 min |
| No responsive classes | 20% | P1 | 5 min |
| Direct localStorage for token | 10% | P0 | 20 min |

---

## ✅ Approval Comments Template

**For Approving:**
```markdown
✅ **APPROVED**

**Reviewed:**
- [x] Type safety verified (no `any`)
- [x] XSS prevention checks passed
- [x] Forms properly validated
- [x] API integration correct
- [x] Responsive design tested

**Notes:**
- Excellent use of TanStack Query
- Good loading/error state handling

**Minor Suggestions (P3):**
- Consider adding unit tests for calculateGPA function
```

**For Requesting Changes:**
```markdown
🔄 **CHANGES REQUESTED**

**P0 Issues (MUST FIX):**
- [ ] Line 67: Using `any` type, should be `Student[]`
- [ ] Line 89: dangerouslySetInnerHTML without sanitization

**P1 Issues (MUST FIX):**
- [ ] Line 123: No loading state shown during API call
- [ ] Line 145: Form has no validation schema
- [ ] Line 201: Not using TanStack Query (raw fetch)

**P2 Issues (Fix if time allows):**
- [ ] Line 178: No empty state when students array is empty
- [ ] Consider memoizing sortedStudents calculation

**How to Fix:**
See `.copilot/agents/frontend-agent.md` for patterns.
```

---

## 📊 Code Review Metrics

Track these over time:

- **Average time to review:** Target < 24 hours
- **P0 issues per PR:** Target < 1
- **P1 issues per PR:** Target < 2
- **Test coverage:** Target > 75%
- **Lighthouse Performance Score:** Target > 90

**Save metrics in:** `docs/frontend-review-metrics.md`

---

## 🔍 Quick Security Scan

```powershell
# Save as: Frontend/security-scan.ps1

Write-Host "=== Frontend Security Scan ===" -ForegroundColor Cyan

# 1. Check for XSS vulnerabilities
Write-Host "`n[1/4] Checking for XSS vulnerabilities..." -ForegroundColor Yellow
$xss = Select-String -Path "src/**/*.tsx" -Pattern "dangerouslySetInnerHTML|innerHTML"
if ($xss) {
    Write-Host "⚠️  CRITICAL: Potential XSS vulnerability!" -ForegroundColor Red
    $xss
} else {
    Write-Host "✅ No XSS vulnerabilities found" -ForegroundColor Green
}

# 2. Check for 'any' types
Write-Host "`n[2/4] Checking for 'any' types..." -ForegroundColor Yellow
$anyTypes = Select-String -Path "src/**/*.tsx" -Pattern ": any"
if ($anyTypes) {
    Write-Host "⚠️  Type safety issue: 'any' types found" -ForegroundColor Yellow
    $anyTypes | Select-Object -First 5
} else {
    Write-Host "✅ No 'any' types found" -ForegroundColor Green
}

# 3. Check for localStorage token storage
Write-Host "`n[3/4] Checking for insecure token storage..." -ForegroundColor Yellow
$tokenStorage = Select-String -Path "src/**/*.ts" -Pattern "localStorage\.setItem.*token"
if ($tokenStorage) {
    Write-Host "⚠️  CRITICAL: Token stored in localStorage (XSS risk)!" -ForegroundColor Red
    $tokenStorage
} else {
    Write-Host "✅ No insecure token storage found" -ForegroundColor Green
}

# 4. Check for console.log
Write-Host "`n[4/4] Checking for console.log statements..." -ForegroundColor Yellow
$consoleLogs = git diff main | Select-String -Pattern "console\.(log|error|warn)"
if ($consoleLogs) {
    Write-Host "⚠️  Clean up console statements before merge" -ForegroundColor Yellow
    $consoleLogs | Select-Object -First 5
} else {
    Write-Host "✅ No console statements in changes" -ForegroundColor Green
}

Write-Host "`n=== Scan Complete ===" -ForegroundColor Cyan
```

**Usage:**
```powershell
cd Frontend
./security-scan.ps1
```

---

**Related Files:**
- `.copilot/agents/frontend-agent.md`
- `.copilot/workflows/add-frontend-component.md`
- `.copilot/code-review-checklists/security-checklist.md`
