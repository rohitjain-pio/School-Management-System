import { useAuth } from '@/context/AuthContext';

/**
 * Hook to check user roles
 * 
 * @example
 * const { hasRole, hasAnyRole, hasAllRoles, userRoles } = useRole();
 * 
 * if (hasRole('Admin')) {
 *   // Show admin features
 * }
 * 
 * if (hasAnyRole(['Admin', 'Teacher'])) {
 *   // Show features for Admin OR Teacher
 * }
 * 
 * if (hasAllRoles(['Admin', 'Teacher'])) {
 *   // Show features only if user has BOTH Admin AND Teacher roles
 * }
 */
export const useRole = () => {
  const { user, isAuthenticated } = useAuth();

  const userRoles = user?.roles || [];

  /**
   * Check if user has a specific role
   */
  const hasRole = (role: string): boolean => {
    return isAuthenticated && userRoles.includes(role);
  };

  /**
   * Check if user has ANY of the specified roles
   */
  const hasAnyRole = (roles: string[]): boolean => {
    return isAuthenticated && roles.some(role => userRoles.includes(role));
  };

  /**
   * Check if user has ALL of the specified roles
   */
  const hasAllRoles = (roles: string[]): boolean => {
    return isAuthenticated && roles.every(role => userRoles.includes(role));
  };

  /**
   * Check if user is Admin
   */
  const isAdmin = (): boolean => {
    return hasRole('Admin');
  };

  /**
   * Check if user is Teacher
   */
  const isTeacher = (): boolean => {
    return hasRole('Teacher');
  };

  /**
   * Check if user is Student
   */
  const isStudent = (): boolean => {
    return hasRole('Student');
  };

  /**
   * Check if user is Parent
   */
  const isParent = (): boolean => {
    return hasRole('Parent');
  };

  return {
    userRoles,
    hasRole,
    hasAnyRole,
    hasAllRoles,
    isAdmin,
    isTeacher,
    isStudent,
    isParent,
  };
};

export default useRole;
