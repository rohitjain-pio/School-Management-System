# Code Samples Library

## Overview

This guide provides comprehensive code examples for integrating with the School Management System API across multiple programming languages and platforms. All examples include error handling, authentication, and best practices.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Authentication Examples](#authentication-examples)
- [Student Management](#student-management)
- [Teacher Management](#teacher-management)
- [Attendance Tracking](#attendance-tracking)
- [Chat Room Integration](#chat-room-integration)
- [Video Call Integration](#video-call-integration)
- [Complete Application Examples](#complete-application-examples)

---

## Prerequisites

### API Credentials
```
Base URL: https://localhost:7266
Username: Your registered username
Password: Your password
```

### Required Libraries

**JavaScript/TypeScript:**
```bash
npm install @microsoft/signalr axios
```

**Python:**
```bash
pip install requests python-signalr-client
```

**C#:**
```bash
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package System.Net.Http.Json
```

**cURL:**
- Pre-installed on most Unix systems
- Windows: Use Git Bash or install from curl.se

---

## Authentication Examples

### JavaScript/TypeScript

```typescript
// auth.ts
interface LoginCredentials {
  userName: string;
  password: string;
}

interface RegisterData extends LoginCredentials {
  email: string;
  role: string;
  schoolId?: string;
}

interface User {
  id: string;
  username: string;
  email: string;
  schoolId?: string;
  roles: string[];
}

class AuthService {
  private baseUrl = 'https://localhost:7266/api';

  async register(data: RegisterData): Promise<void> {
    const response = await fetch(`${this.baseUrl}/Auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.errorMessage || 'Registration failed');
    }

    const result = await response.json();
    if (!result.isSuccess) {
      throw new Error(result.errorMessage);
    }
  }

  async login(credentials: LoginCredentials): Promise<void> {
    const response = await fetch(`${this.baseUrl}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include', // Important for cookies
      body: JSON.stringify(credentials)
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    // Cookie is automatically set by server
  }

  async getCurrentUser(): Promise<User> {
    const response = await fetch(`${this.baseUrl}/Auth/me`, {
      credentials: 'include'
    });

    if (!response.ok) {
      throw new Error('Not authenticated');
    }

    return await response.json();
  }

  async logout(): Promise<void> {
    await fetch(`${this.baseUrl}/Auth/logout`, {
      method: 'POST',
      credentials: 'include'
    });
  }
}

// Usage
const auth = new AuthService();

// Register
await auth.register({
  userName: 'student1',
  email: 'student1@school.edu',
  password: 'Student123',
  role: 'Student',
  schoolId: 'school-uuid'
});

// Login
await auth.login({
  userName: 'student1',
  password: 'Student123'
});

// Get current user
const user = await auth.getCurrentUser();
console.log('Logged in as:', user.username);
```

### Python

```python
# auth.py
import requests
from typing import Optional, Dict

class AuthService:
    def __init__(self, base_url: str = "https://localhost:7266/api"):
        self.base_url = base_url
        self.session = requests.Session()
        # Disable SSL verification for localhost (dev only!)
        self.session.verify = False
    
    def register(
        self,
        username: str,
        email: str,
        password: str,
        role: str,
        school_id: Optional[str] = None
    ) -> bool:
        """Register a new user"""
        data = {
            "userName": username,
            "email": email,
            "password": password,
            "role": role,
            "schoolId": school_id
        }
        
        response = self.session.post(
            f"{self.base_url}/Auth/register",
            json=data
        )
        response.raise_for_status()
        
        result = response.json()
        if not result.get("isSuccess", False):
            raise Exception(result.get("errorMessage", "Registration failed"))
        
        return True
    
    def login(self, username: str, password: str) -> bool:
        """Login and store session cookie"""
        data = {
            "userName": username,
            "password": password
        }
        
        response = self.session.post(
            f"{self.base_url}/Auth/login",
            json=data
        )
        response.raise_for_status()
        
        # Cookie is automatically stored in session
        return True
    
    def get_current_user(self) -> Dict:
        """Get current authenticated user"""
        response = self.session.get(f"{self.base_url}/Auth/me")
        response.raise_for_status()
        return response.json()
    
    def logout(self) -> bool:
        """Logout and clear session"""
        response = self.session.post(f"{self.base_url}/Auth/logout")
        response.raise_for_status()
        return True

# Usage
if __name__ == "__main__":
    auth = AuthService()
    
    # Register
    auth.register(
        username="teacher1",
        email="teacher1@school.edu",
        password="Teacher123",
        role="Teacher",
        school_id="school-uuid"
    )
    
    # Login
    auth.login("teacher1", "Teacher123")
    
    # Get user
    user = auth.get_current_user()
    print(f"Logged in as: {user['username']}")
```

### C#

```csharp
// AuthService.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7266/api";

    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer()
        };
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    public async Task<bool> RegisterAsync(
        string username,
        string email,
        string password,
        string role,
        Guid? schoolId = null)
    {
        var data = new
        {
            UserName = username,
            Email = email,
            Password = password,
            Role = role,
            SchoolId = schoolId
        };

        var response = await _httpClient.PostAsJsonAsync("/Auth/register", data);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResult>();
        if (!result.IsSuccess)
        {
            throw new Exception(result.ErrorMessage);
        }

        return true;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var data = new
        {
            UserName = username,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/Auth/login", data);
        response.EnsureSuccessStatusCode();

        // Cookie is automatically stored
        return true;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("/Auth/me");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<bool> LogoutAsync()
    {
        var response = await _httpClient.PostAsync("/Auth/logout", null);
        response.EnsureSuccessStatusCode();
        return true;
    }
}

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public Guid? SchoolId { get; set; }
    public string[] Roles { get; set; }
}

// Usage
var auth = new AuthService();

// Register
await auth.RegisterAsync(
    "admin1",
    "admin1@school.edu",
    "Admin123",
    "Admin"
);

// Login
await auth.LoginAsync("admin1", "Admin123");

// Get user
var user = await auth.GetCurrentUserAsync();
Console.WriteLine($"Logged in as: {user.Username}");
```

### cURL

```bash
#!/bin/bash
# auth.sh

API_URL="https://localhost:7266/api"
COOKIE_FILE="cookies.txt"

# Register
register_user() {
    curl -X POST "$API_URL/Auth/register" \
      -H "Content-Type: application/json" \
      -d '{
        "userName": "'"$1"'",
        "email": "'"$2"'",
        "password": "'"$3"'",
        "role": "'"$4"'",
        "schoolId": '"${5:-null}"'
      }' \
      -k # -k to skip SSL verification for localhost
}

# Login
login() {
    curl -X POST "$API_URL/Auth/login" \
      -H "Content-Type: application/json" \
      -c "$COOKIE_FILE" \
      -d '{
        "userName": "'"$1"'",
        "password": "'"$2"'"
      }' \
      -k
}

# Get current user
get_current_user() {
    curl -X GET "$API_URL/Auth/me" \
      -b "$COOKIE_FILE" \
      -k
}

# Usage
register_user "student1" "student1@school.edu" "Student123" "Student" "null"
login "student1" "Student123"
get_current_user
```

---

## Student Management

### JavaScript/TypeScript

```typescript
// students.ts
interface Student {
  id?: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  phoneNumber: string;
  email: string;
  guardianName: string;
  guardianPhoneNumber: string;
  classId: string;
  enrollmentDate: string;
  schoolId: string;
}

class StudentService {
  private baseUrl = 'https://localhost:7266/api';

  async getAllStudents(): Promise<Student[]> {
    const response = await fetch(`${this.baseUrl}/Student`, {
      credentials: 'include'
    });

    if (!response.ok) {
      throw new Error('Failed to fetch students');
    }

    const result = await response.json();
    return result.data || result;
  }

  async getStudent(id: string): Promise<Student> {
    const response = await fetch(`${this.baseUrl}/Student/${id}`, {
      credentials: 'include'
    });

    if (!response.ok) {
      throw new Error('Student not found');
    }

    const result = await response.json();
    return result.data || result;
  }

  async createStudent(student: Student): Promise<Student> {
    const response = await fetch(`${this.baseUrl}/Student`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(student)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.errorMessage || 'Failed to create student');
    }

    const result = await response.json();
    return result.data;
  }

  async updateStudent(id: string, student: Partial<Student>): Promise<Student> {
    const response = await fetch(`${this.baseUrl}/Student/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(student)
    });

    if (!response.ok) {
      throw new Error('Failed to update student');
    }

    const result = await response.json();
    return result.data;
  }

  async deleteStudent(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/Student/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    });

    if (!response.ok) {
      throw new Error('Failed to delete student');
    }
  }
}

