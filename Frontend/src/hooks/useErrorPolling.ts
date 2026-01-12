import { useEffect } from 'react';
import { useErrorMonitor } from '../context/ErrorMonitorContext';

export const useErrorPolling = (interval: number = 5000) => {
  const { fetchBackendErrors, isPolling } = useErrorMonitor();

  useEffect(() => {
    if (!isPolling || import.meta.env.MODE !== 'development') {
      return;
    }

    // Initial fetch
    fetchBackendErrors();

    // Set up polling
    const intervalId = setInterval(() => {
      fetchBackendErrors();
    }, interval);

    return () => {
      clearInterval(intervalId);
    };
  }, [isPolling, interval, fetchBackendErrors]);
};
