# Chat Room API Test Cases

**Module:** Real-time Chat  
**Controllers:** ChatRoomsController, ChatHub (SignalR)  
**Base Path:** `/api/ChatRooms`, `/chatHub`

---

## REST API Test Cases

### TC-CHAT-001: Create Chat Room - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Logged in as Teacher/Admin
- School and Class exist

**Test Data:**
```json
{
  "name": "Math Class Discussion",
  "description": "Discussion room for Math homework",
  "privacyLevel": "Private",
  "password": "SecurePass123",
  "classId": "<VALID_CLASS_GUID>",
  "schoolId": "<VALID_SCHOOL_GUID>",
  "createdBy": "<TEACHER_USER_GUID>"
}
```

**Steps:**
1. Send POST request to `/api/ChatRooms`
2. Include room data in body

**Expected Result:**
- Status Code: `201 Created`
- Response Body:
  ```json
  {
    "isSuccess": true,
    "data": {
      "id": "<ROOM_GUID>",
      "name": "Math Class Discussion",
      "privacyLevel": "Private",
      "createdAt": "<ISO_DATETIME>",
      "accessToken": "<ENCRYPTED_TOKEN>"
    }
  }
  ```
- Room created in database
- Access token generated for private rooms

**Postman Test:**
```javascript
pm.test("Chat room created", () => {
    pm.response.to.have.status(201);
    const jsonData = pm.response.json();
    pm.environment.set("roomId", jsonData.data.id);
    pm.environment.set("roomToken", jsonData.data.accessToken);
});
```

---

### TC-CHAT-002: Create Chat Room - Public Room (No Password)

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Test Data:**
```json
{
  "name": "General Announcements",
  "privacyLevel": "Public",
  "schoolId": "<VALID_SCHOOL_GUID>"
}
```

**Expected Result:**
- Status Code: `201 Created`
- No password required
- No access token generated

---

### TC-CHAT-003: Create Chat Room - Duplicate Name

**Priority:** Medium  
**Type:** Negative  
**Category:** Validation

**Prerequisites:**
- Room "Math Class" already exists in same school

**Test Data:**
```json
{
  "name": "Math Class",
  "schoolId": "<SAME_SCHOOL_GUID>"
}
```

**Expected Result:**
- Status Code: `400 Bad Request` or `409 Conflict`
- Error message: "Room with this name already exists"

---

### TC-CHAT-004: Get All Chat Rooms - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Multiple chat rooms exist
- User is authenticated

**Steps:**
1. Send GET request to `/api/ChatRooms`

**Expected Result:**
- Status Code: `200 OK`
- Response contains list of accessible rooms
- Private rooms only shown if user has access

---

### TC-CHAT-005: Get Chat Room by ID - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Send GET request to `/api/ChatRooms/{id}`

**Expected Result:**
- Status Code: `200 OK`
- Response contains room details and recent messages

---

### TC-CHAT-006: Join Private Room - With Valid Password

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Private room exists with password "SecurePass123"

**Test Data:**
```json
{
  "roomId": "<ROOM_GUID>",
  "password": "SecurePass123"
}
```

**Steps:**
1. Send POST request to `/api/ChatRooms/{id}/join`

**Expected Result:**
- Status Code: `200 OK`
- Response contains room access token
- User added to room members

---

### TC-CHAT-007: Join Private Room - Invalid Password

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Test Data:**
```json
{
  "roomId": "<ROOM_GUID>",
  "password": "WrongPassword"
}
```

**Expected Result:**
- Status Code: `401 Unauthorized` or `403 Forbidden`
- Error message: "Invalid password"
- User NOT added to room

---

### TC-CHAT-008: Leave Chat Room - Success

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Send POST request to `/api/ChatRooms/{id}/leave`

**Expected Result:**
- Status Code: `200 OK`
- User removed from room members
- Access revoked

---

### TC-CHAT-009: Delete Chat Room - Success

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Logged in as room creator or Admin

**Steps:**
1. Send DELETE request to `/api/ChatRooms/{id}`

**Expected Result:**
- Status Code: `204 No Content`
- Room deleted from database
- All messages deleted (or archived)
- All members notified

---

### TC-CHAT-010: Delete Chat Room - Unauthorized User

**Priority:** High  
**Type:** Negative  
**Category:** Authorization

**Prerequisites:**
- Logged in as Student (not room creator)

**Expected Result:**
- Status Code: `403 Forbidden`
- Error message: "Only room creator or admin can delete"

---

## SignalR Hub Test Cases

### TC-CHAT-HUB-001: Connect to Chat Hub - Success

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Valid JWT token

**Steps:**
1. Establish WebSocket connection to `/chatHub`
2. Include auth token in query string or header

**Expected Result:**
- Connection established successfully
- Connection ID assigned
- User added to online users list

**JavaScript Test:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", { accessTokenFactory: () => authToken })
    .build();

await connection.start();
console.assert(connection.state === "Connected");
```

---

### TC-CHAT-HUB-002: Connect to Chat Hub - No Auth Token

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Steps:**
1. Attempt to connect without auth token

**Expected Result:**
- Connection rejected
- Error: "Unauthorized"

---

### TC-CHAT-HUB-003: Join Room via Hub - Success

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Connected to hub
- User has access to room

**Steps:**
1. Invoke hub method: `JoinRoom(roomId, accessToken)`

**Expected Result:**
- User added to SignalR room group
- Confirmation message received
- Other room members notified of new user

**JavaScript Test:**
```javascript
connection.on("UserJoined", (userId, userName) => {
    console.log(`${userName} joined the room`);
});

