# Class Management API Documentation

**Base URL:** `/api/Class`  
**Module:** Class Management (School Classes)  
**Status:** ✅ Fully Implemented (Backend + Frontend)

---

## Overview
The Class Management API provides CRUD operations for managing school classes. Each class belongs to a school and can have multiple students and teachers assigned.

## Frontend Integration
- **Hook File:** [Frontend/src/hooks/useClasses.tsx](../../Frontend/src/hooks/useClasses.tsx)
- **Status:** ✅ **WORKING** - Complete integration with React Query
- **Used By:** Class management dashboard, Class assignment views

---

## Endpoints

### 1. GET `/api/Class`
**Get all classes for the authenticated user's school**

#### Access
- 🔒 **Requires Authentication**
- 🛡️ **Policy:** `AdminOrSchoolAdmin`
- **Allowed Roles:** Admin, SchoolAdmin

#### Request
No body required. SchoolId is extracted from JWT token claims.

#### Success Response (200 OK)
```json
{
  "content": [
    {
      "id": "guid",
      "className": "string",
      "year": 2026,
      "section": "string",
      "schoolId": "guid",
      "classIncharge": "string | null",
      "createdDate": "2026-01-12"
    }
  ],
  "isSuccess": true,
  "statusCode": 200,
  "errorMessage": null
}
```

#### Error Responses

**401 Unauthorized - Invalid User ID**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 401,
  "errorMessage": "Invalid or missing user ID."
}
```

**404 Not Found - User Not Found**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 404,
  "errorMessage": "User not found."
}
```

**400 Bad Request - No School Assigned**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 400,
  "errorMessage": "User does not have a SchoolId assigned."
}
```

#### Frontend Implementation
```typescript
// From useClasses.tsx
const { data, isLoading, error, refetch } = useQuery({
  queryKey: ["classes"],
  queryFn: getAllClasses
});
```

---

### 2. GET `/api/Class/{id}`
**Get a specific class by ID**

#### Access
- 🔒 **Requires Authentication**
- 🛡️ **Policy:** `AdminOrSchoolAdmin`
- **Allowed Roles:** Admin, SchoolAdmin

#### Path Parameters
- `id` (guid) - The unique identifier of the class

#### Success Response (200 OK)
```json
{
  "content": {
    "id": "guid",
    "className": "string",
    "year": 2026,
    "section": "string",
    "schoolId": "guid",
    "classIncharge": "string | null",
    "createdDate": "2026-01-12"
  },
  "isSuccess": true,
  "statusCode": 200,
  "errorMessage": null
}
```

#### Error Response (404 Not Found)
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 404,
  "errorMessage": "Class with this Id not found"
}
```

---

### 3. POST `/api/Class`
**Create a new class**

#### Access
- 🔒 **Requires Authentication**
- 🛡️ **Policy:** `AdminOrSchoolAdmin`
- **Allowed Roles:** Admin, SchoolAdmin

#### Request Body
```json
{
  "className": "string",        // Required
  "year": 2026,                 // Required
  "section": "string",          // Required
  "classIncharge": "string"     // Optional
}
```

**Note:** `schoolId` is automatically injected from the user's JWT token claims.

#### Success Response (200 OK)
```json
{
  "content": {
    "id": "guid",               // Auto-generated
    "className": "Grade 10",
    "year": 2026,
    "section": "A",
    "schoolId": "guid",          // From token
    "classIncharge": "John Doe",
    "createdDate": "2026-01-12"
  },
  "isSuccess": true,
  "statusCode": 200,
  "errorMessage": null
}
```

#### Error Responses

**400 Bad Request - Validation Error**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 400,
  "errorMessage": "ClassName is required | Year must be a positive number"
}
```

**401 Unauthorized - Missing SchoolId**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 401,
  "errorMessage": "Missing or invalid SchoolId in token."
}
```

#### Validation Rules
- ✅ `className`: Required, non-empty string
- ✅ `year`: Required, positive integer
- ✅ `section`: Required, non-empty string
- ✅ `classIncharge`: Optional string

#### Frontend Implementation
```typescript
// From useClasses.tsx
const addClass = useMutation({
  mutationFn: addNewClass,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["classes"] });
  }
});

// Usage
addClass.mutate({
  className: "Grade 10",
  year: 2026,
  section: "A",
  classIncharge: "John Doe"
});
```

---