// Usage
const studentService = new StudentService();

// Create student
const newStudent = await studentService.createStudent({
  firstName: 'John',
  lastName: 'Doe',
  dateOfBirth: '2008-05-15',
  gender: 'Male',
  address: '123 Main St',
  phoneNumber: '+1-555-0101',
  email: 'john.doe@student.school.edu',
  guardianName: 'Jane Doe',
  guardianPhoneNumber: '+1-555-0102',
  classId: 'class-uuid',
  enrollmentDate: '2024-01-09',
  schoolId: 'school-uuid'
});

// Get all students
const students = await studentService.getAllStudents();
console.log(`Total students: ${students.length}`);

// Get specific student
const student = await studentService.getStudent(newStudent.id!);

// Update student
await studentService.updateStudent(student.id!, {
  phoneNumber: '+1-555-9999'
});

// Delete student
await studentService.deleteStudent(student.id!);
```

### Python

```python
# students.py
from typing import List, Dict, Optional
from dataclasses import dataclass, asdict
from datetime import date

@dataclass
class Student:
    firstName: str
    lastName: str
    dateOfBirth: str
    gender: str
    address: str
    phoneNumber: str
    email: str
    guardianName: str
    guardianPhoneNumber: str
    classId: str
    enrollmentDate: str
    schoolId: str
    id: Optional[str] = None

