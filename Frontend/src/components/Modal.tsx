import React from "react";
import { X } from "lucide-react"; // Optional icon

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
  priority?: 'low' | 'normal' | 'high'; // low=z-40, normal=z-50, high=z-60
}

const Modal: React.FC<ModalProps> = ({ isOpen, onClose, children, priority = 'normal' }) => {
  if (!isOpen) return null;

  const zIndexClass = priority === 'high' ? 'z-60' : priority === 'low' ? 'z-40' : 'z-50';

  return (
    <div 
      className={`fixed inset-0 ${zIndexClass} flex items-center justify-center bg-black bg-opacity-50`}
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-2xl shadow-2xl w-full max-w-lg p-8 relative animate-fadeIn"
        onClick={(e) => e.stopPropagation()}
      >
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-500 hover:text-gray-800">
          <X className="w-5 h-5" />
        </button>
        {children}
      </div>
    </div>
  );
};

export default Modal;
