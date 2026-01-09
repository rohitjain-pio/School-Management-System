# 🎯 SignalR Quick Reference

## Start Backend
```powershell
cd Backend/SMSPrototype1
dotnet run
```

## Start Frontend  
```powershell
cd Frontend
npm run dev
```

---

## 🔐 Security Features

✅ **[Authorize]** - Required on all hub methods  
✅ **JWT Token** - User identity from authenticated claims  
✅ **Message Validation** - Max 1000 characters, not empty  
✅ **Room Verification** - Room must exist before join  
✅ **No Username Spoofing** - Username from JWT only  

---

## 💾 Database

**Table:** `ChatMessages`  
**Indexes:** RoomId, UserId, Timestamp  
**Foreign Keys:** → ChatRooms, → AspNetUsers (CASCADE)  
**Fields:** Id, RoomId, UserId, Content, Timestamp, IsDeleted, IsEdited

---

## 📡 Hub Methods

```typescript
// Join room (auto-loads last 50 messages)
await connection.invoke("JoinRoom", roomId);

// Send message (auto-saves to database)
await connection.invoke("SendMessage", roomId, message);

// Load history manually
const messages = await connection.invoke("LoadMessageHistory", roomId, 50);

// Leave room
await connection.invoke("LeaveRoom", roomId);

// Typing notification
await connection.invoke("SendTyping", roomId, username);
```

---

## 📨 Client Events

```typescript
connection.on("ReceiveMessage", (msg) => { ... });
connection.on("ReceiveTyping", (user) => { ... });
connection.on("UserListUpdated", (users) => { ... });
connection.on("UserJoined", (user) => { ... });
connection.on("UserLeft", (user) => { ... });
```

---

## ✅ Testing

1. **Login first** (authentication required)
2. Navigate to Meeting/Chat in dashboard
3. Join or create room
4. Send messages
5. Refresh → history loads
6. Open incognito → test multi-user

---

## 🚨 Key Points

- ✅ Must be logged in to access chat
- ✅ All messages saved to database automatically
- ✅ Message history loads on room join
- ✅ Username cannot be faked (from JWT)
- ✅ No anonymous access allowed
- ✅ Real-time updates for all users in room

---

## 📄 Documentation Files

- **SIGNALR_IMPLEMENTATION_SUMMARY.md** - Complete overview
- **SIGNALR_SECURITY_AND_PERSISTENCE.md** - Security details
- **SIGNALR_FIXES_AND_TESTING.md** - Testing guide
- **signalr-test.html** - Standalone test page

---

## 🎉 Status: PRODUCTION READY ✅
