# Problem-Solution Database
**Generic Problem Patterns & Solutions**

## Purpose
Document recurring development problems in an abstract, reusable way. This saves debugging time by checking common causes first before deep analysis.

## How to Use This File
1. **When you encounter an error:** Check this file first for similar problem patterns
2. **Verify common causes:** Try the suggested solutions in order
3. **If unresolved:** Proceed with deeper analysis
4. **After resolution:** Add the generic pattern here (not the specific error message)

---

## Compilation & Build Errors

### Pattern: Class/Interface creation fails
**Problem Type:** New entity, model, or class cannot be created or recognized

**Possible Root Causes:**
- Missing namespace imports or using statements
- Incomplete type definition (missing closing braces, semicolons)
- Circular dependency between classes
- Namespace mismatch between file location and declared namespace
- Missing project reference in .csproj file

**Suggested Solutions:**
1. Check if all required namespaces are imported
2. Verify class/interface has complete syntax (opening/closing braces)
3. Ensure namespace matches folder structure
4. Check if dependent project is referenced in .csproj
5. Clean and rebuild solution (`dotnet clean` then `dotnet build`)

---

### Pattern: Method/property not recognized in controller
**Problem Type:** Controller cannot access service methods or properties

**Possible Root Causes:**
- Service interface not registered in dependency injection
- Wrong interface/implementation passed to constructor
- Method is private or internal instead of public
- Service not added to ServiceCollection
- Wrong namespace imported

**Suggested Solutions:**
1. Verify service is registered in Program.cs or ServiceCollectionExtensions
2. Check constructor parameter type matches registered service
3. Ensure method/property is public
4. Verify correct using statement for service interface
5. Check if service implementation exists

---

### Pattern: Async method issues
**Problem Type:** Async methods not working as expected, warnings about unawaited tasks

**Possible Root Causes:**
- Missing `await` keyword before async method call
- Method signature missing `async` keyword
- Return type is `Task<T>` but not awaited
- Deadlock due to `.Result` or `.Wait()` usage
- Async method called synchronously

