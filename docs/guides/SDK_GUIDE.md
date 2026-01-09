# SDK and Client Library Documentation

## Overview

This guide covers the use of client libraries and SDKs for integrating with the School Management System API. It includes C# client library documentation, TypeScript client generation, and usage examples.

## Table of Contents

- [C# SDK](#c-sdk)
- [TypeScript Client Generation](#typescript-client-generation)
- [Python Client](#python-client)
- [Best Practices](#best-practices)

---

## C# SDK

### Installation

**NuGet Package (When Published):**
```bash
dotnet add package SMS.API.Client
```

**Manual Installation:**
1. Clone the repository
2. Reference the `SMSAPIClient` project in your solution

### Quick Start

```csharp
using SMS.API.Client;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize client
        var client = new SMSApiClient("https://api.sms.edu");
        
        // Authenticate
        await client.AuthenticateAsync("username", "password");
        
        // Get all students
        var students = await client.Students.GetAllAsync();
        
        foreach (var student in students)
        {
            Console.WriteLine($"{student.FirstName} {student.LastName}");
        }
    }
}
```

### Complete C# Client Library

```csharp
// SMSApiClient.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SMS.API.Client
{
    public class SMSApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private string _authToken;
        
        public SMSApiClient(string baseUrl)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true
            };
            
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };
            
            // Initialize resource clients
            Auth = new AuthClient(_httpClient);
            Students = new StudentClient(_httpClient);
            Teachers = new TeacherClient(_httpClient);
            Schools = new SchoolClient(_httpClient);
            Classes = new ClassClient(_httpClient);
            Attendance = new AttendanceClient(_httpClient);
            Announcements = new AnnouncementClient(_httpClient);
            ChatRooms = new ChatRoomClient(_httpClient);
        }
        
        // Resource clients
        public AuthClient Auth { get; }
        public StudentClient Students { get; }
        public TeacherClient Teachers { get; }
        public SchoolClient Schools { get; }
        public ClassClient Classes { get; }
        public AttendanceClient Attendance { get; }
        public AnnouncementClient Announcements { get; }
        public ChatRoomClient ChatRooms { get; }
        
        public async Task AuthenticateAsync(string username, string password)
        {
            var response = await Auth.LoginAsync(username, password);
            _authToken = response.Token;
            
            // Set authorization header for subsequent requests
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
    
    // Auth Client
    public class AuthClient
    {
        private readonly HttpClient _httpClient;
        
        public AuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<ApiResult> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ApiResult>();
        }
        
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", new
            {
                UserName = username,
                Password = password
            });
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        
        public async Task<User> GetCurrentUserAsync()
        {
            var response = await _httpClient.GetAsync("/api/Auth/me");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }
        
        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("/api/Auth/logout", null);
        }
    }
    
    // Student Client
    public class StudentClient
    {
        private readonly HttpClient _httpClient;
        
        public StudentClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<List<Student>> GetAllAsync(Guid? schoolId = null)
        {
            var url = "/api/Student";
            if (schoolId.HasValue)
                url += $"?schoolId={schoolId.Value}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<ApiResult<List<Student>>>();
            return result.Data;
        }
        
        public async Task<Student> GetByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"/api/Student/{id}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<ApiResult<Student>>();
            return result.Data;
        }
        
        public async Task<Student> CreateAsync(CreateStudentRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Student", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<ApiResult<Student>>();
            return result.Data;
        }
        
        public async Task<Student> UpdateAsync(Guid id, UpdateStudentRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Student/{id}", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<ApiResult<Student>>();
            return result.Data;
        }
        
        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"/api/Student/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
    
    // Models
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class ApiResult<T> : ApiResult
    {
        public T Data { get; set; }
    }
    
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
    
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid? SchoolId { get; set; }
        public List<string> Roles { get; set; }
    }
    
    public class Student
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhoneNumber { get; set; }
        public Guid ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Guid SchoolId { get; set; }
    }
    
    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public Guid? SchoolId { get; set; }
    }
    
    public class CreateStudentRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhoneNumber { get; set; }
        public Guid ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Guid SchoolId { get; set; }
    }
    
    public class UpdateStudentRequest : CreateStudentRequest { }
}
```

### Usage Examples

#### Desktop Application

```csharp
// WPF Desktop App Example
using SMS.API.Client;
using System.Windows;

public partial class MainWindow : Window
{
    private SMSApiClient _apiClient;
    
    public MainWindow()
    {
        InitializeComponent();
        _apiClient = new SMSApiClient("https://api.sms.edu");
    }
    
    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _apiClient.AuthenticateAsync(
                UsernameTextBox.Text,
                PasswordBox.Password
            );
            
            MessageBox.Show("Login successful!");
            ShowDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}");
        }
    }
    
    private async void LoadStudents()
    {
        var students = await _apiClient.Students.GetAllAsync();
        StudentsDataGrid.ItemsSource = students;
    }
}
```

#### Xamarin Mobile App

```csharp
// Xamarin.Forms Mobile App
using SMS.API.Client;
using Xamarin.Forms;

public class StudentsPage : ContentPage
{
    private SMSApiClient _apiClient;
    private ListView _listView;
    
    public StudentsPage()
    {
        _apiClient = DependencyService.Get<SMSApiClient>();
        
        _listView = new ListView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var nameLabel = new Label();
                nameLabel.SetBinding(Label.TextProperty, "FullName");
                return new ViewCell { View = nameLabel };
            })
        };
        
        Content = new StackLayout
        {
            Children = { _listView }
        };
        
        LoadStudents();
    }
    
    private async void LoadStudents()
    {
        try
        {
            var students = await _apiClient.Students.GetAllAsync();
            _listView.ItemsSource = students;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
```

---

## TypeScript Client Generation

### Using OpenAPI Generator

**Install OpenAPI Generator:**
```bash
npm install @openapitools/openapi-generator-cli -g
```

**Generate TypeScript Client:**
```bash
# Get OpenAPI spec from Swagger
curl https://localhost:7266/swagger/v1/swagger.json -o swagger.json

# Generate TypeScript client
openapi-generator-cli generate \
  -i swagger.json \
  -g typescript-fetch \
  -o ./generated/api-client \
  --additional-properties=npmName=sms-api-client,supportsES6=true
```

**Install Generated Client:**
```bash
cd generated/api-client
npm install
npm run build
```

### Usage

```typescript
// Using generated TypeScript client
import { Configuration, AuthApi, StudentApi } from 'sms-api-client';

const config = new Configuration({
  basePath: 'https://localhost:7266',
  credentials: 'include' // For cookie-based auth
});

const authApi = new AuthApi(config);
const studentApi = new StudentApi(config);

// Login
await authApi.apiAuthLoginPost({
  loginDto: {
    userName: 'user',
    password: 'pass'
  }
});

// Get students
const students = await studentApi.apiStudentGet();
console.log(students);
```

### Custom TypeScript Client

```typescript
// custom-client.ts
export interface ApiConfig {
  baseUrl: string;
  onAuthError?: () => void;
}

export class SMSApiClient {
  private baseUrl: string;
  private onAuthError?: () => void;
  
  constructor(config: ApiConfig) {
    this.baseUrl = config.baseUrl;
    this.onAuthError = config.onAuthError;
  }
  
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        ...options.headers
      }
    });
    
    if (response.status === 401 && this.onAuthError) {
      this.onAuthError();
    }
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.errorMessage || 'Request failed');
    }
    
    return await response.json();
  }
  
  // Auth methods
  async login(username: string, password: string): Promise<void> {
    await this.request('/api/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ userName: username, password: password })
    });
  }
  
  async getCurrentUser(): Promise<User> {
    return this.request<User>('/api/Auth/me');
  }
  
  // Student methods
  async getStudents(schoolId?: string): Promise<Student[]> {
    const params = schoolId ? `?schoolId=${schoolId}` : '';
    const result = await this.request<ApiResult<Student[]>>(`/api/Student${params}`);
    return result.data;
  }
  
  async getStudent(id: string): Promise<Student> {
    const result = await this.request<ApiResult<Student>>(`/api/Student/${id}`);
    return result.data;
  }
  
  async createStudent(student: CreateStudentRequest): Promise<Student> {
    const result = await this.request<ApiResult<Student>>('/api/Student', {
      method: 'POST',
      body: JSON.stringify(student)
    });
    return result.data;
  }
}

// Usage in React
import { SMSApiClient } from './custom-client';

const api = new SMSApiClient({
  baseUrl: 'https://localhost:7266',
  onAuthError: () => {
    // Redirect to login
    window.location.href = '/login';
  }
});

// In component
const students = await api.getStudents();
```

---

## Python Client

### Installation

```bash
pip install sms-api-client  # When published
```

### Complete Python Client

```python
# sms_client.py
import requests
from typing import List, Dict, Optional
from dataclasses import dataclass
import json

@dataclass
class Student:
    id: str
    firstName: str
    lastName: str
    email: str
    schoolId: str

class SMSApiClient:
    def __init__(self, base_url: str):
        self.base_url = base_url
        self.session = requests.Session()
        self.session.verify = False  # Only for dev with self-signed certs
    
    def login(self, username: str, password: str) -> bool:
        """Authenticate and store session"""
        response = self.session.post(
            f"{self.base_url}/api/Auth/login",
            json={"userName": username, "password": password}
        )
        response.raise_for_status()
        return True
    
    def get_current_user(self) -> Dict:
        """Get current authenticated user"""
        response = self.session.get(f"{self.base_url}/api/Auth/me")
        response.raise_for_status()
        return response.json()
    
    def get_students(self, school_id: Optional[str] = None) -> List[Student]:
        """Get all students"""
        params = {"schoolId": school_id} if school_id else {}
        response = self.session.get(
            f"{self.base_url}/api/Student",
            params=params
        )
        response.raise_for_status()
        
        result = response.json()
        data = result.get("data", result)
        
        return [Student(**s) for s in data]
    
    def create_student(self, student_data: Dict) -> Student:
        """Create a new student"""
        response = self.session.post(
            f"{self.base_url}/api/Student",
            json=student_data
        )
        response.raise_for_status()
        
        result = response.json()
        return Student(**result["data"])
    
    def update_student(self, student_id: str, updates: Dict) -> Student:
        """Update student"""
        response = self.session.put(
            f"{self.base_url}/api/Student/{student_id}",
            json=updates
        )
        response.raise_for_status()
        
        result = response.json()
        return Student(**result["data"])
    
    def delete_student(self, student_id: str) -> bool:
        """Delete student"""
        response = self.session.delete(
            f"{self.base_url}/api/Student/{student_id}"
        )
        response.raise_for_status()
        return True

# Usage
client = SMSApiClient("https://localhost:7266")
client.login("admin", "Admin123")

students = client.get_students()
for student in students:
    print(f"{student.firstName} {student.lastName}")
```

---

## Best Practices

### 1. Error Handling

```csharp
// C# with proper error handling
try
{
    var students = await client.Students.GetAllAsync();
}
catch (HttpRequestException ex)
{
    // Network error
    logger.LogError(ex, "Network error while fetching students");
    ShowErrorMessage("Network connection failed");
}
catch (Exception ex)
{
    // Other errors
    logger.LogError(ex, "Unexpected error");
    ShowErrorMessage("An error occurred");
}
```

### 2. Connection Pooling

```csharp
// Reuse HttpClient instance
public class SMSApiClientFactory
{
    private static SMSApiClient _instance;
    private static readonly object _lock = new object();
    
    public static SMSApiClient GetClient(string baseUrl)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new SMSApiClient(baseUrl);
                }
            }
        }
        
        return _instance;
    }
}
```

### 3. Retry Logic

```typescript
async function retryRequest<T>(
  fn: () => Promise<T>,
  maxRetries: number = 3
): Promise<T> {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      
      const delay = Math.pow(2, i) * 1000;
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
}

// Usage
const students = await retryRequest(() => api.getStudents());
```

### 4. Caching

```python
from functools import lru_cache
from datetime import datetime, timedelta

class CachedSMSClient(SMSApiClient):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._cache = {}
        self._cache_duration = timedelta(minutes=5)
    
    def get_students_cached(self, school_id: Optional[str] = None):
        cache_key = f"students_{school_id}"
        
        if cache_key in self._cache:
            data, timestamp = self._cache[cache_key]
            if datetime.now() - timestamp < self._cache_duration:
                return data
        
        # Fetch fresh data
        students = self.get_students(school_id)
        self._cache[cache_key] = (students, datetime.now())
        
        return students
```

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Code Samples](./CODE_SAMPLES.md)
- [Authentication](./AUTHENTICATION.md)
