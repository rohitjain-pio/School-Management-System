import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Plus,
  DoorOpen,
  Eye,
  Trash2,
  MessageSquareText,
  Video,
  LockKeyhole,
} from "lucide-react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import EmptyState from "@/components/EmptyState";
import AddRoomPopup from "@/popups/meetings/AddRoomPopup";
import DeleteRoomPopup from "@/popups/meetings/DeleteRoomPopup";
import JoinRoomPopup from "@/popups/meetings/JoinRoomPopup";
import { useRooms } from "@/hooks/useRooms";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";

interface MeetingRoom {
  id: string;
  name: string;
  description: string;
  password: string;
  createdBy: string;
}

const MeetingRooms: React.FC = () => {
  const {
    data:rooms = [],
    isLoading,
    error,
    addRoom,
    joinRoom,
    deleteRoom,
  } = useRooms();

  const { user } = useAuth();
  const [selectedRoom, setSelectedRoom] = useState<MeetingRoom | null>(null);
  const [modal, setModal] = useState<"add" | "delete" | "join" | null>(null);
  const [isOpen, setIsOpen] = useState(false);
  const navigate = useNavigate();

  // Check if user can delete a room
  const canDeleteRoom = (room: MeetingRoom) => {
    if (!user) return false;
    
    // Admins can delete any room
    const isAdmin = user.roles?.some(role => 
      ['Admin', 'SuperAdmin', 'Principal', 'SchoolIncharge'].includes(role)
    );
    
    // Room creator can delete their own room
    const isCreator = room.createdBy === user.id;
    
    return isAdmin || isCreator;
  };
  const openModal = (type: "add" | "delete" | "join", room?: MeetingRoom) => {
    setSelectedRoom(room || null);
    setModal(type);
    setIsOpen(true);
  };

  const closeModal = () => {
    setSelectedRoom(null);
    setModal(null);
    setIsOpen(false);
  };

  const handleAddRoom = async (newRoom: MeetingRoom) => {
    try {
      await addRoom({ room: newRoom });
      closeModal();
    } catch (err) {
      alert("Failed to create room");
    }
  };

  const handleDeleteRoom = async (id: string) => {
    try {
      console.log(id);

      await deleteRoom({id});
      closeModal();
    } catch (err: any) {
      if (err.message?.includes('403') || err.message?.includes('Forbidden')) {
        alert("You don't have permission to delete this room.");
      } else {
        alert("Failed to delete room");
      }
    }
  };

  const handleJoinRoom = async (roomId: string, enteredPassword: string, isVideoCall: boolean = false) => {
    try {
      const result = await joinRoom({ roomId, password: enteredPassword });
      console.log(result.ok)
      if (result.ok && result.roomAccessToken) {
        // Store room access token in sessionStorage for the SignalR connection
        sessionStorage.setItem(`roomAccessToken_${roomId}`, result.roomAccessToken);
        navigate(isVideoCall ? `/video-call/${roomId}` : `/chat/${roomId}`);
      } else {
        alert(result.message || "Incorrect password");
      }
    } catch (err: any) {
      alert(err.message || "Failed to join room. Please try again.");
    }
  };
  if (isLoading) return <p>Loading rooms...</p>;
  if (error) return <p className="text-red-500">Failed to load meeting rooms</p>;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl sm:text-3xl font-bold text-gray-900">
            Meeting Rooms
          </h2>
          <p className="text-gray-600 mt-2">
            Join or manage real-time chat rooms. Video/audio coming soon!
          </p>
        </div>
        <Button onClick={() => openModal("add")} className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Create Room</span>
        </Button>
      </div>

      {rooms.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 sm:gap-6">
          {rooms.map((room) => (
            <Card key={room.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <DoorOpen className="h-5 w-5 text-primary-600" />
                  <span className="text-sm sm:text-base">{room.name}</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <p className="text-sm text-gray-600">{room.description || "No description"}</p>
                <p className="text-sm text-gray-600">
                  <span className="font-medium">Created by:</span> {room.createdBy || "N/A"}
                </p>

                <div className="flex gap-2 pt-3">
                  <TooltipProvider>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button
                          size="sm"
                          variant="outline"
                          className="w-1/3 justify-center"
                          onClick={() => openModal("join", room)}
                        >
                          <MessageSquareText className="w-4 h-4 mr-1" />
                          Chat
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Join room with password</TooltipContent>
                    </Tooltip>

                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button
                          size="sm"
                          variant="outline"
                          className="w-1/3 justify-center"
                          onClick={() => openModal("join", room)}
                        >
                          <Video className="w-4 h-4 mr-1" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>Join video call</TooltipContent>
                    </Tooltip>

                    {canDeleteRoom(room) && (
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <Button
                            size="sm"
                            variant="outline"
                            className="w-1/3 justify-center"
                            onClick={() => openModal("delete", room)}
                          >
                            <Trash2 className="w-4 h-4 mr-1 text-red-500" />
                          </Button>
                        </TooltipTrigger>
                        <TooltipContent>Delete this room</TooltipContent>
                      </Tooltip>
                    )}
                  </TooltipProvider>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={<LockKeyhole className="h-12 w-12 text-primary-700" />}
          title="No Meeting Rooms Found"
          description="Create a room and start real-time communication with your peers."
          buttonText="Create Room"
          onClick={() => openModal("add")}
        />
      )}

      {/* Popups */}
      {modal === "add" && (
        <AddRoomPopup isOpen={isOpen} onClose={closeModal} onSubmit={handleAddRoom} />
      )}
      {modal === "delete" && selectedRoom && (
        <DeleteRoomPopup
          isOpen={isOpen}
          roomData={selectedRoom}
          onClose={closeModal}
          onConfirm={() => handleDeleteRoom(selectedRoom.id)}
        />
      )}
      {modal === "join" && selectedRoom && (
        <JoinRoomPopup
          isOpen={isOpen}
          roomData={selectedRoom}
          onClose={closeModal}
          onSubmit={(password, isVideoCall) => handleJoinRoom(selectedRoom.id, password, isVideoCall)}
        />
      )}
    </div>
  );
};

export default MeetingRooms;
