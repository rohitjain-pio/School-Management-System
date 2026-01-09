import React, { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { MessageSquareText, Video } from "lucide-react";

interface JoinRoomPopupProps {
  isOpen: boolean;
  roomData: { id: string; name: string };
  onClose: () => void;
  onSubmit: (password: string, isVideoCall: boolean) => void;
}

const JoinRoomPopup: React.FC<JoinRoomPopupProps> = ({
  isOpen,
  roomData,
  onClose,
  onSubmit,
}) => {
  const [password, setPassword] = useState("");

  const handleJoin = (isVideoCall: boolean) => {
    if (!password) return alert("Password is required");
    onSubmit(password, isVideoCall);
    setPassword("");
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Join Room: {roomData.name}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <Label>Enter Room Password</Label>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleJoin(false);
              }}
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button 
              onClick={() => handleJoin(false)}
              className="flex items-center gap-2"
            >
              <MessageSquareText className="w-4 h-4" />
              Join Chat
            </Button>
            <Button 
              onClick={() => handleJoin(true)}
              className="flex items-center gap-2"
            >
              <Video className="w-4 h-4" />
              Join Video Call
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default JoinRoomPopup;