class StudentService:
    def __init__(self, session: requests.Session):
        self.session = session
        self.base_url = "https://localhost:7266/api"
    
    def get_all(self) -> List[Dict]:
        """Get all students"""
        response = self.session.get(f"{self.base_url}/Student")
        response.raise_for_status()
        
        result = response.json()
        return result.get("data", result)
    
    def get_by_id(self, student_id: str) -> Dict:
        """Get student by ID"""
        response = self.session.get(f"{self.base_url}/Student/{student_id}")
        response.raise_for_status()
        
        result = response.json()
        return result.get("data", result)
    
    def create(self, student: Student) -> Dict:
        """Create new student"""
        response = self.session.post(
            f"{self.base_url}/Student",
            json=asdict(student)
        )
        response.raise_for_status()
        
        result = response.json()
        return result.get("data")
    
    def update(self, student_id: str, updates: Dict) -> Dict:
        """Update student"""
        response = self.session.put(
            f"{self.base_url}/Student/{student_id}",
            json=updates
        )
        response.raise_for_status()
        
        result = response.json()
        return result.get("data")
    
    def delete(self, student_id: str) -> bool:
        """Delete student"""
        response = self.session.delete(f"{self.base_url}/Student/{student_id}")
        response.raise_for_status()
        return True

# Usage
student_service = StudentService(auth.session)

# Create student
new_student = Student(
    firstName="Alice",
    lastName="Johnson",
    dateOfBirth="2009-03-20",
    gender="Female",
    address="456 Oak Ave",
    phoneNumber="+1-555-0201",
    email="alice.j@student.school.edu",
    guardianName="Bob Johnson",
    guardianPhoneNumber="+1-555-0202",
    classId="class-uuid",
    enrollmentDate="2024-01-09",
    schoolId="school-uuid"
)

created = student_service.create(new_student)
print(f"Created student with ID: {created['id']}")

# Get all students
students = student_service.get_all()
print(f"Total students: {len(students)}")

# Update student
student_service.update(created['id'], {
    "phoneNumber": "+1-555-8888"
})

