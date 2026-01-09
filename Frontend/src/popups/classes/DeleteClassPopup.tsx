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

interface DeleteClassPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  classData;
}

const DeleteClassPopup: React.FC<DeleteClassPopupProps> = ({
  isOpen,
  onClose,
  onConfirm,
  classData,
}) => {
  return (
    <AlertDialog open={isOpen} onOpenChange={onClose}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            Delete Class
          </AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the class "{classData?.className}
            {classData?.classSection}"? This action cannot be undone and will
            permanently remove:
            <ul className="list-disc list-inside mt-2 space-y-1">
              <li>All class information</li>
              <li>Student assignments to this class</li>
              <li>Class schedules and timetables</li>
              <li>Associated attendance records</li>
            </ul>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={onClose}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            Delete Class
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
};

export default DeleteClassPopup;
