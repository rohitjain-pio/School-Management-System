import React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { User, Mail, Calendar, Hash, ClipboardList } from "lucide-react";

interface ViewStudentPopupProps {
  isOpen: boolean;
  onClose: () => void;
  studentData: {
    id: string;
    srNumber: string;
    rollNumber: number;
    email: string;
    firstName: string;
    lastName: string;
    dob: string;
    gender: number;
    classId: string;
    class: {
      id: string;
      name: string;
      section: string;
      classTeacherId: string;
      classTeacher: any; // Adjust type if known
      schoolId: string;
      school: any; // Adjust type if known
    };
    userId: string | null;
    user: any; // Adjust type if known
  } | null;
}

const ViewStudentPopup: React.FC<ViewStudentPopupProps> = ({
  isOpen,
  onClose,
  studentData,
}) => {
  if (!studentData) return null;

  // Map gender numeric value to string (adjust mapping based on backend convention)
  const getGenderLabel = (gender: number) => {
    switch (gender) {
      case 0:
        return "Male";
      case 1:
        return "Female";
      case 2:
        return "Other";
      default:
        return "N/A";
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-lg font-semibold">
            <User className="h-6 w-6 text-primary" />
            {studentData.firstName} {studentData.lastName}
          </DialogTitle>
        </DialogHeader>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4 py-2">
          <div className="flex items-center gap-3">
            <Mail className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                Email
              </p>
              <p className="text-sm">{studentData.email || "N/A"}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Hash className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                Roll Number
              </p>
              <p className="text-sm">{studentData.rollNumber || "N/A"}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <ClipboardList className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                Class
              </p>
              <p className="text-sm">
                {studentData.class
                  ? `${studentData.class.name} ${studentData.class.section}`
                  : "N/A"}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Calendar className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                Date of Birth
              </p>
              <p className="text-sm">
                {studentData.dob
                  ? new Date(studentData.dob).toLocaleDateString(undefined, {
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })
                  : "N/A"}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <User className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                Gender
              </p>
              <p className="text-sm">{getGenderLabel(studentData.gender)}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Hash className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">
                SR Number
              </p>
              <p className="text-sm">{studentData.srNumber || "N/A"}</p>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button onClick={onClose} variant="outline">
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default ViewStudentPopup;
