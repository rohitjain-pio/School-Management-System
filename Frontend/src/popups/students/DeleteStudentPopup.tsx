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

interface DeleteStudentPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  studentData: { firstName: string; lastName: string } | null;
}

const DeleteStudentPopup: React.FC<DeleteStudentPopupProps> = ({
  isOpen,
  onClose,
  onConfirm,
  studentData,
}) => {
  return (
    <AlertDialog open={isOpen} onOpenChange={onClose}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            Delete Student
          </AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the student{" "}
            <strong>
              {studentData?.firstName} {studentData?.lastName}
            </strong>
            ? This action cannot be undone and will permanently remove all associated data.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={onClose}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            Delete Student
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
};

export default DeleteStudentPopup;
