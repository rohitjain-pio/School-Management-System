import { ErrorEntry } from '../context/ErrorMonitorContext';

let errorCallback: ((error: Omit<ErrorEntry, 'id' | 'timestamp'>) => void) | null = null;

export const setFetchErrorCallback = (callback: (error: Omit<ErrorEntry, 'id' | 'timestamp'>) => void) => {
  errorCallback = callback;
};

const originalFetch = window.fetch;

export const setupFetchInterceptor = () => {
  if (import.meta.env.MODE !== 'development') return;

  window.fetch = async (...args) => {
    try {
      const response = await originalFetch(...args);
      const url = typeof args[0] === 'string' ? args[0] : args[0].url;

      // Don't log errors from the Debug endpoint to avoid recursion
      if (url.includes('/api/Debug/')) {
        return response;
      }

      // Log failed requests
      if (!response.ok && errorCallback) {
        const method = typeof args[1] === 'object' && args[1]?.method ? args[1].method : 'GET';
        
        let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        let category = 'Network';

        // Try to get error details from response
        try {
          const contentType = response.headers.get('content-type');
          if (contentType?.includes('application/json')) {
            const clonedResponse = response.clone();
            const errorData = await clonedResponse.json();
            if (errorData.error?.message) {
              errorMessage = errorData.error.message;
            } else if (errorData.message) {
              errorMessage = errorData.message;
            }
            
            // Categorize based on response
            if (response.status === 401 || response.status === 403) {
              category = 'Auth';
            } else if (response.status >= 400 && response.status < 500) {
              category = 'Validation';
            } else if (response.status >= 500) {
              category = 'Backend';
            }
          }
        } catch {
          // Ignore JSON parse errors
        }

        errorCallback({
          category,
          message: `${method} ${url}: ${errorMessage}`,
          source: url,
          metadata: {
            method,
            status: response.status,
            statusText: response.statusText,
          },
        });
      }

      return response;
    } catch (error) {
      // Network error (no response) - likely CORS or network failure
      const url = typeof args[0] === 'string' ? args[0] : args[0].url;
      
      // Don't log errors from the Debug endpoint to avoid recursion
      if (url.includes('/api/Debug/')) {
        throw error;
      }

      if (errorCallback) {
        const method = typeof args[1] === 'object' && args[1]?.method ? args[1].method : 'GET';
        const errorMessage = error instanceof Error ? error.message : 'Network request failed';
        
        // Check if it's a CORS error
        const isCorsError = errorMessage.includes('CORS') || 
                           errorMessage.includes('Network request failed') ||
                           errorMessage.includes('Failed to fetch');

        errorCallback({
          category: isCorsError ? 'Network' : 'Frontend',
          message: `${method} ${url}: ${errorMessage}${isCorsError ? ' (CORS or network issue)' : ''}`,
          stackTrace: error instanceof Error ? error.stack : undefined,
          source: url,
          metadata: {
            method,
            type: isCorsError ? 'CORS/Network' : 'Error',
          },
        });
      }

      throw error;
    }
  };

  return () => {
    window.fetch = originalFetch;
  };
};
