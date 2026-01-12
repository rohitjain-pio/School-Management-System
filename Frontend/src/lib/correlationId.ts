/**
 * Correlation ID utilities for request tracking
 * Integrates with backend X-Correlation-ID header
 */

const CORRELATION_ID_HEADER = 'X-Correlation-ID';
const CORRELATION_ID_STORAGE_KEY = 'current-correlation-id';

/**
 * Generate a new correlation ID (UUID v4 format)
 */
export function generateCorrelationId(): string {
  return crypto.randomUUID();
}

/**
 * Get or create correlation ID for current request
 */
export function getCorrelationId(): string {
  let correlationId = sessionStorage.getItem(CORRELATION_ID_STORAGE_KEY);
  
  if (!correlationId) {
    correlationId = generateCorrelationId();
    sessionStorage.setItem(CORRELATION_ID_STORAGE_KEY, correlationId);
  }
  
  return correlationId;
}

/**
 * Set correlation ID (e.g., from response header)
 */
export function setCorrelationId(id: string): void {
  sessionStorage.setItem(CORRELATION_ID_STORAGE_KEY, id);
}

/**
 * Clear correlation ID (e.g., on page navigation)
 */
export function clearCorrelationId(): void {
  sessionStorage.removeItem(CORRELATION_ID_STORAGE_KEY);
}

/**
 * Enhanced fetch with correlation ID support and logging
 */
export async function fetchWithCorrelation(
  url: string,
  options: RequestInit = {}
): Promise<Response> {
  const correlationId = getCorrelationId();
  
  // Add correlation ID header
  const headers = new Headers(options.headers);
  headers.set(CORRELATION_ID_HEADER, correlationId);
  
  // Log request (development mode)
  if (import.meta.env.DEV) {
    console.log(`[API Request] ${options.method || 'GET'} ${url}`, {
      correlationId,
      timestamp: new Date().toISOString(),
    });
  }
  
  const startTime = performance.now();
  
  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });
    
    // Extract correlation ID from response
    const responseCorrelationId = response.headers.get(CORRELATION_ID_HEADER);
    if (responseCorrelationId) {
      setCorrelationId(responseCorrelationId);
    }
    
    const duration = performance.now() - startTime;
    
    // Log response (development mode)
    if (import.meta.env.DEV) {
      console.log(`[API Response] ${options.method || 'GET'} ${url}`, {
        status: response.status,
        correlationId: responseCorrelationId || correlationId,
        duration: `${duration.toFixed(2)}ms`,
        timestamp: new Date().toISOString(),
      });
    }
    
    // Log errors
    if (!response.ok) {
      console.error(`[API Error] ${options.method || 'GET'} ${url}`, {
        status: response.status,
        statusText: response.statusText,
        correlationId: responseCorrelationId || correlationId,
        duration: `${duration.toFixed(2)}ms`,
      });
    }
    
    return response;
  } catch (error) {
    const duration = performance.now() - startTime;
    
    console.error(`[API Network Error] ${options.method || 'GET'} ${url}`, {
      error: error instanceof Error ? error.message : 'Unknown error',
      correlationId,
      duration: `${duration.toFixed(2)}ms`,
    });
    
    throw error;
  }
}

/**
 * Get correlation ID from error context (for error reporting)
 */
export function getErrorContext() {
  return {
    correlationId: getCorrelationId(),
    timestamp: new Date().toISOString(),
    userAgent: navigator.userAgent,
    url: window.location.href,
  };
}
