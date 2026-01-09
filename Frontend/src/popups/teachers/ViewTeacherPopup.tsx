import React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { User, Mail, Phone, MapPin, Calendar } from "lucide-react";

interface ViewTeacherPopupProps {
  isOpen: boolean;
  onClose: () => void;
  teacherData: {
    id: string;
    name: string;
    email: string;
    joiningDate: string;
    phone: string;
    address: string;
  } | null;
}

const ViewTeacherPopup: React.FC<ViewTeacherPopupProps> = ({
  isOpen,
  onClose,
  teacherData,
}) => {
  if (!teacherData) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-lg font-semibold">
            <User className="h-6 w-6 text-primary" />
            {teacherData.name}
          </DialogTitle>
        </DialogHeader>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-4 py-2">
          <div className="flex items-center gap-3">
            <Mail className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">Email</p>
              <p className="text-sm">{teacherData.email}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Phone className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">Phone</p>
              <p className="text-sm">{teacherData.phone || "N/A"}</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Calendar className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">Joining Date</p>
              <p className="text-sm">
                {teacherData.joiningDate
                  ? new Date(teacherData.joiningDate).toLocaleDateString(undefined, {
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })
                  : "N/A"}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <MapPin className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-xs font-semibold text-muted-foreground">Address</p>
              <p className="text-sm">{teacherData.address || "N/A"}</p>
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

export default ViewTeacherPopup;