await connection.invoke("JoinRoom", roomId, accessToken);
```

---

### TC-CHAT-HUB-004: Join Room - Invalid Access Token

**Priority:** High  
**Type:** Negative  
**Category:** Security

**Steps:**
1. Invoke `JoinRoom` with invalid token

**Expected Result:**
- Error returned
- User NOT added to room
- Message: "Invalid access token"

---

### TC-CHAT-HUB-005: Send Message - Success

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- User joined room

**Test Data:**
```json
{
  "roomId": "<ROOM_GUID>",
  "content": "Hello, everyone!",
  "isEncrypted": true
}
```

**Steps:**
1. Invoke hub method: `SendMessage(roomId, content)`

**Expected Result:**
- Message encrypted (if enabled)
- Message saved to database
- Message broadcast to all room members
- Sender receives confirmation

**JavaScript Test:**
```javascript
connection.on("ReceiveMessage", (message) => {
    console.log(`${message.senderName}: ${message.content}`);
});

await connection.invoke("SendMessage", roomId, "Hello, everyone!");
```

---

### TC-CHAT-HUB-006: Send Message - Encryption Validation

**Priority:** High  
**Type:** Security  
**Category:** Integration

**Steps:**
1. Send message to encrypted room
2. Check database directly

**Expected Result:**
- Message content in database is encrypted (not plain text)
- Decrypted message matches original
- Recipients receive decrypted message

---

### TC-CHAT-HUB-007: Send Message - Flood Protection

**Priority:** High  
**Type:** Security  
**Category:** Integration

**Prerequisites:**
- Flood protection enabled (30 messages/minute)

**Steps:**
1. Send 35 messages in 1 minute

**Expected Result:**
- First 30 messages sent successfully
- Messages 31-35 rejected
- Error: "Rate limit exceeded"
- User temporarily throttled

---

### TC-CHAT-HUB-008: Send Message - Profanity Filter

**Priority:** Medium  
**Type:** Functional  
**Category:** Integration

**Test Data:**
- Message content contains profanity

**Expected Result:**
- Message rejected OR filtered
- User warned (depending on policy)

---

### TC-CHAT-HUB-009: Receive Real-time Message

**Priority:** Critical  
**Type:** Positive  
**Category:** Integration

**Prerequisites:**
- Two users connected to same room

**Steps:**
1. User A sends message
2. User B receives message in real-time

**Expected Result:**
- Message delivered within 100ms
- Message contains sender info, content, timestamp
- No page refresh required

---

### TC-CHAT-HUB-010: Message History - Load Previous Messages

**Priority:** High  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Join room with existing message history
2. Request previous messages: `GetMessageHistory(roomId, skip=0, take=50)`

**Expected Result:**
- Last 50 messages returned
- Messages in chronological order
- Encrypted messages decrypted for authorized users

---

### TC-CHAT-HUB-011: Leave Room via Hub

**Priority:** Medium  
**Type:** Positive  
**Category:** Integration

**Steps:**
1. Invoke hub method: `LeaveRoom(roomId)`

**Expected Result:**
- User removed from SignalR group
- Other members notified
- User stops receiving messages from room

---

### TC-CHAT-HUB-012: Disconnect from Hub - Cleanup

**Priority:** Medium  
**Type:** Functional  
**Category:** Integration

**Steps:**
1. Disconnect from hub

**Expected Result:**
- User removed from all rooms
- Connection resources cleaned up
- Online status updated

---

### TC-CHAT-HUB-013: Typing Indicator

**Priority:** Low  
**Type:** Functional  
**Category:** Integration

**Steps:**
1. User A starts typing
2. Invoke: `NotifyTyping(roomId)`

**Expected Result:**
- Other users see "User A is typing..."
- Indicator clears after 3 seconds or on message send

---

### TC-CHAT-HUB-014: Read Receipts

**Priority:** Low  
**Type:** Functional  
**Category:** Integration

**Steps:**
1. User reads messages
2. Invoke: `MarkAsRead(messageIds)`

**Expected Result:**
- Messages marked as read in database
- Sender notified of read status

---

### TC-CHAT-HUB-015: Connection Resilience - Auto Reconnect

**Priority:** High  
**Type:** Functional  
**Category:** Integration

**Steps:**
1. Simulate network interruption
2. Restore connection

**Expected Result:**
- Client automatically reconnects
- User rejoins previous rooms
- Missed messages delivered

**JavaScript Test:**
```javascript
connection.onreconnecting((error) => {
    console.log("Reconnecting...");
});

connection.onreconnected((connectionId) => {
    console.log("Reconnected with ID:", connectionId);
});
```

---

## Performance Test Cases

### TC-CHAT-PERF-001: Concurrent Users in Single Room

**Priority:** High  
**Type:** Performance  
**Category:** Load Test

**Setup:**
- 100 users connected to same room
- Each sends 1 message per second

**Expected Result:**
- All messages delivered within 100ms
- No message loss
- Server CPU < 80%
- Memory usage stable

---

### TC-CHAT-PERF-002: Multiple Concurrent Rooms

**Priority:** High  
**Type:** Performance  
**Category:** Load Test

**Setup:**
- 10 rooms with 20 users each
- Total 200 active connections

**Expected Result:**
- All rooms function independently
- Message delivery < 100ms
- No cross-room message leakage

---

## Test Execution Checklist

- [ ] REST API endpoints tested
- [ ] SignalR hub methods tested
- [ ] Message encryption validated
- [ ] Flood protection working
- [ ] Real-time delivery confirmed
- [ ] Authorization checks pass
- [ ] Performance benchmarks met
- [ ] Connection resilience verified

---

**Total Test Cases:** 28  
**Critical:** 6  
**High:** 14  
**Medium:** 6  
**Low:** 2  
**Coverage:** REST API, SignalR, Security, Real-time, Performance
