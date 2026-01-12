import { useEffect, useRef, useCallback } from 'react';
import { authService } from '@/services/authService';
import { toast } from 'sonner';

/**
 * Hook to automatically refresh JWT tokens before they expire
 * Refreshes token every 2.5 hours (tokens expire in 3 hours)
 */
export function useTokenRefresh(isAuthenticated: boolean) {
  const refreshIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const isRefreshingRef = useRef(false);

  const refreshToken = useCallback(async () => {
    // Prevent concurrent refresh attempts
    if (isRefreshingRef.current) {
      return;
    }

    isRefreshingRef.current = true;

    try {
      await authService.refreshToken();
      console.log('Token refreshed successfully');
    } catch (error) {
      console.error('Token refresh failed:', error);
      // Don't show error toast as this happens in background
      // The user will be redirected to login when they try to make a request
    } finally {
      isRefreshingRef.current = false;
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      // Clear interval when user logs out
      if (refreshIntervalRef.current) {
        clearInterval(refreshIntervalRef.current);
        refreshIntervalRef.current = null;
      }
      return;
    }

    // Refresh token every 2.5 hours (150 minutes)
    // Access tokens expire in 3 hours, so this gives us a 30-minute buffer
    const REFRESH_INTERVAL = 150 * 60 * 1000; // 2.5 hours in milliseconds

    // Start the refresh interval
    refreshIntervalRef.current = setInterval(() => {
      refreshToken();
    }, REFRESH_INTERVAL);

    // Cleanup on unmount
    return () => {
      if (refreshIntervalRef.current) {
        clearInterval(refreshIntervalRef.current);
      }
    };
  }, [isAuthenticated, refreshToken]);

  return { refreshToken };
}

/**
 * Hook to handle session timeout warning
 * Shows warning 5 minutes before session expires
 */
export function useSessionTimeout(isAuthenticated: boolean, onTimeout?: () => void) {
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const warningRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (!isAuthenticated) {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
      if (warningRef.current) clearTimeout(warningRef.current);
      return;
    }

    // Session expires in 3 hours
    const SESSION_DURATION = 3 * 60 * 60 * 1000; // 3 hours
    const WARNING_TIME = 5 * 60 * 1000; // 5 minutes before expiry

    // Show warning 5 minutes before session expires
    warningRef.current = setTimeout(() => {
      toast.warning('Your session will expire in 5 minutes. Please save your work.', {
        duration: 10000,
      });
    }, SESSION_DURATION - WARNING_TIME);

    // Execute timeout callback when session expires
    timeoutRef.current = setTimeout(() => {
      toast.error('Your session has expired. Please login again.');
      onTimeout?.();
    }, SESSION_DURATION);

    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
      if (warningRef.current) clearTimeout(warningRef.current);
    };
  }, [isAuthenticated, onTimeout]);
}
