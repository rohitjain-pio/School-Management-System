import React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

interface DeleteAnnouncementPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  announcementData: any;
}

const DeleteAnnouncementPopup: React.FC<DeleteAnnouncementPopupProps> = ({
  isOpen,
  onClose,
  onConfirm,
  announcementData,
}) => {
  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Announcement</DialogTitle>
        </DialogHeader>
        <div>
          Are you sure you want to delete the announcement titled{" "}
          <strong>"{announcementData?.title}"</strong>?
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="destructive" onClick={onConfirm}>
            Delete
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default DeleteAnnouncementPopup;
