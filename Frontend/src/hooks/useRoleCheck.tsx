import { useAuth } from "@/context/AuthContext";

/**
 * Hook to check if current user has any of the specified roles
 * @param allowedRoles - Array of roles to check against
 * @returns boolean indicating if user has at least one of the allowed roles
 */
export const useRoleCheck = (allowedRoles: string[]): boolean => {
  const { user } = useAuth();

  if (!user || !user.roles || user.roles.length === 0) {
    return false;
  }

  return user.roles.some((role) => allowedRoles.includes(role));
};

/**
 * Hook to check if current user has a specific role
 * @param role - Single role to check
 * @returns boolean indicating if user has the role
 */
export const useHasRole = (role: string): boolean => {
  const { user } = useAuth();

  if (!user || !user.roles) {
    return false;
  }

  return user.roles.includes(role);
};

/**
 * Hook to get all user roles
 * @returns Array of user roles or empty array
 */
export const useUserRoles = (): string[] => {
  const { user } = useAuth();
  return user?.roles || [];
};

/**
 * Predefined role groups for convenience
 */
export const RoleGroups = {
  ADMIN_ROLES: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge"],
  STAFF_ROLES: ["Teacher", "Admin", "SuperAdmin", "Principal", "SchoolIncharge"],
  CAN_MANAGE_STUDENTS: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge"],
  CAN_VIEW_STUDENTS: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge", "Teacher"],
  CAN_MANAGE_TEACHERS: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge"],
  CAN_MANAGE_CLASSES: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge"],
  CAN_CREATE_ANNOUNCEMENTS: ["Admin", "SuperAdmin", "Principal", "SchoolIncharge", "Teacher"],
};
