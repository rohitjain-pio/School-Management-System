// API Service for all authentication-related operations
const API_URL = import.meta.env.VITE_API_URL;

export interface LoginCredentials {
  userName: string;
  password: string;
}

export interface RegisterData {
  userName: string;
  email: string;
  password: string;
  role: string;
  schoolId: string;
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordData {
  email: string;
  token: string;
  newPassword: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
  schoolId?: string;
  roles: string[];
}

export interface ApiResponse<T = any> {
  message?: string;
  user?: T;
  isSuccess?: boolean;
  errorMessage?: string;
}

class AuthService {
  /**
   * Login user with credentials
   */
  async login(credentials: LoginCredentials): Promise<User> {
    const response = await fetch(`${API_URL}/api/Authentication/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Login failed' }));
      throw new Error(error.errorMessage || error.message || 'Invalid credentials');
    }

    const data: ApiResponse<User> = await response.json();
    
    // Return user from response or fetch it
    if (data.user) {
      return data.user;
    }
    
    // Fallback: fetch user info
    return await this.getCurrentUser();
  }

  /**
   * Register new user
   */
  async register(data: RegisterData): Promise<void> {
    const response = await fetch(`${API_URL}/api/Registration/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Registration failed' }));
      throw new Error(error.errorMessage || error.message || 'Registration failed');
    }
  }

  /**
   * Logout current user
   */
  async logout(): Promise<void> {
    try {
      await fetch(`${API_URL}/api/Authentication/logout`, {
        method: 'POST',
        credentials: 'include',
      });
    } catch (error) {
      console.warn('Logout request failed:', error);
    }
  }

  /**
   * Get current authenticated user
   */
  async getCurrentUser(): Promise<User> {
    const response = await fetch(`${API_URL}/api/Authentication/me`, {
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error('Not authenticated');
    }

    return await response.json();
  }

  /**
   * Refresh authentication token
   */
  async refreshToken(): Promise<void> {
    const response = await fetch(`${API_URL}/api/Token/refresh`, {
      method: 'POST',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error('Token refresh failed');
    }
  }

  /**
   * Request password reset email
   */
  async requestPasswordReset(email: string): Promise<void> {
    const response = await fetch(`${API_URL}/api/Password/request-reset`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Request failed' }));
      throw new Error(error.errorMessage || error.message || 'Failed to request password reset');
    }
  }

  /**
   * Reset password with token
   */
  async resetPassword(data: ResetPasswordData): Promise<void> {
    const response = await fetch(`${API_URL}/api/Password/reset`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Reset failed' }));
      throw new Error(error.errorMessage || error.message || 'Failed to reset password');
    }
  }

  /**
   * Change password for authenticated user
   */
  async changePassword(data: ChangePasswordData): Promise<void> {
    const response = await fetch(`${API_URL}/api/Password/change`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Change failed' }));
      throw new Error(error.errorMessage || error.message || 'Failed to change password');
    }
  }

  /**
   * Check if user is authenticated
   */
  async checkAuth(): Promise<User | null> {
    try {
      return await this.getCurrentUser();
    } catch {
      return null;
    }
  }
}

export const authService = new AuthService();
