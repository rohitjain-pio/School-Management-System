import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { Menu, X, GraduationCap, LogOut } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useRole } from "@/hooks/useRole";

interface NavItem {
  name: string;
  path: string;
  requiresAuth?: boolean;
  roles?: string[]; // If specified, only show for these roles
}

interface NavigationProps {
  onLoginClick?: () => void;
  onRegisterClick?: () => void;
}

const Navigation: React.FC<NavigationProps> = ({
  onLoginClick,
  onRegisterClick,
}) => {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const location = useLocation();

  const { isAuthenticated, logout, user } = useAuth();
  const { hasAnyRole } = useRole();

  const navItems: NavItem[] = [
    { name: "Home", path: "/" },
    { name: "About Us", path: "/about" },
    { name: "Pricing", path: "/pricing" },
    { name: "Contact", path: "/contact" },
    { 
      name: "Dashboard", 
      path: "/dashboard", 
      requiresAuth: true 
    },
  ];

  const isActive = (path: string): boolean => location.pathname === path;

  const handleLogout = async () => {
    try {
      await logout();
      setIsOpen(false);
    } catch (error) {
      console.error("Logout failed", error);
    }
  };

  // Filter nav items based on auth state and roles
  const filteredNavItems = navItems.filter(item => {
    // If item doesn't require auth, always show it
    if (!item.requiresAuth) return true;
    
    // If item requires auth but user is not authenticated, hide it
    if (item.requiresAuth && !isAuthenticated) return false;
    
    // If item has role restrictions, check if user has any of the required roles
    if (item.roles && item.roles.length > 0) {
      return hasAnyRole(item.roles);
    }
    
    // If no role restrictions but requires auth, show it
    return true;
  });

  return (
    <nav className="bg-white/95 backdrop-blur-md shadow-lg sticky top-0 z-50 border-b border-gray-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link
            to="/"
            className="flex items-center space-x-3 text-primary-600 hover:text-primary-700 transition-all duration-300 transform hover:scale-105"
          >
            <div className="w-10 h-10 bg-gradient-to-br from-primary-500 to-primary-700 rounded-xl flex items-center justify-center shadow-lg">
              <GraduationCap className="h-6 w-6 text-white" />
            </div>
            <span className="text-2xl font-bold text-primary-600  bg-clip-text">
              SchoolSync
            </span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-1">
            {filteredNavItems.map((item) => (
              <Link
                key={item.name}
                to={item.path}
                className={`relative px-4 py-2 text-sm font-medium rounded-lg transition-all duration-300 ${
                  isActive(item.path)
                    ? "text-primary-600 bg-primary-50"
                    : "text-gray-700 hover:text-primary-600 hover:bg-gray-50"
                }`}
              >
                {item.name}
                {isActive(item.path) && (
                  <span className="absolute bottom-0 left-1/2 transform -translate-x-1/2 w-6 h-1 bg-gradient-to-r from-primary-500 to-primary-700 rounded-full"></span>
                )}
              </Link>
            ))}

            {!isAuthenticated ? (
              <button
                onClick={onLoginClick}
                className="ml-4 bg-gradient-to-r from-primary-600 to-primary-700 text-white px-6 py-2.5 rounded-xl font-medium  transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-xl"
              >
                Login/Register
              </button>
            ) : (
              <div className="ml-4 flex items-center gap-3">
                <span className="text-sm text-gray-600">
                  Welcome, <span className="font-semibold text-primary-600">{user?.username}</span>
                </span>
                <button
                  onClick={handleLogout}
                  className="bg-red-600 text-white px-6 py-2.5 rounded-xl font-medium transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-xl flex items-center gap-2"
                >
                  <LogOut className="w-4 h-4" />
                  Logout
                </button>
              </div>
            )}
          </div>

          {/* Mobile menu button */}
          <div className="md:hidden">
            <button
              onClick={() => setIsOpen(!isOpen)}
              className="text-gray-700 hover:text-primary-600 transition-colors duration-300 p-2 rounded-lg hover:bg-gray-50"
            >
              {isOpen ? (
                <X className="h-6 w-6" />
              ) : (
                <Menu className="h-6 w-6" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile Navigation */}
        {isOpen && (
          <div className="md:hidden">
            <div className="px-2 pt-2 pb-3 space-y-1 bg-white/95 backdrop-blur-md border-t border-gray-100 shadow-lg">
              {isAuthenticated && user && (
                <div className="px-4 py-2 text-sm text-gray-600 border-b border-gray-200 mb-2">
                  Welcome, <span className="font-semibold text-primary-600">{user.username}</span>
                </div>
              )}
              
              {filteredNavItems.map((item) => (
                <Link
                  key={item.name}
                  to={item.path}
                  onClick={() => setIsOpen(false)}
                  className={`block px-4 py-3 rounded-xl text-base font-medium transition-all duration-300 ${
                    isActive(item.path)
                      ? "text-primary-600 bg-primary-50 border-l-4 border-primary-600"
                      : "text-gray-700 hover:text-primary-600 hover:bg-gray-50"
                  }`}
                >
                  {item.name}
                </Link>
              ))}

              {!isAuthenticated ? (
                <button
                  onClick={() => {
                    onLoginClick?.();
                    setIsOpen(false);
                  }}
                  className="block w-full text-center bg-gradient-to-r from-primary-600 to-primary-700 text-white px-4 py-3 rounded-xl font-medium hover:from-primary-700 hover:to-primary-800 transition-all duration-300 mt-4"
                >
                  Login/Register
                </button>
              ) : (
                <button
                  onClick={() => {
                    handleLogout();
                    setIsOpen(false);
                  }}
                  className="
                      block w-full text-center 
                      text-red-600 bg-white 
                      border-2 border-red-600 
                      px-4 py-3 rounded-xl font-medium 
                      flex items-center justify-center gap-2 
                      hover:bg-red-600 hover:text-white 
                      transition-all duration-300 mt-4
                    "
                >
                  <LogOut className="w-5 h-5" />
                  Logout
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navigation;
