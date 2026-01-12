import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { setErrorCallback } from '../utils/errorHandlers';
import { setFetchErrorCallback } from '../utils/fetchInterceptor';

export interface ErrorEntry {
  id: string;
  timestamp: string;
  category: string;
  message: string;
  stackTrace?: string;
  source?: string;
  metadata?: Record<string, unknown>;
}

interface ErrorMonitorContextType {
  errors: ErrorEntry[];
  addError: (error: Omit<ErrorEntry, 'id' | 'timestamp'>) => void;
  clearErrors: () => void;
  backendErrors: ErrorEntry[];
  fetchBackendErrors: () => Promise<void>;
  isPolling: boolean;
  togglePolling: () => void;
}

const ErrorMonitorContext = createContext<ErrorMonitorContextType | undefined>(undefined);

export const ErrorMonitorProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [errors, setErrors] = useState<ErrorEntry[]>([]);
  const [backendErrors, setBackendErrors] = useState<ErrorEntry[]>([]);
  const [isPolling, setIsPolling] = useState(false);

  const addError = useCallback((error: Omit<ErrorEntry, 'id' | 'timestamp'>) => {
    const newError: ErrorEntry = {
      ...error,
      id: crypto.randomUUID(),
      timestamp: new Date().toISOString(),
    };

    setErrors(prev => {
      const updated = [newError, ...prev];
      // Limit to 500 errors in memory
      return updated.slice(0, 500);
    });
  }, []);

  // Connect error handlers when in development mode
  useEffect(() => {
    if (import.meta.env.MODE === 'development') {
      setErrorCallback(addError);
      setFetchErrorCallback(addError);
    }
  }, [addError]);

  const clearErrors = useCallback(() => {
    setErrors([]);
    setBackendErrors([]);
  }, []);

  const fetchBackendErrors = useCallback(async () => {
    try {
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:7266';
      const response = await fetch(`${API_URL}/api/Debug/errors`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setBackendErrors(data);
      }
    } catch (error) {
      console.error('Failed to fetch backend errors:', error);
    }
  }, []);

  const togglePolling = useCallback(() => {
    setIsPolling(prev => !prev);
  }, []);

  return (
    <ErrorMonitorContext.Provider
      value={{
        errors,
        addError,
        clearErrors,
        backendErrors,
        fetchBackendErrors,
        isPolling,
        togglePolling,
      }}
    >
      {children}
    </ErrorMonitorContext.Provider>
  );
};

export const useErrorMonitor = () => {
  const context = useContext(ErrorMonitorContext);
  if (!context) {
    throw new Error('useErrorMonitor must be used within ErrorMonitorProvider');
  }
  return context;
};
