# Contract Testing Configuration

**Purpose:** Ensure API responses match OpenAPI schema and maintain contract consistency between backend and frontend.

---

## OpenAPI Schema Validation

### Postman Schema Validation

**Pre-request Script (Load Schema):**
```javascript
// Load OpenAPI schema from API endpoint
pm.sendRequest(pm.environment.get("baseUrl") + "/swagger/v1/swagger.json", (err, response) => {
    if (!err) {
        pm.environment.set("openApiSchema", response.json());
    }
});
```

**Test Script (Validate Response):**
```javascript
const schema = JSON.parse(pm.environment.get("openApiSchema"));
const endpoint = "/api/Student";
const method = "get";

// Extract schema for this endpoint
const endpointSchema = schema.paths[endpoint][method].responses["200"].content["application/json"].schema;

// Resolve $ref if present
function resolveRef(ref) {
    const parts = ref.split('/');
    let resolved = schema;
    parts.slice(1).forEach(part => {
        resolved = resolved[part];
    });
    return resolved;
}

if (endpointSchema.$ref) {
    const resolvedSchema = resolveRef(endpointSchema.$ref);
    pm.response.to.have.jsonSchema(resolvedSchema);
} else {
    pm.response.to.have.jsonSchema(endpointSchema);
}
```

---

## Schemathesis Configuration

**Installation:**
```bash
pip install schemathesis
```

**Run Contract Tests:**
```bash
# Test all endpoints against OpenAPI schema
schemathesis run https://localhost:7266/swagger/v1/swagger.json \
    --base-url https://localhost:7266 \
    --header "Authorization: Bearer <TOKEN>" \
    --checks all \
    --hypothesis-max-examples=50

# Generate detailed report
schemathesis run https://localhost:7266/swagger/v1/swagger.json \
    --base-url https://localhost:7266 \
    --header "Authorization: Bearer <TOKEN>" \
    --report report.html
```

**Configuration File (schemathesis.yaml):**
```yaml
base_url: "https://localhost:7266"
headers:
  Authorization: "Bearer {AUTH_TOKEN}"
checks:
  - not_a_server_error
  - status_code_conformance
  - content_type_conformance
  - response_schema_conformance
hypothesis:
  max_examples: 100
  phases:
    - explicit
    - generate
```

---

## TypeScript Type Validation

### Generate TypeScript Types from OpenAPI

**Using openapi-typescript:**
```bash
npm install -D openapi-typescript

# Generate types
npx openapi-typescript https://localhost:7266/swagger/v1/swagger.json \
    --output src/types/api.ts
```

**Generated Types (src/types/api.ts):**
```typescript
export interface paths {
  "/api/Student": {
    get: operations["GetAllStudents"];
    post: operations["CreateStudent"];
  };
  "/api/Student/{id}": {
    get: operations["GetStudentById"];
    put: operations["UpdateStudent"];
    delete: operations["DeleteStudent"];
  };
}

export interface components {
  schemas {
    Student: {
      id: string;
      firstName: string;
      lastName: string;
      email: string;
      dateOfBirth: string;
      gender: "Male" | "Female" | "Other";
      phoneNumber?: string;
      classId?: string;
      schoolId: string;
    };
    ApiResult<T>: {
      isSuccess: boolean;
      data: T;
      errorMessage?: string;
    };
  };
}
```

**Frontend Usage:**
```typescript
import type { components } from '@/types/api';

type Student = components['schemas']['Student'];
type ApiResult<T> = components['schemas']['ApiResult'];

async function getStudents(): Promise<ApiResult<Student[]>> {
  const response = await fetch('/api/Student');
  return response.json(); // TypeScript validates structure
}
```

---

## Enum Consistency Validation

