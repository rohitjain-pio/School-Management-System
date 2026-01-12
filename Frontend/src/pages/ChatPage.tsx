import React, { useEffect, useRef, useState } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { useParams, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ArrowLeft, Send, Users, Smile } from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

interface Message {
  sender: string;
  content: string;
  timestamp: string;
}

const ChatPage: React.FC = () => {
  const { id: roomId } = useParams(); // Room ID from URL
  const navigate = useNavigate();
  const apiUrl = import.meta.env.VITE_API_URL;

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [username, setUsername] = useState<string>("");
  const [newMessage, setNewMessage] = useState("");
  const [typingUser, setTypingUser] = useState<string | null>(null);
  const [onlineUsers, setOnlineUsers] = useState<string[]>([]);
  const messageEndRef = useRef<HTMLDivElement | null>(null);
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [emojiPickerOpen, setEmojiPickerOpen] = useState(false);

  // Fetch logged-in user info
  // Fetch logged-in user info from /api/Auth/me
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await fetch(`${apiUrl}/api/Authentication/me`, {
          credentials: "include",
        });
        if (res.status === 401) {
          navigate('/login');
          return;
        }
        if (res.ok) {
          const data = await res.json();
          setUsername(data.username || data.email || "Anonymous");
        } else {
          console.error("Failed to fetch user info");
        }
      } catch (err) {
        console.error("Failed to fetch user from /api/Authentication/me", err);
      }
    };
    fetchUser();
  }, []);

  // Auto-scroll to bottom
  useEffect(() => {
    messageEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // SignalR connection
  useEffect(() => {
    if (!roomId) return;

    // Get room access token from sessionStorage
    const roomAccessToken = sessionStorage.getItem(`roomAccessToken_${roomId}`);
    if (!roomAccessToken) {
      console.error("No room access token found. Please join the room first.");
      navigate("/dashboard/meeting");
      return;
    }

    let isMounted = true;
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${apiUrl}/chatHub`, {
        withCredentials: true,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Register event listeners BEFORE starting connection
    newConnection.on("ReceiveMessage", (message: Message) => {
      if (isMounted) {
        setMessages((prev) => [...prev, message]);
      }
    });

    newConnection.on("ReceiveTyping", (user: string) => {
      if (isMounted) {
        setTypingUser(user);
        setTimeout(() => setTypingUser(null), 2000);
      }
    });

    // Fix: Use lowercase to match SignalR's default behavior
    newConnection.on("UserListUpdated", (users: string[]) => {
      if (isMounted) {
        console.log("UserListUpdated received:", users);
        setOnlineUsers(users);
      }
    });

    newConnection.on("UserJoined", (user: string) => {
      if (isMounted) {
        console.log(`${user} joined the room`);
      }
    });

    newConnection.on("UserLeft", (user: string) => {
      if (isMounted) {
        console.log(`${user} left the room`);
      }
    });

    newConnection.on("Kicked", (reason: string) => {
      if (isMounted) {
        alert(`You were kicked from the room: ${reason}`);
        navigate("/dashboard/meeting");
      }
    });

    const start = async () => {
      try {
        // Only start if not already connecting/connected
        if (newConnection.state === "Disconnected" && isMounted) {
          await newConnection.start();
          console.log("Connected to SignalR");

          if (!isMounted) {
            await newConnection.stop();
            return;
          }

          // Join room with access token for authorization
          await newConnection.invoke("JoinRoom", roomId, roomAccessToken);

          // Load message history
          const history = await newConnection.invoke("LoadMessageHistory", roomId, 50);
          if (history && history.length > 0 && isMounted) {
            setMessages(history);
          }
        }
      } catch (err) {
        console.error("SignalR Connection Error: ", err);
        alert("Failed to join room. You may not have permission.");
        navigate("/dashboard/meeting");
      }
    };

    start();
    setConnection(newConnection);

    return () => {
      isMounted = false;
      if (newConnection.state === "Connected") {
        newConnection.invoke("LeaveRoom", roomId)
          .catch(console.error)
          .finally(() => {
            newConnection.stop();
          });
      } else if (newConnection.state === "Connecting") {
        newConnection.stop();
      }
    };
  }, [roomId, apiUrl, navigate]);

  const handleSend = async () => {
    if (!newMessage.trim() || !connection) return;

    try {
      // SendMessage now uses authenticated user automatically
      await connection.invoke("SendMessage", roomId, newMessage);
      setNewMessage("");
    } catch (err) {
      console.error("Failed to send message: ", err);
      alert("Failed to send message. Please check your authentication.");
    }
  };

  const handleTyping = async () => {
    if (connection && username) {
      try {
        await connection.invoke("SendTyping", roomId, username);
      } catch (err) {
        // Silently fail for typing indicator
        console.debug("Typing notification failed", err);
      }
    }
  };

  const handleEmojiSelect = (emoji: string) => {
    const input = inputRef.current;
    if (input) {
      const start = input.selectionStart || 0;
      const end = input.selectionEnd || 0;
      const newText = newMessage.substring(0, start) + emoji + newMessage.substring(end);
      setNewMessage(newText);
      
      // Set cursor position after emoji
      setTimeout(() => {
        input.focus();
        const newCursorPos = start + emoji.length;
        input.setSelectionRange(newCursorPos, newCursorPos);
      }, 0);
    } else {
      setNewMessage(prev => prev + emoji);
    }
    setEmojiPickerOpen(false);
  };

  // Common emojis list
  const emojis = [
    "😀", "😃", "😄", "😁", "😅", "😂", "🤣", "😊",
    "😇", "🙂", "🙃", "😉", "😌", "😍", "🥰", "😘",
    "😗", "😙", "😚", "😋", "😛", "😝", "😜", "🤪",
    "🤨", "🧐", "🤓", "😎", "🤩", "🥳", "😏", "😒",
    "😞", "😔", "😟", "😕", "🙁", "😣", "😖", "😫",
    "😩", "🥺", "😢", "😭", "😤", "😠", "😡", "🤬",
    "🤯", "😳", "🥵", "🥶", "😱", "😨", "😰", "😥",
    "😓", "🤗", "🤔", "🤭", "🤫", "🤥", "😶", "😐",
    "👍", "👎", "👌", "✌️", "🤞", "🤟", "🤘", "🤙",
    "👏", "🙌", "👐", "🤲", "🤝", "🙏", "✍️", "💪",
    "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍",
    "💔", "❤️‍🔥", "💕", "💞", "💓", "💗", "💖", "💘",
    "🎉", "🎊", "🎈", "🎁", "🏆", "🥇", "🥈", "🥉",
    "⭐", "🌟", "✨", "💫", "🔥", "💯", "✅", "❌"
  ];

  return (
    <div className="flex flex-col h-[calc(100vh-80px)] max-w-4xl mx-auto px-4 py-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" onClick={() => navigate("/dashboard/meeting")}>
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <h2 className="text-xl font-semibold text-gray-800">Room #{roomId}</h2>
        </div>

        <Popover>
          <PopoverTrigger asChild>
            <Button variant="outline" size="icon">
              <Users className="w-5 h-5" />
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-48">
            <h4 className="text-sm font-semibold mb-2">Online Users</h4>
            <ul className="space-y-1 text-sm text-gray-700">
              {onlineUsers.length > 0 ? (
                onlineUsers.map((user) => <li key={user}>{user}</li>)
              ) : (
                <li>No one online</li>
              )}
            </ul>
          </PopoverContent>
        </Popover>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto border rounded-md p-4 space-y-3 bg-white shadow-sm">
        {messages.map((msg, index) => {
          const isOwn = msg.sender === username;
          return (
            <div
              key={index}
              className={`flex flex-col ${isOwn ? "items-end text-right" : "items-start text-left"}`}
            >
              <span className="text-xs font-medium text-gray-500">{msg.sender}</span>
              <div
                className={`inline-block rounded-md px-3 py-2 text-sm max-w-xs shadow ${
                  isOwn ? "bg-blue-600 text-white" : "bg-gray-100 text-gray-800"
                }`}
              >
                {msg.content}
              </div>
              <span className="text-[11px] text-gray-400 mt-1">
                {msg.timestamp ? new Date(msg.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : ''}
              </span>
            </div>
          );
        })}

        {/* Typing indicator */}
        {typingUser && typingUser !== username && (
          <div className="text-sm text-gray-400 italic">{typingUser} is typing...</div>
        )}

        <div ref={messageEndRef} />
      </div>

      {/* Input */}
      <div className="mt-4 flex items-center gap-2">
        <Popover open={emojiPickerOpen} onOpenChange={setEmojiPickerOpen}>
          <PopoverTrigger asChild>
            <Button variant="outline" size="icon" type="button">
              <Smile className="w-5 h-5" />
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-80" align="start">
            <div className="space-y-2">
              <h4 className="text-sm font-semibold">Select Emoji</h4>
              <div className="grid grid-cols-8 gap-2 max-h-64 overflow-y-auto">
                {emojis.map((emoji, index) => (
                  <button
                    key={index}
                    type="button"
                    onClick={() => handleEmojiSelect(emoji)}
                    className="text-2xl hover:bg-gray-100 rounded p-1 transition-colors"
                  >
                    {emoji}
                  </button>
                ))}
              </div>
            </div>
          </PopoverContent>
        </Popover>
        <Input
          ref={inputRef}
          placeholder="Type your message..."
          value={newMessage}
          onChange={(e) => {
            setNewMessage(e.target.value);
            handleTyping();
          }}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault(); // Prevent form submission
              handleSend();
            }
          }}
        />
        <Button onClick={handleSend} className="flex items-center gap-1">
          <Send className="w-4 h-4" />
          Send
        </Button>
      </div>
    </div>
  );
};

export default ChatPage;
