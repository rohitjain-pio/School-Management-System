import React from "react";
import { X, Book, Users, UserCheck } from "lucide-react";
import { Button } from "./ui/button";

interface ClassItem {
  id: string;
  className: string;
  classSection: string;
  classTeacher: string | null;
  students?: number;
  assignedTeacher?: string[];
  attendance?: string;
}

interface ViewClassModalProps {
  classData: ClassItem;
  onClose: () => void;
}

const ViewClassModal: React.FC<ViewClassModalProps> = ({ classData, onClose }) => {
  return (
    <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center">
      <div className="bg-white rounded-lg w-full max-w-md p-6 relative shadow-xl">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500 hover:text-gray-800"
          aria-label="Close View Class Modal"
        >
          <X className="w-5 h-5" />
        </button>
        <h2 className="text-xl font-semibold mb-4 text-primary-800">Class Details</h2>

        <div className="space-y-3 text-gray-700">
          <div className="flex items-center space-x-2">
            <Book className="w-5 h-5 text-primary-600" />
            <span className="font-medium text-lg">{classData.className} {classData.classSection}</span>
          </div>

          <div>
            <strong>Class Teacher:</strong> {classData.classTeacher || "Not Assigned"}
          </div>

          <div className="flex items-center space-x-2">
            <Users className="w-4 h-4 text-gray-500" />
            <span>
              Total Students: {classData.students ?? 0}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <Users className="w-4 h-4 text-gray-500" />
            <span>
              Assigned Teachers: {classData.assignedTeacher?.length ?? 0}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <UserCheck className="w-4 h-4 text-green-500" />
            <span>Attendance: {classData.attendance ?? "N/A"}</span>
          </div>
        </div>

        <Button onClick={onClose} className="mt-6 w-full">Close</Button>
      </div>
    </div>
  );
};

export default ViewClassModal;