# Delete student
student_service.delete(created['id'])
```

---

## Chat Room Integration

### JavaScript/TypeScript with SignalR

```typescript
// chat.ts
import * as signalR from "@microsoft/signalr";

interface ChatMessage {
  id: string;
  sender: string;
  content: string;
  timestamp: string;
  isEncrypted: boolean;
  isEdited?: boolean;
}

interface RoomDetails {
  id: string;
  name: string;
  description?: string;
  isUserModerator: boolean;
}

class ChatService {
  private connection: signalR.HubConnection;
  private roomId: string;
  private roomAccessToken: string;

  constructor(private baseUrl: string = 'https://localhost:7266') {}

  async createRoom(
    name: string,
    password: string,
    description?: string,
    maxParticipants: number = 50
  ): Promise<RoomDetails> {
    const response = await fetch(`${this.baseUrl}/api/ChatRooms`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({
        name,
        description,
        password,
        privacyLevel: 'Private',
        maxParticipants,
        allowRecording: true
      })
    });

    if (!response.ok) {
      throw new Error('Failed to create room');
    }

    return await response.json();
  }

  async joinRoom(roomId: string, password: string): Promise<string> {
    const response = await fetch(`${this.baseUrl}/api/ChatRooms/join`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ roomId, password })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to join room');
    }

    const result = await response.json();
    this.roomAccessToken = result.roomAccessToken;
    return result.roomAccessToken;
  }

  async connectToChat(authToken: string): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/chatHub`, {
        accessTokenFactory: () => authToken
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.setupEventHandlers();

    // Start connection
    await this.connection.start();
    console.log('Connected to chat hub');
  }

  private setupEventHandlers(): void {
    this.connection.on('ReceiveMessage', (message: ChatMessage) => {
      this.onMessageReceived(message);
    });

    this.connection.on('UserJoined', (username: string) => {
      console.log(`${username} joined the chat`);
      this.onUserJoined(username);
    });

    this.connection.on('UserLeft', (username: string) => {
      console.log(`${username} left the chat`);
      this.onUserLeft(username);
    });

    this.connection.on('UserListUpdated', (users: string[]) => {
      this.onUserListUpdated(users);
    });

    this.connection.on('ReceiveTyping', (username: string) => {
      this.onUserTyping(username);
    });

    this.connection.onreconnecting((error) => {
      console.log('Reconnecting...', error);
      this.onReconnecting(error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('Reconnected!', connectionId);
      this.onReconnected(connectionId);
      // Re-join room after reconnection
      this.joinChatRoom(this.roomId, this.roomAccessToken);
    });

    this.connection.onclose((error) => {
      console.log('Connection closed', error);
      this.onConnectionClosed(error);
    });
  }

  async joinChatRoom(roomId: string, roomAccessToken: string): Promise<void> {
    this.roomId = roomId;
    this.roomAccessToken = roomAccessToken;
    await this.connection.invoke('JoinRoom', roomId, roomAccessToken);
  }

  async sendMessage(message: string): Promise<void> {
    if (!message.trim()) {
      throw new Error('Message cannot be empty');
    }

    if (message.length > 1000) {
      throw new Error('Message too long (max 1000 characters)');
    }

    await this.connection.invoke('SendMessage', this.roomId, message);
  }

  async sendTypingIndicator(username: string): Promise<void> {
    await this.connection.invoke('SendTyping', this.roomId, username);
  }

  async loadMessageHistory(count: number = 50): Promise<ChatMessage[]> {
    return await this.connection.invoke('LoadMessageHistory', this.roomId, count);
  }

  async leaveRoom(): Promise<void> {
    await this.connection.invoke('LeaveRoom', this.roomId);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
    }
  }

  // Event handler callbacks (override in subclass or set as properties)
  protected onMessageReceived(message: ChatMessage): void {
    console.log('Message received:', message);
  }

  protected onUserJoined(username: string): void {}
  protected onUserLeft(username: string): void {}
  protected onUserListUpdated(users: string[]): void {}
  protected onUserTyping(username: string): void {}
  protected onReconnecting(error?: Error): void {}
  protected onReconnected(connectionId?: string): void {}
  protected onConnectionClosed(error?: Error): void {}
}

// Usage Example
async function chatExample() {
  const chat = new ChatService();

  // Override event handlers
  chat['onMessageReceived'] = (message) => {
    const messageEl = document.createElement('div');
    messageEl.textContent = `${message.sender}: ${message.content}`;
    document.getElementById('messages')?.appendChild(messageEl);
  };

  // Create room
  const room = await chat.createRoom(
    'Class Discussion',
    'SecurePass123',
    'Discussion room for Grade 10A'
  );

  // Join room
  const accessToken = await chat.joinRoom(room.id, 'SecurePass123');

  // Connect to SignalR
  await chat.connectToChat('your-jwt-token');

  // Join chat room
  await chat.joinChatRoom(room.id, accessToken);

  // Load message history
  const history = await chat.loadMessageHistory(50);
  console.log('Message history:', history);

  // Send message
  await chat.sendMessage('Hello everyone!');

  // Send typing indicator
  await chat.sendTypingIndicator('CurrentUser');

  // Leave room when done
  await chat.leaveRoom();
  await chat.disconnect();
}
```

