import React, { useEffect, useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { Button } from "@/components/ui/button";
import { 
  Mic, 
  MicOff, 
  Video, 
  VideoOff, 
  PhoneOff, 
  Monitor,
  ArrowLeft,
  Users
} from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

interface Participant {
  connectionId: string;
  username: string;
  userId: string;
  stream?: MediaStream;
  audioEnabled?: boolean;
  videoEnabled?: boolean;
}

const VideoCallPage: React.FC = () => {
  const { id: roomId } = useParams();
  const navigate = useNavigate();
  const apiUrl = import.meta.env.VITE_API_URL;

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [localStream, setLocalStream] = useState<MediaStream | null>(null);
  const [participants, setParticipants] = useState<Participant[]>([]);
  const [audioEnabled, setAudioEnabled] = useState(true);
  const [videoEnabled, setVideoEnabled] = useState(true);
  const [username, setUsername] = useState<string>("");

  const localVideoRef = useRef<HTMLVideoElement>(null);
  const peerConnections = useRef<Map<string, RTCPeerConnection>>(new Map());

  const ICE_SERVERS = {
    iceServers: [
      { urls: "stun:stun.l.google.com:19302" },
      { urls: "stun:stun1.l.google.com:19302" },
    ],
  };

  // Fetch user info
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
        }
      } catch (err) {
        console.error("Failed to fetch user", err);
      }
    };
    fetchUser();
  }, [apiUrl]);

  // Get local media stream
  useEffect(() => {
    const initLocalStream = async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({
          video: true,
          audio: true,
        });
        setLocalStream(stream);
        if (localVideoRef.current) {
          localVideoRef.current.srcObject = stream;
        }
      } catch (err) {
        console.error("Error accessing media devices:", err);
        alert("Unable to access camera/microphone. Please grant permissions.");
      }
    };

    initLocalStream();

    return () => {
      localStream?.getTracks().forEach((track) => track.stop());
    };
  }, []);

  // SignalR connection
  useEffect(() => {
    if (!roomId || !localStream) return;

    // Get room access token from sessionStorage
    const roomAccessToken = sessionStorage.getItem(`roomAccessToken_${roomId}`);
    if (!roomAccessToken) {
      console.error("No room access token found. Please join the room first.");
      navigate("/dashboard/meeting");
      return;
    }

    let isMounted = true;
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${apiUrl}/videoCallHub`, {
        withCredentials: true,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Handle existing participants
    newConnection.on("ExistingParticipants", async (existingParticipants: Participant[]) => {
      if (!isMounted) return;
      console.log("Existing participants:", existingParticipants);
      
      for (const participant of existingParticipants) {
        await createPeerConnection(participant.connectionId, true);
      }
      
      setParticipants(existingParticipants);
    });

    // Handle new user joining
    newConnection.on("UserJoinedCall", async (user: Participant) => {
      if (!isMounted) return;
      console.log("User joined:", user);
      await createPeerConnection(user.connectionId, false);
      setParticipants((prev) => [...prev, user]);
    });

    // Handle user leaving
    newConnection.on("UserLeftCall", (connectionId: string) => {
      if (!isMounted) return;
      console.log("User left:", connectionId);
      const pc = peerConnections.current.get(connectionId);
      if (pc) {
        pc.close();
        peerConnections.current.delete(connectionId);
      }
      setParticipants((prev) => prev.filter((p) => p.connectionId !== connectionId));
    });

    // WebRTC signaling handlers
    newConnection.on("ReceiveOffer", async (fromConnectionId: string, offer: RTCSessionDescriptionInit) => {
      console.log("Received offer from:", fromConnectionId);
      const pc = peerConnections.current.get(fromConnectionId);
      if (pc) {
        await pc.setRemoteDescription(new RTCSessionDescription(offer));
        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        await newConnection.invoke("SendAnswer", fromConnectionId, answer);
      }
    });

    newConnection.on("ReceiveAnswer", async (fromConnectionId: string, answer: RTCSessionDescriptionInit) => {
      console.log("Received answer from:", fromConnectionId);
      const pc = peerConnections.current.get(fromConnectionId);
      if (pc) {
        await pc.setRemoteDescription(new RTCSessionDescription(answer));
      }
    });

    newConnection.on("ReceiveIceCandidate", async (fromConnectionId: string, candidate: RTCIceCandidateInit) => {
      const pc = peerConnections.current.get(fromConnectionId);
      if (pc && candidate) {
        await pc.addIceCandidate(new RTCIceCandidate(candidate));
      }
    });

    newConnection.on("ParticipantMediaStateChanged", (connectionId: string, audioEnabled: boolean, videoEnabled: boolean) => {
      setParticipants((prev) =>
        prev.map((p) =>
          p.connectionId === connectionId
            ? { ...p, audioEnabled, videoEnabled }
            : p
        )
      );
    });

    newConnection.on("Kicked", (reason: string) => {
      if (isMounted) {
        alert(`You were kicked from the video call: ${reason}`);
        navigate("/dashboard/meeting");
      }
    });

    const start = async () => {
      try {
        if (newConnection.state === "Disconnected" && isMounted) {
          await newConnection.start();
          console.log("Connected to VideoCall SignalR");
          // Join video room with access token for authorization
          await newConnection.invoke("JoinVideoRoom", roomId, roomAccessToken);
        }
      } catch (err) {
        console.error("VideoCall SignalR Connection Error:", err);
        alert("Failed to join video call. You may not have permission.");
        navigate("/dashboard/meeting");
      }
    };

    const createPeerConnection = async (targetConnectionId: string, createOffer: boolean) => {
      const pc = new RTCPeerConnection(ICE_SERVERS);

      // Add local stream tracks
      localStream.getTracks().forEach((track) => {
        pc.addTrack(track, localStream);
      });

      // Handle incoming stream
      pc.ontrack = (event) => {
        console.log("Received remote track from:", targetConnectionId);
        setParticipants((prev) =>
          prev.map((p) =>
            p.connectionId === targetConnectionId
              ? { ...p, stream: event.streams[0] }
              : p
          )
        );
      };

      // Handle ICE candidates
      pc.onicecandidate = (event) => {
        if (event.candidate) {
          newConnection.invoke("SendIceCandidate", targetConnectionId, event.candidate);
        }
      };

      peerConnections.current.set(targetConnectionId, pc);

      // Create and send offer if initiator
      if (createOffer) {
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await newConnection.invoke("SendOffer", targetConnectionId, offer);
      }
    };

    start();
    setConnection(newConnection);

    return () => {
      isMounted = false;
      if (newConnection.state === "Connected") {
        newConnection.invoke("LeaveVideoRoom", roomId).catch(console.error);
        newConnection.stop();
      }
      peerConnections.current.forEach((pc) => pc.close());
      peerConnections.current.clear();
    };
  }, [roomId, apiUrl, localStream]);

  const toggleAudio = () => {
    if (localStream) {
      const audioTrack = localStream.getAudioTracks()[0];
      audioTrack.enabled = !audioTrack.enabled;
      setAudioEnabled(audioTrack.enabled);
      connection?.invoke("UpdateMediaState", roomId, audioTrack.enabled, videoEnabled);
    }
  };

  const toggleVideo = () => {
    if (localStream) {
      const videoTrack = localStream.getVideoTracks()[0];
      videoTrack.enabled = !videoTrack.enabled;
      setVideoEnabled(videoTrack.enabled);
      connection?.invoke("UpdateMediaState", roomId, audioEnabled, videoTrack.enabled);
    }
  };

  const endCall = () => {
    localStream?.getTracks().forEach((track) => track.stop());
    connection?.invoke("LeaveVideoRoom", roomId);
    connection?.stop();
    navigate("/dashboard/meeting");
  };

  return (
    <div className="flex flex-col h-screen bg-gray-900">
      {/* Header */}
      <div className="flex items-center justify-between p-4 bg-gray-800 text-white">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => navigate("/dashboard/meeting")}>
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <h2 className="text-xl font-semibold">Video Call - Room #{roomId?.substring(0, 8)}</h2>
        </div>

        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon">
              <Users className="w-5 h-5" />
              <span className="ml-1">{participants.length + 1}</span>
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-48">
            <h4 className="text-sm font-semibold mb-2">Participants</h4>
            <ul className="space-y-1 text-sm">
              <li>{username} (You)</li>
              {participants.map((p) => (
                <li key={p.connectionId}>{p.username}</li>
              ))}
            </ul>
          </PopoverContent>
        </Popover>
      </div>

      {/* Video Grid */}
      <div className="flex-1 p-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 overflow-auto">
        {/* Local Video */}
        <div className="relative bg-gray-800 rounded-lg overflow-hidden aspect-video">
          <video
            ref={localVideoRef}
            autoPlay
            muted
            playsInline
            className="w-full h-full object-cover"
          />
          <div className="absolute bottom-2 left-2 bg-black bg-opacity-60 px-2 py-1 rounded text-white text-sm">
            {username} (You)
          </div>
          {!videoEnabled && (
            <div className="absolute inset-0 flex items-center justify-center bg-gray-700">
              <VideoOff className="w-12 h-12 text-gray-400" />
            </div>
          )}
        </div>

        {/* Remote Videos */}
        {participants.map((participant) => (
          <RemoteVideo key={participant.connectionId} participant={participant} />
        ))}
      </div>

      {/* Controls */}
      <div className="p-6 bg-gray-800 flex justify-center gap-4">
        <Button
          size="lg"
          variant={audioEnabled ? "secondary" : "destructive"}
          className="rounded-full w-14 h-14"
          onClick={toggleAudio}
        >
          {audioEnabled ? <Mic className="w-6 h-6" /> : <MicOff className="w-6 h-6" />}
        </Button>

        <Button
          size="lg"
          variant={videoEnabled ? "secondary" : "destructive"}
          className="rounded-full w-14 h-14"
          onClick={toggleVideo}
        >
          {videoEnabled ? <Video className="w-6 h-6" /> : <VideoOff className="w-6 h-6" />}
        </Button>

        <Button
          size="lg"
          variant="destructive"
          className="rounded-full w-14 h-14"
          onClick={endCall}
        >
          <PhoneOff className="w-6 h-6" />
        </Button>
      </div>
    </div>
  );
};

const RemoteVideo: React.FC<{ participant: Participant }> = ({ participant }) => {
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (videoRef.current && participant.stream) {
      videoRef.current.srcObject = participant.stream;
    }
  }, [participant.stream]);

  return (
    <div className="relative bg-gray-800 rounded-lg overflow-hidden aspect-video">
      <video
        ref={videoRef}
        autoPlay
        playsInline
        className="w-full h-full object-cover"
      />
      <div className="absolute bottom-2 left-2 bg-black bg-opacity-60 px-2 py-1 rounded text-white text-sm">
        {participant.username}
      </div>
      {participant.videoEnabled === false && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-700">
          <VideoOff className="w-12 h-12 text-gray-400" />
        </div>
      )}
      {participant.audioEnabled === false && (
        <div className="absolute top-2 right-2 bg-red-600 p-1 rounded-full">
          <MicOff className="w-4 h-4 text-white" />
        </div>
      )}
    </div>
  );
};

export default VideoCallPage;
