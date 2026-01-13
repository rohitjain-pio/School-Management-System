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

// Intercept console.error to capture all error logs
const originalConsoleError = console.error;

export const setupConsoleErrorInterceptor = () => {
  if (import.meta.env.MODE !== 'development') return;

  console.error = (...args: any[]) => {
    // Call original console.error
    originalConsoleError.apply(console, args);

    // Log to error monitor if callback is set
    if (errorCallback) {
      const message = args.map(arg => {
        if (arg instanceof Error) return arg.message;
        if (typeof arg === 'object') {
          try {
            return JSON.stringify(arg, null, 2);
          } catch {
            return String(arg);
          }
        }
        return String(arg);
      }).join(' ');

      const error = args.find(arg => arg instanceof Error);

      errorCallback({
        category: 'Frontend',
        message: message,
        stackTrace: error?.stack,
        source: 'console.error',
      });
    }
  };

  return () => {
    console.error = originalConsoleError;
  };
};

export const setupGlobalErrorHandlers = () => {
  if (import.meta.env.MODE !== 'development') return;

  window.addEventListener('error', handleGlobalError);
  window.addEventListener('unhandledrejection', handleUnhandledRejection);
  setupConsoleErrorInterceptor();

  return () => {
    window.removeEventListener('error', handleGlobalError);
    window.removeEventListener('unhandledrejection', handleUnhandledRejection);
    console.error = originalConsoleError;
  };
};