---

## Video Call Integration

### JavaScript/TypeScript with WebRTC

```typescript
// video-call.ts
import * as signalR from "@microsoft/signalr";

interface Participant {
  connectionId: string;
  username: string;
  userId: string;
  audioEnabled: boolean;
  videoEnabled: boolean;
}

class VideoCallService {
  private connection: signalR.HubConnection;
  private peerConnections: Map<string, RTCPeerConnection> = new Map();
  private localStream: MediaStream | null = null;
  private roomId: string;
  private roomAccessToken: string;

  constructor(private baseUrl: string = 'https://localhost:7266') {}

  async joinVideoRoom(
    roomId: string,
    roomAccessToken: string,
    authToken: string
  ): Promise<void> {
    this.roomId = roomId;
    this.roomAccessToken = roomAccessToken;

    // Get local media stream
    this.localStream = await navigator.mediaDevices.getUserMedia({
      video: true,
      audio: true
    });

    // Connect to SignalR hub
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/videoCallHub`, {
        accessTokenFactory: () => authToken
      })
      .withAutomaticReconnect()
      .build();

    this.setupSignalRHandlers();
    await this.connection.start();

    // Join video room
    await this.connection.invoke('JoinVideoRoom', roomId, roomAccessToken);
  }

  private setupSignalRHandlers(): void {
    // Existing participants in room
    this.connection.on('ExistingParticipants', async (participants: Participant[]) => {
      for (const participant of participants) {
        await this.createPeerConnection(participant.connectionId, true);
      }
    });

    // New user joined
    this.connection.on('UserJoinedCall', async (user: Participant) => {
      await this.createPeerConnection(user.connectionId, false);
    });

    // User left
    this.connection.on('UserLeftCall', (connectionId: string) => {
      this.closePeerConnection(connectionId);
    });

    // WebRTC signaling
    this.connection.on('ReceiveOffer', async (connectionId: string, offer: any) => {
      await this.handleOffer(connectionId, offer);
    });

    this.connection.on('ReceiveAnswer', async (connectionId: string, answer: any) => {
      await this.handleAnswer(connectionId, answer);
    });

    this.connection.on('ReceiveIceCandidate', async (connectionId: string, candidate: any) => {
      await this.handleIceCandidate(connectionId, candidate);
    });

    // Media state changes
    this.connection.on('ParticipantMediaStateChanged', 
      (connectionId: string, audioEnabled: boolean, videoEnabled: boolean) => {
        this.onParticipantMediaStateChanged(connectionId, audioEnabled, videoEnabled);
      }
    );

    // Kicked from room
    this.connection.on('KickedFromRoom', (message: string) => {
      alert(message);
      this.leaveRoom();
    });
  }

  private async createPeerConnection(
    connectionId: string,
    createOffer: boolean
  ): Promise<void> {
    const configuration: RTCConfiguration = {
      iceServers: [
        { urls: 'stun:stun.l.google.com:19302' },
        { urls: 'stun:stun1.l.google.com:19302' }
      ]
    };

    const peerConnection = new RTCPeerConnection(configuration);
    this.peerConnections.set(connectionId, peerConnection);

    // Add local stream tracks
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        peerConnection.addTrack(track, this.localStream!);
      });
    }

    // Handle incoming tracks
    peerConnection.ontrack = (event) => {
      this.onRemoteStream(connectionId, event.streams[0]);
    };

    // Handle ICE candidates
    peerConnection.onicecandidate = (event) => {
      if (event.candidate) {
        this.connection.invoke('SendIceCandidate', connectionId, event.candidate);
      }
    };

    // Create offer if initiator
    if (createOffer) {
      const offer = await peerConnection.createOffer();
      await peerConnection.setLocalDescription(offer);
      await this.connection.invoke('SendOffer', connectionId, offer);
    }
  }

  private async handleOffer(connectionId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    let peerConnection = this.peerConnections.get(connectionId);
    
    if (!peerConnection) {
      await this.createPeerConnection(connectionId, false);
      peerConnection = this.peerConnections.get(connectionId)!;
    }

    await peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);
    await this.connection.invoke('SendAnswer', connectionId, answer);
  }

  private async handleAnswer(connectionId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    const peerConnection = this.peerConnections.get(connectionId);
    if (peerConnection) {
      await peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    }
  }

  private async handleIceCandidate(connectionId: string, candidate: RTCIceCandidateInit): Promise<void> {
    const peerConnection = this.peerConnections.get(connectionId);
    if (peerConnection) {
      await peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
    }
  }

  private closePeerConnection(connectionId: string): void {
    const peerConnection = this.peerConnections.get(connectionId);
    if (peerConnection) {
      peerConnection.close();
      this.peerConnections.delete(connectionId);
      this.onParticipantLeft(connectionId);
    }
  }

  async toggleAudio(enabled: boolean): Promise<void> {
    if (this.localStream) {
      this.localStream.getAudioTracks().forEach(track => {
        track.enabled = enabled;
      });

      const videoEnabled = this.localStream.getVideoTracks()[0]?.enabled ?? true;
      await this.connection.invoke('UpdateMediaState', this.roomId, enabled, videoEnabled);
    }
  }

  async toggleVideo(enabled: boolean): Promise<void> {
    if (this.localStream) {
      this.localStream.getVideoTracks().forEach(track => {
        track.enabled = enabled;
      });

      const audioEnabled = this.localStream.getAudioTracks()[0]?.enabled ?? true;
      await this.connection.invoke('UpdateMediaState', this.roomId, audioEnabled, enabled);
    }
  }

  async kickParticipant(connectionId: string): Promise<void> {
    await this.connection.invoke('KickParticipant', this.roomId, connectionId);
  }

  async leaveRoom(): Promise<void> {
    // Stop local stream
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = null;
    }

    // Close all peer connections
    this.peerConnections.forEach(pc => pc.close());
    this.peerConnections.clear();

    // Leave SignalR room
    await this.connection.invoke('LeaveVideoRoom', this.roomId);
    await this.connection.stop();
  }

  // Event handlers (override these)
  protected onRemoteStream(connectionId: string, stream: MediaStream): void {
    console.log('Remote stream received:', connectionId);
  }

  protected onParticipantLeft(connectionId: string): void {
    console.log('Participant left:', connectionId);
  }

  protected onParticipantMediaStateChanged(
    connectionId: string,
    audioEnabled: boolean,
    videoEnabled: boolean
  ): void {
    console.log('Media state changed:', connectionId, { audioEnabled, videoEnabled });
  }

  getLocalStream(): MediaStream | null {
    return this.localStream;
  }
}