**Suggested Solutions:**
1. Add `await` before all Task-returning methods
2. Add `async` keyword to method signature
3. Change return type to `Task` or `Task<T>`
4. Replace `.Result`/`.Wait()` with `await`
5. Ensure async all the way (don't mix sync/async)

---

### Pattern: Type conversion failures
**Problem Type:** Cannot convert between types, implicit conversion errors

**Possible Root Causes:**
- Missing type cast or conversion method
- Incompatible types (no inheritance/interface relationship)
- Async Task not awaited (Task<T> vs T mismatch)
- Nullable vs non-nullable type mismatch
- Collection type mismatch (IEnumerable vs List)

**Suggested Solutions:**
1. Add explicit cast: `(TargetType)source`
2. Use conversion method: `.ToString()`, `.Parse()`, `.Convert()`
3. Add `await` if source is Task<T>
4. Handle null cases with `??` or `?.`
5. Use `.ToList()` or `.ToArray()` for collections

---

## Database & EF Core Errors

### Pattern: Migration creation or application fails
**Problem Type:** Cannot create or apply database migrations

**Possible Root Causes:**
- DbContext not registered in dependency injection
- Multiple DbContext classes causing ambiguity
- Connection string incorrect or inaccessible
- Database is locked or in use
- Foreign key constraint violations in existing data
- Incompatible schema changes

**Suggested Solutions:**
1. Verify DbContext registered: `builder.Services.AddDbContext<>()`
2. Specify DbContext if multiple exist: `-Context ContextName`
3. Check connection string in appsettings.json
4. Close all database connections (SSMS, Azure Data Studio)
5. Backup and drop database if clean slate needed
6. Review migration code for breaking changes

---

### Pattern: Query returns wrong data or no data
**Problem Type:** Database query executes but returns unexpected results

**Possible Root Causes:**
- Missing filter conditions (especially SchoolId in multi-tenant apps)
- Wrong property used in Where clause
- Case sensitivity in string comparisons
- Deleted/soft-deleted records included
- Eager/lazy loading issues with navigation properties
- Data actually missing in database

**Suggested Solutions:**
1. Add missing filter conditions (check SchoolId requirement)
2. Verify property names match database columns
3. Use `.ToLower()` for case-insensitive comparisons
4. Check for IsDeleted or IsActive filters
5. Add `.Include()` for navigation properties
6. Query database directly (SQL) to verify data exists

---

### Pattern: Foreign key constraint violations
**Problem Type:** Cannot insert, update, or delete due to foreign key constraints

**Possible Root Causes:**
- Parent record doesn't exist (invalid FK value)
- Child records exist (can't delete parent)
- Cascade delete not configured
- FK value is null when required
- Wrong Guid being used for relationship

**Suggested Solutions:**
1. Verify parent record exists before insert/update
2. Delete child records first, then parent
3. Configure cascade delete in OnModelCreating
4. Check FK property is assigned valid Guid
5. Use `.Include()` to load relationships before delete

---

### Pattern: Connection failures to database
**Problem Type:** Application cannot connect to database

**Possible Root Causes:**
- Connection string incorrect (server, database, credentials)
- Database server not running
- Network/firewall blocking connection
- Azure SQL requires IP whitelisting
- SSL/TLS certificate issues
- Connection pool exhausted

**Suggested Solutions:**
1. Verify connection string format and values
2. Test connection with SQL client (SSMS, sqlcmd)
3. Check if database service is running
4. Add IP to Azure SQL firewall rules
5. Add `TrustServerCertificate=True` if local dev
6. Restart application to reset connection pool

---

## API & Controller Errors

### Pattern: 401 Unauthorized responses
**Problem Type:** API returns 401 even with valid credentials

**Possible Root Causes:**
- JWT token expired
- Token not included in Authorization header
- Token signature validation failed
- JWT secret key mismatch
- Authentication middleware not registered
- Token format incorrect (missing "Bearer " prefix)

**Suggested Solutions:**
1. Check token expiration (`exp` claim)
2. Verify Authorization header: `Bearer <token>`
3. Ensure JWT secret matches in appsettings.json
4. Check middleware order: Authentication before Authorization
5. Decode token at jwt.io to verify claims
6. Regenerate token with correct credentials

---

### Pattern: 403 Forbidden responses
**Problem Type:** Authenticated user cannot access resource

**Possible Root Causes:**
- User lacks required role
- Policy requirements not met
- Missing claims in JWT token (e.g., SchoolId)
- Cross-tenant access attempt (multi-tenant apps)
- Authorization policy not configured
- Endpoint requires higher privilege level

**Suggested Solutions:**
1. Check user roles against required roles
2. Verify all required claims present in token
3. Ensure SchoolId/tenant validation in middleware
4. Check authorization policy configuration
5. Log user claims for debugging
6. Verify user assigned correct roles in database

---

### Pattern: 404 Not Found when resource exists
**Problem Type:** API returns 404 but data exists in database

**Possible Root Causes:**
- Wrong route URL or parameter
- Tenant/SchoolId mismatch (multi-tenant isolation)
- Soft-deleted records filtered out
- Wrong HTTP method used (GET vs POST)
- Route parameter type mismatch
- Query filter too restrictive

**Suggested Solutions:**
1. Verify URL matches route template
2. Check SchoolId filter in query
3. Include soft-deleted records if appropriate
4. Confirm HTTP method matches endpoint
5. Ensure route parameter type correct (Guid, int, etc.)
6. Test query directly in database to verify data

---

### Pattern: 500 Internal Server Error
**Problem Type:** Server error with no specific details

**Possible Root Causes:**
- Null reference exception
- Unhandled exception in business logic
- Database query failure
- Missing dependency injection registration
- Type conversion error
- Infinite loop or stack overflow

**Suggested Solutions:**
1. Check application logs for stack trace
2. Add null checks before property access
3. Wrap risky code in try-catch blocks
4. Verify all services registered in DI
5. Enable detailed errors in development
6. Use debugger to identify exact failure point

---

### Pattern: Request validation failures
**Problem Type:** Request rejected due to validation errors

**Possible Root Causes:**
- Required fields missing in request body
- Data type mismatch (string vs number)
- Validation rules not met (length, format, range)
- Null values where not allowed
- Enum value invalid
- Date format incorrect

**Suggested Solutions:**
1. Check all required properties populated
2. Verify data types match DTO definitions
3. Review FluentValidation rules or Data Annotations
4. Handle optional fields with nullable types
5. Use valid enum values or integers
6. Use ISO 8601 format for dates

---

## Frontend & API Integration Errors

### Pattern: CORS policy blocking requests
**Problem Type:** Browser blocks API calls due to CORS policy

**Possible Root Causes:**
- Backend CORS policy not configured
- Frontend origin not in allowed origins
- Preflight request failing
- Credentials not allowed in CORS policy
- Wrong HTTP method or headers

**Suggested Solutions:**
1. Add CORS policy in backend Program.cs
2. Add frontend URL to WithOrigins()
3. Add AllowCredentials() if using cookies/auth
4. Add AllowAnyMethod() and AllowAnyHeader()
5. Ensure CORS middleware before UseAuthorization()
6. Check browser console for specific CORS error

---

### Pattern: API call returns undefined/null
**Problem Type:** Frontend receives unexpected null or undefined response

**Possible Root Causes:**
- API endpoint returned null/empty response
- Response structure doesn't match expected type
- Async call not awaited properly
- Error occurred but not caught
- Wrong API endpoint URL
- Response parsing failed

**Suggested Solutions:**
1. Check network tab for actual response
2. Verify response structure matches TypeScript interface
3. Add await to async API calls
4. Add error handling (try-catch or .catch())
5. Verify API URL matches backend route
6. Check if response is JSON formatted

---

### Pattern: State not updating in React components
**Problem Type:** Component state change doesn't trigger re-render

**Possible Root Causes:**
- Mutating state directly instead of using setState
- State update is asynchronous and not waited
- React Query cache not invalidating
- Component not re-rendering due to same reference
- Wrong state variable being updated
- Dependency array missing in useEffect

**Suggested Solutions:**
1. Use setState/useState properly (immutable updates)
2. Use callback form of setState if needed
3. Invalidate React Query cache after mutations
4. Create new object/array references for state
5. Verify correct state variable name
6. Add all dependencies to useEffect array

---

## Authentication & Authorization Errors

### Pattern: Token refresh not working
**Problem Type:** Expired token not refreshing automatically

**Possible Root Causes:**
- Refresh token expired
- Refresh token endpoint not called
- Interceptor not configured for 401 responses
- Refresh token not stored properly
- Infinite refresh loop
- Token storage cleared prematurely

**Suggested Solutions:**
1. Check refresh token expiration
2. Implement axios/fetch interceptor for 401
3. Store refresh token in secure storage
4. Add retry logic with flag to prevent loops
5. Verify refresh endpoint returns new tokens
6. Clear tokens only on refresh failure

---

### Pattern: User logged out unexpectedly
**Problem Type:** User session ends without user action

**Possible Root Causes:**
- Token expired and no refresh mechanism
- LocalStorage/SessionStorage cleared
- Browser cleared cookies/storage
- Token blacklisted on backend
- Concurrent login limit reached
- Server session expired

**Suggested Solutions:**
1. Implement token refresh before expiration
2. Use longer token expiration times
3. Store tokens securely (not in vulnerable storage)
4. Check if backend invalidated token
5. Allow multiple sessions or notify user
6. Implement session heartbeat mechanism

---

## Real-time Communication Errors (SignalR/WebSocket)

### Pattern: WebSocket connection fails
**Problem Type:** Cannot establish real-time connection

**Possible Root Causes:**
- WebSocket not enabled on server
- CORS not configured for WebSocket
- Proxy/firewall blocking WebSocket port
- SSL/TLS certificate issues
- Hub route not registered
- Connection token invalid

**Suggested Solutions:**
1. Enable WebSocket in server configuration
2. Add WebSocket to CORS policy
3. Test without proxy or configure proxy for WebSocket
4. Use valid SSL certificate or disable verification (dev only)
5. Register hub: `app.MapHub<ChatHub>("/chathub")`
6. Pass valid JWT token to connection

---

### Pattern: Real-time messages not received
**Problem Type:** Messages sent but not received by clients

**Possible Root Causes:**
- Client not subscribed to correct group/channel
- Connection lost but not detected
- Method name mismatch between server and client
- Client handler not registered
- Message sent to wrong client/group
- Connection not fully established before sending

**Suggested Solutions:**
1. Verify client joined correct group
2. Implement connection lost handling
3. Match method names exactly (case-sensitive)
4. Register client handler before starting connection
5. Check recipient identifier (connection ID, group name)
6. Add connection state check before sending

---

## Performance Issues

### Pattern: Slow database queries
**Problem Type:** API responses take too long due to database operations

**Possible Root Causes:**
- Missing database indexes
- N+1 query problem (lazy loading)
- Large dataset returned without pagination
- Inefficient LINQ queries
- Missing query optimization
- Database fragmentation

**Suggested Solutions:**
1. Add indexes on frequently queried columns
2. Use `.Include()` for eager loading
3. Implement pagination (Take/Skip)
4. Review LINQ to SQL translation
5. Use `.AsNoTracking()` for read-only queries
6. Run query performance analysis in SQL

---

### Pattern: Memory leaks or high memory usage
**Problem Type:** Application memory continuously increases

**Possible Root Causes:**
- Event handlers not unsubscribed
- Large objects not disposed
- DbContext not disposed properly
- Circular references preventing garbage collection
- Caching too much data
- Background tasks accumulating

**Suggested Solutions:**
1. Unsubscribe from events when done
2. Implement IDisposable and use using statements
3. Use scoped lifetime for DbContext
4. Break circular references
5. Implement cache expiration policies
6. Clean up background tasks properly

---

## Deployment & Environment Errors

### Pattern: Works locally but fails in production
**Problem Type:** Code runs fine in development but errors in production

**Possible Root Causes:**
- Environment variables not set
- Configuration differences
- Connection strings pointing to wrong database
- Missing dependencies or DLLs
- File paths hardcoded for local system
- CORS configured only for localhost

**Suggested Solutions:**
1. Verify all environment variables set in production
2. Check appsettings.Production.json exists and is correct
3. Update connection strings for production database
4. Ensure all NuGet packages published with app
5. Use relative or configuration-based paths
6. Add production URL to CORS policy

---

### Pattern: Deployment succeeds but app doesn't start
**Problem Type:** Deployment completes but application fails to run

**Possible Root Causes:**
- Missing runtime dependencies
- Configuration file missing or invalid
- Database migrations not applied
- Port already in use
- Insufficient permissions
- Environment mismatch (.NET version)

**Suggested Solutions:**
1. Check runtime is installed (correct .NET version)
2. Verify appsettings.json deployed correctly
3. Run migrations manually: `dotnet ef database update`
4. Change port or stop conflicting process
5. Check file/folder permissions
6. Match development and production .NET versions

---

## File & Resource Errors

### Pattern: Static files not served
**Problem Type:** Images, CSS, JS files return 404

**Possible Root Causes:**
- Static files middleware not configured
- Files not in correct directory (wwwroot)
- Build process not copying files
- Path case sensitivity (Linux servers)
- MIME type not configured
- Files not included in publish output

**Suggested Solutions:**
1. Add `app.UseStaticFiles()` in Program.cs
2. Place files in wwwroot folder
3. Check csproj includes files in Content
4. Use correct case for file paths
5. Configure MIME types if needed
6. Verify files exist in publish folder

---

## Testing Errors

### Pattern: Tests pass individually but fail together
**Problem Type:** Unit tests succeed in isolation but fail when run as suite

**Possible Root Causes:**
- Shared state between tests
- Database not reset between tests
- Mock configurations conflicting
- Order-dependent tests
- Resource locks not released
- Static variables not cleared

**Suggested Solutions:**
1. Make tests independent (no shared state)
2. Use separate test database or in-memory DB
3. Reset mocks in test setup/teardown
4. Avoid test dependencies on execution order
5. Dispose resources properly
6. Avoid static variables or reset them

---

## How to Add New Entries

When you encounter and resolve a problem:

1. **Abstract the problem** - Don't write "Error: NullReferenceException at line 42"
   - ✅ Write: "Null reference errors in data access layer"

2. **Identify generic root causes** - What could cause this type of problem?
   - Database query returns null
   - Property not initialized
   - Missing null check

3. **List solution patterns** - How to verify/fix?
   - Add null checks before property access
   - Use FirstOrDefaultAsync and handle null
   - Initialize properties in constructor

4. **Keep it searchable** - Use clear category headers and problem types

---

## Maintenance Notes

- **Review quarterly** - Remove outdated entries, add new common patterns
- **Keep generic** - If entry becomes too specific, rewrite it abstractly
- **Prioritize by frequency** - Most common problems should be at the top of each section
- **Link related patterns** - Reference related entries when applicable

---

**Last Updated:** January 15, 2026  
**Entries:** 30+ problem patterns  
**Purpose:** First-check resource before deep debugging
