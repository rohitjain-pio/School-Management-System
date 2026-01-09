import React from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

interface DeleteRoomPopupProps {
  isOpen: boolean;
  roomData: { id: string; name: string };
  onClose: () => void;
  onConfirm: () => void;
}

const DeleteRoomPopup: React.FC<DeleteRoomPopupProps> = ({
  isOpen,
  roomData,
  onClose,
  onConfirm,
}) => {
  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Room</DialogTitle>
        </DialogHeader>
        <p>Are you sure you want to delete the room <strong>{roomData.name}</strong>?</p>
        <div className="flex justify-end gap-2 mt-4">
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="destructive" onClick={onConfirm}>
            Delete
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default DeleteRoomPopup;
