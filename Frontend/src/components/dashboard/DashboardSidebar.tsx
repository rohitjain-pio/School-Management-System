
import React, { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Book,
  Calendar,
  Settings,
  DollarSign,
  FileText,
  LogOut,
  Menu,
  X,
  Bell,
  GraduationCap,
  PhoneCall

} from 'lucide-react';
import { useAuth } from '@/context/AuthContext';

interface NavItem {
  name: string;
  path: string;
  icon: React.ComponentType<{ className?: string }>;
  roles: string[];
  isUpcoming?: boolean;
}

const DashboardSidebar: React.FC = () => {
  const location = useLocation();
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const { logout } = useAuth();
  const navigate = useNavigate();

  const navItems: NavItem[] = [
    {
      name: 'Dashboard',
      path: '/dashboard',
      icon: LayoutDashboard,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students']
    },
    {
      name: 'Classes',
      path: '/dashboard/classes',
      icon: Book,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher']
    },
    {
      name: 'Teachers',
      path: '/dashboard/teachers',
      icon: Users,
      roles: ['Super Admin', 'School', 'Accounts']
    },
    {
      name: 'Students',
      path: '/dashboard/students',
      icon: Users,
      roles: ['Super Admin', 'School', 'Accounts']
    },
    {
      name: 'Announcements',
      path: '/dashboard/announcements',
      icon: Bell,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students']
    },
    {
      name: 'Meetings',
      path: '/dashboard/meeting',
      icon: PhoneCall,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students']
    },
    {
      name: 'Settings',
      path: '/dashboard/settings',
      icon: Settings,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students']
    },
    // Upcoming features at the end
    {
      name: 'Schedule',
      path: '/dashboard/schedule',
      icon: Calendar,
      roles: [],
      isUpcoming: true
    },
    {
      name: 'Reports',
      path: '/dashboard/reports',
      icon: FileText,
      roles: [],
      isUpcoming: true
    },
    {
      name: 'Payment',
      path: '/dashboard/payment',
      icon: DollarSign,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students'],
      isUpcoming: true
    },
    {
      name: 'Leave',
      path: '/dashboard/leave',
      icon: FileText,
      roles: ['Super Admin', 'School', 'Accounts', 'Teacher', 'Students'],
      isUpcoming: true
    },
  ];

  const isActive = (path: string): boolean => location.pathname === path;

  const toggleMobile = () => setIsMobileOpen(!isMobileOpen);

  return (
    <>
      {/* Mobile Menu Button */}
      <button
        onClick={toggleMobile}
        className="lg:hidden fixed top-4 left-4 z-50 p-2 bg-white rounded-lg shadow-lg"
      >
        {isMobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
      </button>

      {/* Mobile Overlay */}
      {isMobileOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-black bg-opacity-50 z-30"
          onClick={() => setIsMobileOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div className={`
        fixed lg:static inset-y-0 left-0 z-40 w-56 bg-white shadow-lg h-screen
        transform ${isMobileOpen ? 'translate-x-0' : '-translate-x-full'} 
        lg:translate-x-0 transition-transform duration-300 ease-in-out
        lg:sticky lg:top-0
      `}>
        <div className="p-4 border-b border-gray-200">
          <Link to="/" className="flex items-center space-x-2">
            <div className="w-7 h-7 bg-gradient-to-br from-primary-500 to-primary-700 rounded-lg flex items-center justify-center">
              <GraduationCap className="h-4 w-4 text-white" />
            </div>
            <span className="text-lg font-bold text-gray-800">SchoolSync</span>
          </Link>
        </div>

        <nav className="p-3 flex-1 overflow-y-auto">
          <ul className="space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <li key={item.name}>
                  <Link
                    to={item.path}
                    onClick={() => setIsMobileOpen(false)}
                    className={`flex items-center justify-between px-3 py-2 rounded-lg transition-colors duration-200 text-sm ${isActive(item.path)
                        ? 'bg-primary-50 text-primary-600 border-r-2 border-primary-600'
                        : item.isUpcoming
                          ? 'text-gray-400 cursor-not-allowed'
                          : 'text-gray-700 hover:bg-gray-50 hover:text-primary-600'
                      }`}
                  >
                    <div className="flex items-center space-x-2">
                      <Icon className="h-4 w-4" />
                      <span className="font-medium">{item.name}</span>
                    </div>
                    {item.isUpcoming && (
                      <span className="text-xs bg-red-500 text-white px-1.5 py-0.5 rounded-full">
                        soon
                      </span>
                    )}
                  </Link>
                </li>
              );
            })}

            <li className="border-t border-gray-200 pt-2 mt-3">
              <button
                onClick={async () => {
                  try {
                    await logout();
                    navigate("/");
                  } catch (err) {
                    console.error("Logout error:", err);
                    alert("Failed to log out. Please try again.");
                  }
                }}
                className="w-full flex items-center space-x-2 px-3 py-2 rounded-lg text-red-600 hover:bg-red-50 transition-colors duration-200 text-sm"
              >
                <LogOut className="h-4 w-4" />
                <span className="font-medium">Log out</span>
              </button>
            </li>
          </ul>
        </nav>
      </div>
    </>
  );
};

export default DashboardSidebar;
