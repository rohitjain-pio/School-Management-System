import { ReactNode } from 'react';
import { useAuth } from '@/context/AuthContext';

interface RoleGateProps {
  roles: string | string[];
  children: ReactNode;
  fallback?: ReactNode;
  requireAll?: boolean; // If true, requires all roles; if false (default), requires any role
}

/**
 * Component that conditionally renders children based on user roles
 * 
 * @example
 * // Render only for Admin
 * <RoleGate roles="Admin">
 *   <AdminPanel />
 * </RoleGate>
 * 
 * @example
 * // Render for Admin or Teacher
 * <RoleGate roles={["Admin", "Teacher"]}>
 *   <GradingPanel />
 * </RoleGate>
 * 
 * @example
 * // Render for Admin AND Teacher (must have both roles)
 * <RoleGate roles={["Admin", "Teacher"]} requireAll>
 *   <SupervisorPanel />
 * </RoleGate>
 * 
 * @example
 * // Show fallback if not authorized
 * <RoleGate roles="Admin" fallback={<p>Access Denied</p>}>
 *   <AdminPanel />
 * </RoleGate>
 */
export const RoleGate: React.FC<RoleGateProps> = ({ 
  roles, 
  children, 
  fallback = null,
  requireAll = false 
}) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user) {
    return <>{fallback}</>;
  }

  const rolesToCheck = Array.isArray(roles) ? roles : [roles];
  const userRoles = user.roles || [];

  const hasAccess = requireAll
    ? rolesToCheck.every(role => userRoles.includes(role))
    : rolesToCheck.some(role => userRoles.includes(role));

  return hasAccess ? <>{children}</> : <>{fallback}</>;
};

export default RoleGate;
