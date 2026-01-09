import React, { useState } from "react";
import { X } from "lucide-react";
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

interface EditClassModalProps {
  classData: ClassItem;
  onClose: () => void;
  
  onSave: (updatedClass: ClassItem) => void;
}

const EditClassModal: React.FC<EditClassModalProps> = ({
  classData,
  onClose,
  onSave,
}) => {
  const [form, setForm] = useState({
    className: classData.className || "",
    classSection: classData.classSection || "",
    classTeacher: classData.classTeacher || "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = () => {
    if (!form.className.trim() || !form.classSection.trim()) {
      alert("Class Name and Section cannot be empty.");
      return;
    }

    onSave({
      ...classData,
      className: form.className.trim(),
      classSection: form.classSection.trim(),
      classTeacher: form.classTeacher.trim() || null,
    });
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center">
      <div className="bg-white rounded-lg w-full max-w-lg p-6 relative shadow-xl">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500 hover:text-gray-800"
          aria-label="Close Edit Class Modal"
        >
          <X className="w-5 h-5" />
        </button>
        <h2 className="text-xl font-semibold mb-4 text-primary-800">Edit Class</h2>
        <div className="space-y-4">
          <input
            name="className"
            value={form.className}
            onChange={handleChange}
            placeholder="Class Name"
            className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
          <input
            name="classSection"
            value={form.classSection}
            onChange={handleChange}
            placeholder="Section"
            className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
          <input
            name="classTeacher"
            value={form.classTeacher}
            onChange={handleChange}
            placeholder="Class Teacher"
            className="w-full border border-gray-300 p-2 rounded focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
          <Button className="w-full" onClick={handleSubmit}>
            Save Changes
          </Button>
        </div>
      </div>
    </div>
  );
};

export default EditClassModal;
