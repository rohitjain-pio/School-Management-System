import React, { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "@/hooks/use-toast";

const server_url = import.meta.env.VITE_API_URL;

interface AddRoomPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (room: { name: string; description: string; password: string; createdBy: string }) => void;
}

const AddRoomPopup: React.FC<AddRoomPopupProps> = ({ isOpen, onClose, onSubmit }) => {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [password, setPassword] = useState("");
  const [createdBy, setCreatedBy] = useState("Loading...");

  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const res = await fetch(`${server_url}/api/Authentication/me`, {
          credentials: "include",
          cache: "no-store",
        });

        if (!res.ok) throw new Error("Failed to fetch user");

        const userData = await res.json();
        setCreatedBy(userData.username || userData.name || "Unknown");

      } catch (error) {
        console.error("Error fetching user:", error);
        setCreatedBy("Unknown");
      }
    };

    if (isOpen) {
      fetchUserInfo();
    }
  }, [isOpen]);

  const handleSubmit = () => {
    if (!name || !password || !createdBy || createdBy === "Unknown") {
      toast({
        title: "Missing Fields",
        description: "Room Name, Password, and Creator are required.",
        variant: "destructive",
      });
      return;
    }

    onSubmit({ name, description, password, createdBy });

    setName("");
    setDescription("");
    setPassword("");
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create Meeting Room</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <Label>Room Name *</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </div>
          <div>
            <Label>Description (optional)</Label>
            <Input value={description} onChange={(e) => setDescription(e.target.value)} />
          </div>
          <div>
            <Label>Password *</Label>
            <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </div>
          <div>
            <Label>Created By</Label>
            <Input value={createdBy} disabled />
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button onClick={handleSubmit}>Create</Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default AddRoomPopup;
