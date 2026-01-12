const API_BASE_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:7266'}/api`;

export interface AnalyzeErrorRequest {
  category: string;
  message: string;
  stackTrace?: string;
  metadata?: Record<string, any>;
}

export interface AnalyzeErrorResponse {
  success: boolean;
  suggestion?: string;
  cached: boolean;
  tokensUsed: number;
  error?: string;
  message?: string;
  retryAfter?: number;
}

/**
 * Service for interacting with Gemini AI error analysis
 */
class GeminiService {
  /**
   * Analyze an error using Gemini AI
   */
  async analyzeError(request: AnalyzeErrorRequest): Promise<AnalyzeErrorResponse> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/Debug/analyze-error`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(request),
        }
      );

      const data = await response.json();

      // Handle rate limiting
      if (response.status === 429) {
        return {
          success: false,
          cached: false,
          tokensUsed: 0,
          error: 'Rate limit exceeded',
          message: data?.message || 'Too many requests. Please try again later.',
          retryAfter: data?.retryAfter,
        };
      }

      // Handle other errors
      if (!response.ok) {
        return {
          success: false,
          cached: false,
          tokensUsed: 0,
          error: data?.error || 'Unknown error',
          message: data?.message || 'Failed to analyze error. Please try again.',
        };
      }

      return data;
    } catch (error: any) {
      return {
        success: false,
        cached: false,
        tokensUsed: 0,
        error: error.message || 'Unknown error',
        message: 'Failed to analyze error. Please try again.',
      };
    }
  }

  /**
   * Test Gemini API connection
   */
  async testConnection(): Promise<{ success: boolean; message: string; model?: string }> {
    try {
      const response = await fetch(`${API_BASE_URL}/Debug/gemini/test`);
      const data = await response.json();

      if (!response.ok) {
        return {
          success: false,
          message: data?.message || 'Failed to connect to Gemini API',
        };
      }

      return data;
    } catch (error: any) {
      return {
        success: false,
        message: error.message || 'Failed to connect to Gemini API',
      };
    }
  }
}

export const geminiService = new GeminiService();