### 4. PUT `/api/Class/{id}`
**Update an existing class**

#### Access
- 🔒 **Requires Authentication**
- 🛡️ **Policy:** `AdminOrSchoolAdmin`
- **Allowed Roles:** Admin, SchoolAdmin

#### Path Parameters
- `id` (guid) - The unique identifier of the class to update

#### Request Body
```json
{
  "className": "string",        // Required
  "year": 2026,                 // Required
  "section": "string",          // Required
  "classIncharge": "string"     // Optional
}
```

#### Success Response (200 OK)
```json
{
  "content": {
    "id": "guid",
    "className": "Grade 11",     // Updated
    "year": 2026,
    "section": "B",              // Updated
    "schoolId": "guid",
    "classIncharge": "Jane Smith", // Updated
    "createdDate": "2026-01-12"
  },
  "isSuccess": true,
  "statusCode": 200,
  "errorMessage": null
}
```

#### Error Responses

**404 Not Found**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 404,
  "errorMessage": "Class with this Id not found"
}
```

**400 Bad Request - Validation Error**
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 400,
  "errorMessage": "Validation error message"
}
```

#### Frontend Implementation
```typescript
// From useClasses.tsx
const editClass = useMutation({
  mutationFn: editOldClass,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["classes"] });
  }
});

// Usage
editClass.mutate({
  id: "class-guid-here",
  className: "Grade 11",
  year: 2026,
  section: "B",
  classIncharge: "Jane Smith"
});
```

---

### 5. DELETE `/api/Class/{id}`
**Delete a class**

#### Access
- 🔒 **Requires Authentication**
- 🛡️ **Policy:** `AdminOrSchoolAdmin`
- **Allowed Roles:** Admin, SchoolAdmin

#### Path Parameters
- `id` (guid) - The unique identifier of the class to delete

#### Success Response (200 OK)
```json
{
  "content": {
    "id": "guid",
    "className": "Grade 10",
    "year": 2026,
    "section": "A",
    "schoolId": "guid",
    "classIncharge": "John Doe",
    "createdDate": "2026-01-12"
  },
  "isSuccess": true,
  "statusCode": 200,
  "errorMessage": null
}
```

#### Error Response (404 Not Found)
```json
{
  "content": null,
  "isSuccess": false,
  "statusCode": 404,
  "errorMessage": "Class with this Id not found"
}
```

#### ⚠️ Cascade Considerations
- Check if students are assigned to this class before deletion
- Consider soft delete instead of hard delete for data integrity

#### Frontend Implementation
```typescript
// From useClasses.tsx
const deleteClass = useMutation({
  mutationFn: deleteOldClass,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["classes"] });
  }
});

// Usage
deleteClass.mutate("class-guid-here");
```

---

## Frontend Integration Details

### React Query Hook: `useClasses`

#### File Location
`Frontend/src/hooks/useClasses.tsx`

#### Hook Structure
```typescript
export function useClasses() {
  const queryClient = useQueryClient();

  // Fetch all classes
  const query = useQuery({
    queryKey: ["classes"],
    queryFn: getAllClasses
  });

  // Add new class
  const addClass = useMutation({
    mutationFn: addNewClass,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    }
  });

  // Edit existing class
  const editClass = useMutation({
    mutationFn: editOldClass,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    }
  });

  // Delete class
  const deleteClass = useMutation({
    mutationFn: deleteOldClass,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    }
  });

  return { 
    ...query,
    addClass, 
    editClass, 
    deleteClass 
  };
}
```

#### Features
- ✅ Automatic cache invalidation after mutations
- ✅ Loading states for all operations
- ✅ Error handling
- ✅ Optimistic updates support
- ✅ Cookie-based authentication

---

## Security Features

### ✅ Implemented
1. **Role-Based Access Control**
   - Only Admin and SchoolAdmin can manage classes
   - Enforced via `AdminOrSchoolAdmin` policy

2. **School Isolation**
   - Users can only see/manage classes from their school
   - SchoolId is extracted from JWT token (not user input)
   - Prevents cross-school data access

3. **Token Validation**
   - JWT authentication required
   - SchoolId claim validation

4. **Input Validation**
   - FluentValidation for request DTOs
   - Model state validation
   - Type safety with strongly-typed models

---

## Data Model

