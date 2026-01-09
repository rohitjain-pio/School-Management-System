import React, { useState } from "react";
import { X, Trash2 } from "lucide-react";
import { Button } from "./ui/button";

interface ClassItem {
  id: string;
  className: string;
  classSection: string;
}

interface DeleteClassModalProps {
  classData: ClassItem;
  onClose: () => void;
  onDelete: (id: string) => Promise<void>; 
  loading?: boolean;
  error?: string | null;
}

const DeleteClassModal: React.FC<DeleteClassModalProps> = ({
  classData,
  onClose,
  onDelete,
  loading,
  error = null,
}) => {
  const handleDelete = async () => {
    await onDelete(classData.id);
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/30 flex items-center justify-center">
      <div className="bg-white rounded-lg w-full max-w-sm p-6 relative shadow-xl">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500 hover:text-gray-800"
          aria-label="Close Delete Class Modal"
        >
          <X className="w-5 h-5" />
        </button>
        <div className="flex items-center space-x-3 mb-4">
          <Trash2 className="w-6 h-6 text-red-600" />
          <h2 className="text-xl font-semibold text-red-700">Delete Class</h2>
        </div>
        <p className="mb-4 text-gray-700">
          Are you sure you want to delete the class <strong>{classData.className} {classData.classSection}</strong>? This action cannot be undone.
        </p>

        {error && (
          <p className="text-sm text-red-600 mb-4">
            ⚠️ {error}
          </p>
        )}

        <div className="flex gap-3">
          <Button variant="outline" onClick={onClose} disabled={loading} className="flex-1">
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleDelete}
            disabled={loading}
            className="flex-1"
          >
            {loading ? "Deleting..." : "Delete"}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default DeleteClassModal;