// Usage Example
async function videoCallExample() {
  const videoCall = new VideoCallService();

  // Override event handlers
  videoCall['onRemoteStream'] = (connectionId, stream) => {
    const video = document.createElement('video');
    video.srcObject = stream;
    video.autoplay = true;
    video.id = `video-${connectionId}`;
    document.getElementById('remote-videos')?.appendChild(video);
  };

  videoCall['onParticipantLeft'] = (connectionId) => {
    const video = document.getElementById(`video-${connectionId}`);
    video?.remove();
  };

  // Join video room
  await videoCall.joinVideoRoom(roomId, roomAccessToken, authToken);

  // Display local video
  const localVideo = document.getElementById('local-video') as HTMLVideoElement;
  localVideo.srcObject = videoCall.getLocalStream();

  // Toggle audio/video
  document.getElementById('mute-btn')?.addEventListener('click', async () => {
    const enabled = !localVideo.srcObject.getAudioTracks()[0].enabled;
    await videoCall.toggleAudio(enabled);
  });

  document.getElementById('video-btn')?.addEventListener('click', async () => {
    const enabled = !localVideo.srcObject.getVideoTracks()[0].enabled;
    await videoCall.toggleVideo(enabled);
  });

  // Leave call
  document.getElementById('leave-btn')?.addEventListener('click', async () => {
    await videoCall.leaveRoom();
  });
}
```

---

## Complete Application Examples

### React TypeScript Chat Application

See the Frontend implementation at `Frontend/src/pages/ChatRoom.tsx` for a complete working example with:
- User authentication
- Room creation and joining
- Real-time messaging
- Typing indicators
- Message history
- Error handling

### Python CLI Chat Bot

```python
# bot.py - Simple chatbot that responds to messages
import asyncio
from signalrcore.hub_connection_builder import HubConnectionBuilder
import logging

