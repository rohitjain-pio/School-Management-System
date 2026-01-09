import React from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { AlertTriangle } from "lucide-react";

interface DeleteTeacherPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  teacherData: { name: string } | null;
}

const DeleteTeacherPopup: React.FC<DeleteTeacherPopupProps> = ({
  isOpen,
  onClose,
  onConfirm,
  teacherData,
}) => {
  return (
    <AlertDialog open={isOpen} onOpenChange={onClose}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            Delete Teacher
          </AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the teacher{" "}
            <strong>{teacherData?.name}</strong>? This action cannot be undone
            and will permanently remove all associated data.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={onClose}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            Delete Teacher
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
};

export default DeleteTeacherPopup;
