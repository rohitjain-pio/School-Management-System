import React, { useMemo } from 'react';
import { useErrorMonitor } from '../context/ErrorMonitorContext';

interface ErrorMonitorButtonProps {
  onClick: () => void;
}

export const ErrorMonitorButton: React.FC<ErrorMonitorButtonProps> = ({ onClick }) => {
  const { errors, backendErrors } = useErrorMonitor();

  const totalErrors = useMemo(() => {
    return errors.length + backendErrors.length;
  }, [errors.length, backendErrors.length]);

  // Only show in development
  if (import.meta.env.MODE !== 'development') {
    return null;
  }

  return (
    <button
      onClick={onClick}
      className="fixed bottom-6 right-6 bg-red-600 text-white rounded-full w-14 h-14 flex items-center justify-center shadow-lg hover:bg-red-700 transition-all hover:scale-110 z-50 group"
      title="Error Monitor (Development Only)"
    >
      <svg
        className="w-6 h-6"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
        />
      </svg>
      {totalErrors > 0 && (
        <span className="absolute -top-1 -right-1 bg-yellow-400 text-gray-900 text-xs font-bold rounded-full w-6 h-6 flex items-center justify-center">
          {totalErrors > 99 ? '99+' : totalErrors}
        </span>
      )}
      <span className="absolute bottom-16 right-0 bg-gray-900 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 transition-opacity">
        Error Monitor
      </span>
    </button>
  );
};
