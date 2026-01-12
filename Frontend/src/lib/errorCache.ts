/**
 * Cache for Gemini AI error analysis suggestions
 * Stores suggestions in localStorage with 24-hour expiration
 */

interface CachedSuggestion {
  suggestion: string;
  timestamp: number;
  expiresAt: number;
}

const CACHE_PREFIX = 'gemini_suggestion_';
const CACHE_DURATION_MS = 24 * 60 * 60 * 1000; // 24 hours

class ErrorCache {
  /**
   * Generate a cache key from error details
   */
  private generateKey(category: string, message: string, stackTrace?: string): string {
    // Simple hash function for cache key
    const keySource = `${category}:${message}:${stackTrace?.substring(0, 100) || ''}`;
    let hash = 0;
    for (let i = 0; i < keySource.length; i++) {
      const char = keySource.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return `${CACHE_PREFIX}${category}_${Math.abs(hash)}`;
  }

  /**
   * Get cached suggestion if it exists and hasn't expired
   */
  getCachedSuggestion(category: string, message: string, stackTrace?: string): string | null {
    try {
      const key = this.generateKey(category, message, stackTrace);
      const cachedData = localStorage.getItem(key);

      if (!cachedData) {
        return null;
      }

      const cached: CachedSuggestion = JSON.parse(cachedData);

      // Check if expired
      if (Date.now() > cached.expiresAt) {
        localStorage.removeItem(key);
        return null;
      }

      return cached.suggestion;
    } catch (error) {
      console.error('Error reading from cache:', error);
      return null;
    }
  }

  /**
   * Cache a suggestion with 24-hour expiration
   */
  setCachedSuggestion(category: string, message: string, suggestion: string, stackTrace?: string): void {
    try {
      const key = this.generateKey(category, message, stackTrace);
      const now = Date.now();

      const cached: CachedSuggestion = {
        suggestion,
        timestamp: now,
        expiresAt: now + CACHE_DURATION_MS,
      };

      localStorage.setItem(key, JSON.stringify(cached));

      // Clean up old entries periodically
      this.cleanupExpiredEntries();
    } catch (error) {
      console.error('Error writing to cache:', error);
    }
  }

  /**
   * Clear all Gemini suggestion cache
   */
  clearCache(): void {
    try {
      const keys = Object.keys(localStorage);
      for (const key of keys) {
        if (key.startsWith(CACHE_PREFIX)) {
          localStorage.removeItem(key);
        }
      }
    } catch (error) {
      console.error('Error clearing cache:', error);
    }
  }

  /**
   * Remove expired cache entries
   */
  private cleanupExpiredEntries(): void {
    try {
      const keys = Object.keys(localStorage);
      const now = Date.now();

      for (const key of keys) {
        if (key.startsWith(CACHE_PREFIX)) {
          const cachedData = localStorage.getItem(key);
          if (cachedData) {
            try {
              const cached: CachedSuggestion = JSON.parse(cachedData);
              if (now > cached.expiresAt) {
                localStorage.removeItem(key);
              }
            } catch {
              // Invalid data, remove it
              localStorage.removeItem(key);
            }
          }
        }
      }
    } catch (error) {
      console.error('Error during cache cleanup:', error);
    }
  }

  /**
   * Get cache statistics
   */
  getCacheStats(): { totalEntries: number; validEntries: number; expiredEntries: number } {
    const keys = Object.keys(localStorage);
    const now = Date.now();
    let totalEntries = 0;
    let validEntries = 0;
    let expiredEntries = 0;

    for (const key of keys) {
      if (key.startsWith(CACHE_PREFIX)) {
        totalEntries++;
        const cachedData = localStorage.getItem(key);
        if (cachedData) {
          try {
            const cached: CachedSuggestion = JSON.parse(cachedData);
            if (now > cached.expiresAt) {
              expiredEntries++;
            } else {
              validEntries++;
            }
          } catch {
            expiredEntries++;
          }
        }
      }
    }

    return { totalEntries, validEntries, expiredEntries };
  }
}

export const errorCache = new ErrorCache();
