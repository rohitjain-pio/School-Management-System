# OpenAPI Schema Configuration Guide for SMS Project

## Overview
This guide provides comprehensive OpenAPI schema definitions for the School Management System (SMS) project, optimized for ASP.NET Core backend and TypeScript React frontend code generation.

## Table of Contents
1. [Swashbuckle Configuration](#swashbuckle-configuration)
2. [Schema Components](#schema-components)
3. [OpenAPI Generator Configuration](#openapi-generator-configuration)
4. [Naming Conventions](#naming-conventions)
5. [Common Issues and Solutions](#common-issues-and-solutions)

---

## Swashbuckle Configuration

### Program.cs Configuration
```csharp
// Add to Backend/SMSPrototype1/Program.cs

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Management System API",
        Version = "v1",
        Description = "API for School Management System with Student, Teacher, Class, Attendance, and Chat management",
        Contact = new OpenApiContact
        {
            Name = "SMS Development Team",
            Email = "dev@sms.edu"
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Use XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Schema naming convention
    options.CustomSchemaIds(type => type.Name.Replace("Dto", "").Replace("RqstDto", "Request"));

    // Use PascalCase for property names (C# convention)
    options.DescribeAllParametersInCamelCase();
});

// Configure JSON serialization for Swagger
builder.Services.ConfigureSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});
```

### Enable XML Documentation
Add to `SMSPrototype1.csproj`:
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

## Schema Components

### 1. Authentication Schemas

#### LoginRequest
```yaml
components:
  schemas:
    LoginRequest:
      type: object
      required:
        - email
        - password
      properties:
        email:
          type: string
          format: email
          maxLength: 100
          example: "student@sms.edu"
        password:
          type: string
          format: password
          minLength: 6
          maxLength: 100
          example: "Password123"
```

#### RegisterRequest
```yaml
RegisterRequest:
  type: object
  required:
    - email
    - password
    - firstName
    - lastName
    - role
  properties:
    email:
      type: string
      format: email
      maxLength: 100
    password:
      type: string
      format: password
      minLength: 6
    firstName:
      type: string
      maxLength: 50
    lastName:
      type: string
      maxLength: 50
    role:
      type: string
      enum: [Student, Teacher, Admin]
```

#### AuthResponse
```yaml
AuthResponse:
  type: object
  properties:
    token:
      type: string
      description: JWT Bearer token
    userId:
      type: string
      format: uuid
    email:
      type: string
      format: email
    role:
      type: string
      enum: [Student, Teacher, Admin]
    expiresAt:
      type: string
      format: date-time
```

### 2. Student Schemas

#### CreateStudentRequest
```yaml
CreateStudentRequest:
  type: object
  required:
    - srNumber
    - rollNumber
    - email
    - firstName
    - lastName
    - dob
    - gender
    - classId
  properties:
    srNumber:
      type: string
      description: Student Registration Number
      example: "SR2024001"
    rollNumber:
      type: integer
      minimum: 1
      example: 1
    email:
      type: string
      format: email
      maxLength: 100
    firstName:
      type: string
      maxLength: 50
    lastName:
      type: string
      maxLength: 50
    dob:
      type: string
      format: date
      description: Date of Birth (ISO 8601 date format)
    gender:
      $ref: '#/components/schemas/Gender'
    classId:
      type: string
      format: uuid
```

#### StudentResponse
```yaml
StudentResponse:
  allOf:
    - $ref: '#/components/schemas/CreateStudentRequest'
    - type: object
      properties:
        id:
          type: string
          format: uuid
        className:
          type: string
          nullable: true
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
          nullable: true
```

### 3. Teacher Schemas

#### CreateTeacherRequest
```yaml
CreateTeacherRequest:
  type: object
  required:
    - name
    - email
    - phone
    - address
    - gender
    - schoolId
  properties:
    name:
      type: string
      maxLength: 100
    email:
      type: string
      format: email
    phone:
      type: string
      pattern: '^\d{10,12}$'
      maxLength: 12
    address:
      type: string
      maxLength: 200
    gender:
      $ref: '#/components/schemas/Gender'
    schoolId:
      type: string
      format: uuid
```

### 4. Class Schemas

#### CreateClassRequest
```yaml
CreateClassRequest:
  type: object
  required:
    - className
    - section
    - schoolId
  properties:
    className:
      type: string
      maxLength: 50
      example: "Grade 10"
    section:
      type: string
      maxLength: 10
      example: "A"
    schoolId:
      type: string
      format: uuid
```

### 5. Attendance Schemas

#### CreateAttendanceRequest
```yaml
CreateAttendanceRequest:
  type: object
  required:
    - studentId
    - date
    - status
  properties:
    studentId:
      type: string
      format: uuid
    date:
      type: string
      format: date
    status:
      $ref: '#/components/schemas/AttendanceStatus'
    remarks:
      type: string
      maxLength: 500
      nullable: true
```

#### AttendanceResponse
```yaml
AttendanceResponse:
  allOf:
    - $ref: '#/components/schemas/CreateAttendanceRequest'
    - type: object
      properties:
        id:
          type: string
          format: uuid
        studentName:
          type: string
        className:
          type: string
        createdAt:
          type: string
          format: date-time
```

### 6. Chat Schemas

#### ChatRoom
```yaml
ChatRoom:
  type: object
  properties:
    id:
      type: string
      format: uuid
    name:
      type: string
      maxLength: 100
    description:
      type: string
      maxLength: 500
      nullable: true
    createdBy:
      type: string
      description: User ID of creator
    createdByUsername:
      type: string
      nullable: true
    createdAt:
      type: string
      format: date-time
    lastActivityAt:
      type: string
      format: date-time
      nullable: true
    isActive:
      type: boolean
    privacyLevel:
      $ref: '#/components/schemas/RoomPrivacyLevel'
    maxParticipants:
      type: integer
      minimum: 2
      maximum: 100
      default: 50
    allowRecording:
      type: boolean
    isEncrypted:
      type: boolean
```

#### CreateChatRoomRequest
```yaml
CreateChatRoomRequest:
  type: object
  required:
    - name
    - password
  properties:
    name:
      type: string
      maxLength: 100
    description:
      type: string
      maxLength: 500
      nullable: true
    password:
      type: string
      format: password
      minLength: 4
    privacyLevel:
      $ref: '#/components/schemas/RoomPrivacyLevel'
    maxParticipants:
      type: integer
      minimum: 2
      maximum: 100
      default: 50
```

#### ChatMessage
```yaml
ChatMessage:
  type: object
  properties:
    id:
      type: string
      format: uuid
    roomId:
      type: string
      format: uuid
    userId:
      type: string
      format: uuid
    content:
      type: string
      maxLength: 1000
    timestamp:
      type: string
      format: date-time
    isDeleted:
      type: boolean
    isEdited:
      type: boolean
```

### 7. Enum Schemas

#### Gender
```yaml
Gender:
  type: string
  enum:
    - Male
    - Female
    - Other
```

#### AttendanceStatus
```yaml
AttendanceStatus:
  type: string
  enum:
    - Present
    - Absent
    - Late
    - Leave
```

#### RoomPrivacyLevel
```yaml
RoomPrivacyLevel:
  type: integer
  enum: [0, 1, 2]
  x-enum-varnames: [Public, Private, InviteOnly]
  description: |
    0 = Public (Anyone can see and join)
    1 = Private (Need password to join)
    2 = InviteOnly (Need invitation to join)
```

### 8. Generic Response Wrapper

#### ApiResult
```yaml
ApiResult:
  type: object
  properties:
    content:
      type: object
      description: The actual response data
    isSuccess:
      type: boolean
    errorMessage:
      type: string
      nullable: true
    statusCode:
      type: integer
      format: int32
  required:
    - isSuccess
    - statusCode

# Generic typed version
ApiResultOfStudent:
  allOf:
    - $ref: '#/components/schemas/ApiResult'
    - type: object
      properties:
        content:
          $ref: '#/components/schemas/StudentResponse'
```

### 9. Pagination Schemas

#### PaginatedResponse
```yaml
PaginatedResponse:
  type: object
  properties:
    items:
      type: array
      items:
        type: object
    totalCount:
      type: integer
    pageNumber:
      type: integer
      minimum: 1
    pageSize:
      type: integer
      minimum: 1
      maximum: 100
    totalPages:
      type: integer
    hasNextPage:
      type: boolean
    hasPreviousPage:
      type: boolean
```

---

## OpenAPI Generator Configuration

### Install OpenAPI Generator CLI
```bash
# In Frontend directory
npm install @openapitools/openapi-generator-cli -D
```

### Configuration File: `openapitools.json`
Create in `Frontend/` directory:
```json
{
  "$schema": "node_modules/@openapitools/openapi-generator-cli/config.schema.json",
  "spaces": 2,
  "generator-cli": {
    "version": "7.2.0",
    "generators": {
      "typescript-axios": {
        "generatorName": "typescript-axios",
        "output": "./src/generated/api",
        "glob": "**/*",
        "inputSpec": "http://localhost:7266/swagger/v1/swagger.json",
        "additionalProperties": {
          "npmName": "@sms/api-client",
          "npmVersion": "1.0.0",
          "supportsES6": true,
          "withSeparateModelsAndApi": true,
          "modelPackage": "models",
          "apiPackage": "api",
          "useSingleRequestParameter": true,
          "enumNameSuffix": "",
          "enumPropertyNaming": "PascalCase"
        },
        "typeMappings": {
          "DateTime": "string",
          "DateOnly": "string"
        },
        "importMappings": {}
      }
    }
  }
}
```

### Package.json Scripts
Add to `Frontend/package.json`:
```json
{
  "scripts": {
    "generate-api": "openapi-generator-cli generate",
    "generate-api-local": "openapi-generator-cli generate -i http://localhost:7266/swagger/v1/swagger.json -g typescript-axios -o ./src/generated/api"
  }
}
```

### Custom Templates (Optional)
Create custom mustache templates in `Frontend/openapi-templates/`:

#### `apiInner.mustache` (Custom API client)
```typescript
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

export const BASE_PATH = "{{basePath}}".replace(/\/+$/, "");

export class Configuration {
    constructor(
        public basePath: string = BASE_PATH,
        public accessToken?: string | (() => string)
    ) {}
}

export const createApiClient = (config?: Configuration): AxiosInstance => {
    const instance = axios.create({
        baseURL: config?.basePath || BASE_PATH,
        withCredentials: true, // For JWT cookies
    });

    // Request interceptor for JWT token
    instance.interceptors.request.use(
        (config) => {
            const token = typeof config?.accessToken === 'function' 
                ? config.accessToken() 
                : config?.accessToken;
            
            if (token) {
                config.headers.Authorization = `Bearer ${token}`;
            }
            return config;
        },
        (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    instance.interceptors.response.use(
        (response) => response,
        (error) => {
            if (error.response?.status === 401) {
                // Handle unauthorized - redirect to login
                window.location.href = '/login';
            }
            return Promise.reject(error);
        }
    );

    return instance;
};
```

---

## Naming Conventions

### C# Backend (PascalCase)
- **Classes**: `StudentController`, `CreateStudentRequest`, `ApiResult`
- **Properties**: `FirstName`, `LastName`, `StudentId`
- **Methods**: `GetStudentById`, `CreateStudent`, `UpdateAttendance`
- **Enums**: `Gender`, `AttendanceStatus` (enum values: `Male`, `Female`)

### TypeScript Frontend (camelCase)
- **Variables/Functions**: `firstName`, `lastName`, `studentId`
- **Types/Interfaces**: `StudentResponse`, `CreateStudentRequest`
- **Enum values**: Generated as `Gender.Male`, `AttendanceStatus.Present`

### API Routes
- **Controllers**: `/api/students`, `/api/teachers`, `/api/chat-rooms`
- **Actions**: GET `/api/students/{id}`, POST `/api/students`, PUT `/api/students/{id}`

---

## Common Issues and Solutions

### 1. Date Handling

**Issue**: .NET `DateOnly` and `DateTime` not properly serialized.

**Solution**:
```csharp
// In Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

// DateOnlyJsonConverter.cs
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
```

**TypeScript Handling**:
```typescript
// Helper functions
export const parseDate = (dateString: string): Date => new Date(dateString);
export const formatDate = (date: Date): string => date.toISOString().split('T')[0];
```

### 2. Enum Handling

**Issue**: C# enums as integers vs TypeScript string enums.

**Solution**:
```csharp
// Use JsonStringEnumConverter
[JsonConverter(typeof(JsonStringEnumConverter))]
public Gender Gender { get; set; }
```

**Global Configuration**:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

### 3. Nullable Reference Types

**Issue**: C# nullable types not reflected in OpenAPI.

**Solution**:
```csharp
// Enable in .csproj
<Nullable>enable</Nullable>

// Use nullable annotations
public string? Description { get; set; } // Optional
public string Name { get; set; } = string.Empty; // Required
```

### 4. File Uploads

**Issue**: File upload schemas not properly documented.

**Solution**:
```csharp
/// <summary>
/// Upload student photo
/// </summary>
[HttpPost("{id}/photo")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
{
    // Implementation
}
```

**OpenAPI Schema**:
```yaml
requestBody:
  content:
    multipart/form-data:
      schema:
        type: object
        properties:
          file:
            type: string
            format: binary
```

### 5. Generic ApiResult Unwrapping

**Issue**: Generated client wraps everything in `ApiResult<T>`.

**TypeScript Helper**:
```typescript
export const unwrapApiResult = <T>(result: ApiResult<T>): T => {
    if (!result.isSuccess) {
        throw new Error(result.errorMessage || 'API request failed');
    }
    return result.content;
};

// Usage
const student = await unwrapApiResult(
    await studentsApi.getStudentById({ id: studentId })
);
```

### 6. SignalR Integration

**Note**: SignalR hubs are NOT part of OpenAPI specs.

**Separate Documentation**:
```typescript
// src/services/signalr/chatHub.ts
import * as signalR from '@microsoft/signalr';

export interface IChatHub {
    // Server methods (called from client)
    sendMessage(roomId: string, message: string): Promise<void>;
    joinRoom(roomId: string, password: string): Promise<void>;
    leaveRoom(roomId: string): Promise<void>;
}

export interface IChatHubCallbacks {
    // Client methods (called from server)
    receiveMessage(message: ChatMessage): void;
    userJoined(userId: string, username: string): void;
    userLeft(userId: string): void;
}

export const createChatHubConnection = (token: string): signalR.HubConnection => {
    return new signalR.HubConnectionBuilder()
        .withUrl("/chatHub", {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();
};
```

---

## Controller Examples with Proper Annotations

### StudentController.cs
```csharp
/// <summary>
/// Student management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class StudentController : ControllerBase
{
    /// <summary>
    /// Get all students
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of students</returns>
    /// <response code="200">Returns the list of students</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PaginatedResponse<StudentResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAllStudents(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        // Implementation
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student details</returns>
    /// <response code="200">Returns the student</response>
    /// <response code="404">Student not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResult<StudentResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        // Implementation
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="request">Student creation request</param>
    /// <returns>Created student</returns>
    /// <response code="201">Student created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<StudentResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        // Implementation
    }
}
```

---

## Usage Example in React Frontend

### API Client Setup
```typescript
// src/lib/api/client.ts
import { Configuration, StudentsApi, AuthApi } from '@/generated/api';
import { createApiClient } from './apiClient';

const getToken = (): string => {
    return localStorage.getItem('auth_token') || '';
};

const config = new Configuration({
    basePath: import.meta.env.VITE_API_BASE_URL || 'http://localhost:7266',
    accessToken: getToken
});

export const apiClient = createApiClient(config);
export const studentsApi = new StudentsApi(config, config.basePath, apiClient);
export const authApi = new AuthApi(config, config.basePath, apiClient);
```

### React Component Usage
```typescript
// src/components/Students/StudentList.tsx
import { useEffect, useState } from 'react';
import { studentsApi } from '@/lib/api/client';
import { StudentResponse } from '@/generated/api/models';

export const StudentList = () => {
    const [students, setStudents] = useState<StudentResponse[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStudents = async () => {
            try {
                const response = await studentsApi.getAllStudents({ 
                    pageNumber: 1, 
                    pageSize: 20 
                });
                
                if (response.data.isSuccess) {
                    setStudents(response.data.content.items);
                }
            } catch (error) {
                console.error('Failed to fetch students:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchStudents();
    }, []);

    // Render component
};
```

---

## Validation Integration

### Backend Validation
```csharp
// Using FluentValidation
public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RollNumber)
            .GreaterThan(0);
    }
}
```

### Frontend Validation (Zod)
```typescript
// src/schemas/student.schema.ts
import { z } from 'zod';
import { Gender } from '@/generated/api/models';

export const createStudentSchema = z.object({
    srNumber: z.string().min(1, 'SR Number is required'),
    rollNumber: z.number().int().positive(),
    email: z.string().email('Invalid email format'),
    firstName: z.string().min(1).max(50),
    lastName: z.string().min(1).max(50),
    dob: z.string().refine((date) => !isNaN(Date.parse(date)), 'Invalid date'),
    gender: z.nativeEnum(Gender),
    classId: z.string().uuid()
});

export type CreateStudentFormData = z.infer<typeof createStudentSchema>;
```

---

## Best Practices Summary

1. **Always use XML documentation comments** on controllers and actions
2. **Use `ProducesResponseType`** attributes for all endpoints
3. **Configure JSON serialization** globally in Program.cs
4. **Enable nullable reference types** in C# projects
5. **Use JsonStringEnumConverter** for enum serialization
6. **Create custom type converters** for DateOnly/TimeOnly
7. **Document SignalR separately** from REST API
8. **Use generic ApiResult wrapper** consistently
9. **Implement pagination** for list endpoints
10. **Add proper authentication** schemas to Swagger

---

## Regenerating API Client

```bash
# Make sure backend is running
cd Backend/SMSPrototype1
dotnet run

# In another terminal, generate frontend client
cd Frontend
npm run generate-api

# Review generated files
ls src/generated/api
```

Generated structure:
```
src/generated/api/
├── api/
│   ├── auth-api.ts
│   ├── students-api.ts
│   ├── teachers-api.ts
│   ├── classes-api.ts
│   ├── attendance-api.ts
│   └── chat-rooms-api.ts
├── models/
│   ├── student-response.ts
│   ├── create-student-request.ts
│   ├── gender.ts
│   └── ...
└── index.ts
```

---

## Conclusion

This guide provides a complete reference for implementing and maintaining OpenAPI schemas in the SMS project. By following these conventions and configurations, you ensure:

- **Type safety** across backend and frontend
- **Consistent naming** conventions
- **Automated client generation** with proper TypeScript types
- **Clear API documentation** via Swagger UI
- **Easy validation** on both tiers

For questions or issues, refer to:
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Generator](https://openapi-generator.tech/)
- [OpenAPI Specification](https://swagger.io/specification/)