logging.basicConfig(level=logging.INFO)

class ChatBot:
    def __init__(self, base_url: str, username: str, password: str):
        self.base_url = base_url
        self.username = username
        self.password = password
        self.connection = None
        self.room_id = None
        self.room_token = None
    
    async def start(self, room_id: str, room_password: str):
        # Login and get JWT token
        auth = AuthService(self.base_url)
        auth.login(self.username, self.password)
        
        # Join room via REST API
        response = auth.session.post(
            f"{self.base_url}/api/ChatRooms/join",
            json={"roomId": room_id, "password": room_password}
        )
        result = response.json()
        self.room_id = room_id
        self.room_token = result["roomAccessToken"]
        
        # Get JWT token from cookie
        jwt_token = auth.session.cookies.get("auth_token")
        
        # Connect to SignalR
        self.connection = HubConnectionBuilder() \
            .with_url(f"{self.base_url}/chatHub?access_token={jwt_token}") \
            .with_automatic_reconnect({
                "type": "raw",
                "keep_alive_interval": 10,
                "reconnect_interval": 5,
                "max_attempts": 5
            }) \
            .build()
        
        self.connection.on("ReceiveMessage", self.on_message)
        self.connection.on_open(lambda: self.on_connected())
        self.connection.on_close(lambda: print("Connection closed"))
        
        self.connection.start()
        
        # Keep running
        while True:
            await asyncio.sleep(1)
    
    def on_connected(self):
        print("Connected to chat hub")
        self.connection.send("JoinRoom", [self.room_id, self.room_token])
    
    def on_message(self, message):
        sender = message["sender"]
        content = message["content"]
        print(f"{sender}: {content}")
        
        # Respond to specific commands
        if content.lower() == "!help":
            self.connection.send("SendMessage", [
                self.room_id,
                "Available commands: !help, !time, !ping"
            ])
        elif content.lower() == "!time":
            from datetime import datetime
            self.connection.send("SendMessage", [
                self.room_id,
                f"Current time: {datetime.now().strftime('%H:%M:%S')}"
            ])
        elif content.lower() == "!ping":
            self.connection.send("SendMessage", [self.room_id, "Pong!"])

# Run bot
bot = ChatBot("https://localhost:7266", "botuser", "Bot123")
asyncio.run(bot.start("room-id-here", "room-password"))
```

---

**Version:** 1.0  
**Last Updated:** January 9, 2026  
**Related Guides:**
- [Getting Started](./GETTING_STARTED.md)
- [Authentication](./AUTHENTICATION.md)
- [SignalR Documentation](./signalr/QUICK_REFERENCE.md)