### SchoolClass Entity
```csharp
public class SchoolClass
{
    public Guid Id { get; set; }           // Primary key
    public string ClassName { get; set; }   // e.g., "Grade 10"
    public int Year { get; set; }          // e.g., 2026
    public string Section { get; set; }     // e.g., "A", "B", "C"
    public Guid SchoolId { get; set; }     // Foreign key to School
    public string? ClassIncharge { get; set; } // Optional teacher name
    public DateOnly CreatedDate { get; set; }  // Record creation date
    
    // Navigation properties
    public School School { get; set; }
    public ICollection<Student> Students { get; set; }
}
```

---

## Usage Examples

### Frontend Component Example
```typescript
import { useClasses } from '@/hooks/useClasses';

function ClassManagement() {
  const { data, isLoading, error, addClass, editClass, deleteClass } = useClasses();

  const handleAddClass = () => {
    addClass.mutate({
      className: "Grade 10",
      year: 2026,
      section: "A",
      classIncharge: "John Doe"
    });
  };

  const handleEditClass = (classId: string) => {
    editClass.mutate({
      id: classId,
      className: "Grade 11",
      year: 2026,
      section: "B",
      classIncharge: "Jane Smith"
    });
  };

  const handleDeleteClass = (classId: string) => {
    if (confirm("Are you sure?")) {
      deleteClass.mutate(classId);
    }
  };

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      {data.map(cls => (
        <div key={cls.id}>
          {cls.className} - {cls.section} ({cls.year})
        </div>
      ))}
    </div>
  );
}
```

---

## Testing Recommendations

### Manual Testing Checklist
- [ ] Login as SchoolAdmin
- [ ] Create a new class with valid data
- [ ] Create a class with missing required fields (should fail)
- [ ] Get list of all classes (should only show classes from user's school)
- [ ] Get a specific class by ID
- [ ] Update a class
- [ ] Delete a class
- [ ] Verify non-admin users cannot access endpoints
- [ ] Verify users from different schools cannot see each other's classes

### Test Data
```json
{
  "className": "Grade 10",
  "year": 2026,
  "section": "A",
  "classIncharge": "Mr. John Smith"
}
```

---

## Known Issues & Considerations

### ⚠️ Current Limitations
1. **No Soft Delete** - Deleted classes are permanently removed
2. **No Cascade Handling** - Deleting a class doesn't handle related students/attendance
3. **No Duplicate Check** - Same className + section + year can be created multiple times
4. **No Pagination** - All classes are returned at once (could be issue for large schools)
5. **ClassIncharge** is a string, not a foreign key to Teacher entity

### 🔄 Recommended Improvements
1. **Add Soft Delete** - Mark classes as inactive instead of deleting
2. **Add Duplicate Validation** - Prevent duplicate class entries
3. **Implement Pagination** - For schools with many classes
4. **Link ClassIncharge to Teacher** - Use proper foreign key relationship
5. **Add Capacity Management** - Max students per class
6. **Add Active/Inactive Status** - For class lifecycle management
7. **Add Class Schedule Support** - Timetable integration

---

## Related APIs

### Dependencies
- **School API** - SchoolId foreign key relationship
- **Student API** - Students are assigned to classes
- **Teacher API** - Teachers can be class incharges
- **Attendance API** - Attendance is tracked per class

### Frontend Components Using This API
- Class List View
- Class Creation Dialog ([Frontend/src/popups/classes/AddClassPopup.tsx](../../Frontend/src/popups/classes/AddClassPopup.tsx))
- Class Edit Dialog ([Frontend/src/popups/classes/EditClassPopup.tsx](../../Frontend/src/popups/classes/EditClassPopup.tsx))
- Class View Dialog ([Frontend/src/popups/classes/ViewClassPopup.tsx](../../Frontend/src/popups/classes/ViewClassPopup.tsx))

---

## Error Codes Summary

| Status Code | Scenario |
|-------------|----------|
| 200 | Success |
| 400 | Bad request (validation errors, missing schoolId) |
| 401 | Unauthorized (invalid/missing token, no SchoolId claim) |
| 403 | Forbidden (insufficient permissions - not Admin/SchoolAdmin) |
| 404 | Class not found |

---

**Last Updated:** January 12, 2026  
**API Version:** v1  
**Backend Status:** ✅ Fully Implemented  
**Frontend Status:** ✅ Fully Integrated  
**Testing Status:** ⚠️ Requires database setup for live testing