**Backend Enums (C#):**
```csharp
public enum Gender
{
    Male,
    Female,
    Other
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    Excused
}

public enum RoomPrivacyLevel
{
    Public,
    Private,
    Restricted
}
```

**Frontend Enums (TypeScript):**
```typescript
// Sync with backend enums
export enum Gender {
  Male = "Male",
  Female = "Female",
  Other = "Other"
}

export enum AttendanceStatus {
  Present = "Present",
  Absent = "Absent",
  Late = "Late",
  Excused = "Excused"
}

export enum RoomPrivacyLevel {
  Public = "Public",
  Private = "Private",
  Restricted = "Restricted"
}
```

**Validation Script (validate-enums.js):**
```javascript
const fs = require('fs');
const axios = require('axios');

async function validateEnums() {
  // Fetch OpenAPI schema
  const response = await axios.get('https://localhost:7266/swagger/v1/swagger.json');
  const schema = response.data;
  
  // Extract enums from schema
  const backendEnums = {
    Gender: schema.components.schemas.Gender.enum,
    AttendanceStatus: schema.components.schemas.AttendanceStatus.enum,
    RoomPrivacyLevel: schema.components.schemas.RoomPrivacyLevel.enum
  };
  
  // Load frontend enums
  const frontendEnums = require('./src/types/enums.ts');
  
  // Validate consistency
  let errors = [];
  
  Object.keys(backendEnums).forEach(enumName => {
    const backendValues = backendEnums[enumName];
    const frontendValues = Object.values(frontendEnums[enumName]);
    
    if (JSON.stringify(backendValues.sort()) !== JSON.stringify(frontendValues.sort())) {
      errors.push(`Enum mismatch: ${enumName}`);
      console.error(`Backend: ${backendValues}`);
      console.error(`Frontend: ${frontendValues}`);
    }
  });
  
  if (errors.length > 0) {
    console.error('Enum validation failed!');
    process.exit(1);
  } else {
    console.log('✓ All enums are consistent');
  }
}

validateEnums();
```

---

## DTO Validation

### Validate Request/Response DTOs

**Postman Test (Student Creation):**
```javascript
pm.test("Response matches Student DTO", function () {
    const jsonData = pm.response.json();
    const student = jsonData.data;
    
    // Required fields
    pm.expect(student).to.have.property('id');
    pm.expect(student).to.have.property('firstName');
    pm.expect(student).to.have.property('lastName');
    pm.expect(student).to.have.property('email');
    pm.expect(student).to.have.property('dateOfBirth');
    pm.expect(student).to.have.property('gender');
    pm.expect(student).to.have.property('schoolId');
    
    // Data types
    pm.expect(student.id).to.be.a('string');
    pm.expect(student.firstName).to.be.a('string');
    pm.expect(student.email).to.match(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/);
    
    // Enum validation
    pm.expect(student.gender).to.be.oneOf(['Male', 'Female', 'Other']);
});
```

**JSON Schema for Student DTO:**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Student",
  "type": "object",
  "required": ["id", "firstName", "lastName", "email", "schoolId"],
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid"
    },
    "firstName": {
      "type": "string",
      "minLength": 1,
      "maxLength": 50
    },
    "lastName": {
      "type": "string",
      "minLength": 1,
      "maxLength": 50
    },
    "email": {
      "type": "string",
      "format": "email"
    },
    "dateOfBirth": {
      "type": "string",
      "format": "date"
    },
    "gender": {
      "type": "string",
      "enum": ["Male", "Female", "Other"]
    },
    "phoneNumber": {
      "type": ["string", "null"],
      "pattern": "^[0-9\\-\\+\\s\\(\\)]+$"
    },
    "classId": {
      "type": ["string", "null"],
      "format": "uuid"
    },
    "schoolId": {
      "type": "string",
      "format": "uuid"
    }
  }
}
```

---

## Automated Contract Testing Pipeline

### GitHub Actions Workflow

**File:** `.github/workflows/contract-tests.yml`

```yaml
name: Contract Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  schema-validation:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Start API Server
      run: |
        dotnet run --project Backend/SMSPrototype1/SMSPrototype1.csproj &
        sleep 30
    
    - name: Install Schemathesis
      run: pip install schemathesis
    
    - name: Run Contract Tests
      run: |
        schemathesis run https://localhost:7266/swagger/v1/swagger.json \
          --base-url https://localhost:7266 \
          --checks all \
          --report contract-report.html
    
    - name: Upload Contract Report
      uses: actions/upload-artifact@v3
      with:
        name: contract-report
        path: ./contract-report.html
  
  typescript-types:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '20'
    
    - name: Generate TypeScript Types
      run: |
        cd Frontend
        npm install
        npx openapi-typescript https://localhost:7266/swagger/v1/swagger.json \
          --output src/types/api-generated.ts
    
    - name: Compare Types
      run: |
        # Check if generated types differ from committed types
        diff Frontend/src/types/api.ts Frontend/src/types/api-generated.ts || exit 1
  
  enum-validation:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Validate Enums
      run: node scripts/validate-enums.js
```

---

## Contract Test Report Template

**HTML Report Structure:**
```html
<!DOCTYPE html>
<html>
<head>
    <title>Contract Test Report</title>
    <style>
        .pass { color: green; }
        .fail { color: red; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
    </style>
</head>
<body>
    <h1>API Contract Test Report</h1>
    <p>Date: <span id="date"></span></p>
    
    <h2>Summary</h2>
    <table>
        <tr>
            <th>Total Tests</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Pass Rate</th>
        </tr>
        <tr>
            <td id="total">0</td>
            <td id="passed">0</td>
            <td id="failed">0</td>
            <td id="passRate">0%</td>
        </tr>
    </table>
    
    <h2>Test Results</h2>
    <table id="results">
        <tr>
            <th>Endpoint</th>
            <th>Method</th>
            <th>Status</th>
            <th>Schema Match</th>
            <th>Details</th>
        </tr>
    </table>
</body>
</html>
```

---

## Best Practices

1. **Run contract tests on every commit** to catch schema changes early
2. **Version your OpenAPI schema** (commit swagger.json to repo)
3. **Auto-generate TypeScript types** as part of build process
4. **Validate enums separately** as they're common sources of mismatches
5. **Test both success and error responses** for schema compliance
6. **Use Postman collection runners** for continuous validation
7. **Monitor schema drift** - alert when frontend/backend diverge

---

**Related Documentation:**
- [Test Strategy](./TEST_STRATEGY.md)
- [Postman Guide](./POSTMAN_GUIDE.md)
- [OpenAPI Schema Guide](../openapi/OPENAPI_SCHEMA_GUIDE.md)
