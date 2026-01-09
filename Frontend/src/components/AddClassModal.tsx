import { useState } from "react";
import { X } from "lucide-react";
import { Button } from "./ui/button";

interface AddClassModalProps {
  onClose: () => void;
  onSave: (data: {
    className: string;
    classSection: string;
    classTeacher: string | null;
    students: number;
    assignedTeacher: string[];
    attendance: string;
  }) => void;
  loading?: boolean;
  error?: string | null;
}

const AddClassModal:React.FC<AddClassModalProps> = ({ onClose, onSave, loading = false, error = null }) => {
  const [className, setClassName] = useState("");
  const [classSection, setClassSection] = useState("");

  const handleSubmit = () => {
    if (!className.trim() || !classSection.trim()) {
      alert("Please fill in both fields.");
      return;
    }

    const newClass = {
      className,
      classSection,
      classTeacher: null,
      students: 0,
      assignedTeacher: [],
      attendance: "0/0",
    };

    onSave(newClass);
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center">
      <div className="bg-white rounded-lg w-full max-w-lg p-6 relative shadow-xl">
        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500 hover:text-gray-800"
          aria-label="Close Add Class Modal"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Title */}
        <h2 className="text-xl font-semibold mb-4 text-primary-800">Add New Class</h2>

        {/* Error message */}
        {error && (
          <p className="text-sm text-red-600 mb-3">
            ⚠️ {error}
          </p>
        )}

        {/* Form Fields */}
        <div className="space-y-4">
          <input
            type="text"
            placeholder="Class Name (e.g., Class 11)"
            value={className}
            onChange={(e) => setClassName(e.target.value)}
            disabled={loading}
            className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:bg-gray-100"
          />
          <input
            type="text"
            placeholder="Section (e.g., A)"
            value={classSection}
            onChange={(e) => setClassSection(e.target.value)}
            disabled={loading}
            className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:bg-gray-100"
          />

          {/* Submit Button */}
          <Button className="w-full" onClick={handleSubmit} disabled={loading}>
            {loading ? "Saving..." : "Save Class"}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default AddClassModal;
