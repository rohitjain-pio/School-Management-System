import { ErrorEntry } from '../context/ErrorMonitorContext';

let errorCallback: ((error: Omit<ErrorEntry, 'id' | 'timestamp'>) => void) | null = null;

export const setErrorCallback = (callback: (error: Omit<ErrorEntry, 'id' | 'timestamp'>) => void) => {
  errorCallback = callback;
};

export const handleGlobalError = (event: ErrorEvent) => {
  if (!errorCallback || import.meta.env.MODE !== 'development') return;

  errorCallback({
    category: 'Frontend',
    message: event.message,
    stackTrace: event.error?.stack,
    source: event.filename,
    metadata: {
      lineno: event.lineno,
      colno: event.colno,
    },
  });
};

export const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
  if (!errorCallback || import.meta.env.MODE !== 'development') return;

  const error = event.reason;
  const message = error instanceof Error ? error.message : String(error);
  const stackTrace = error instanceof Error ? error.stack : undefined;

  errorCallback({
    category: 'Frontend',
    message: `Unhandled Promise Rejection: ${message}`,
    stackTrace,
    source: 'Promise',
  });
};

export const setupGlobalErrorHandlers = () => {
  if (import.meta.env.MODE !== 'development') return;

  window.addEventListener('error', handleGlobalError);
  window.addEventListener('unhandledrejection', handleUnhandledRejection);

  return () => {
    window.removeEventListener('error', handleGlobalError);
    window.removeEventListener('unhandledrejection', handleUnhandledRejection);
  };
};
