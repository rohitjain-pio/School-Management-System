// components/EmptyState.tsx
import React from "react";
import { Button } from "@/components/ui/button"; // Adjust path if needed

interface EmptyStateProps {
  icon: React.ReactNode;
  title: string;
  description: string;
  buttonText: string;
  onClick: () => void;
}

const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  buttonText,
  onClick,
}) => {
  return (
    <div className="flex flex-col items-center justify-center text-center py-16 px-6 mt-12">
      <div className="bg-gray-200 rounded-full p-4 mb-4">
        {icon}
      </div>
      <h2 className="text-2xl sm:text-3xl font-bold text-primary-800 mb-2">
        {title}
      </h2>
      <p className="text-gray-600 text-base sm:text-lg max-w-md mb-6">
        {description}
      </p>
      <Button onClick={onClick} className="flex items-center space-x-2">
        <span>{buttonText}</span>
      </Button>
    </div>
  );
};

export default EmptyState;
