import { useState } from "react";
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import Navigation from "./components/Navigation";
import Footer from "./components/Footer";
import Modal from "./components/Modal";

import Home from "./pages/Home";
import About from "./pages/About";
import Pricing from "./pages/Pricing";
import Contact from "./pages/Contact";
import NotFound from "./pages/NotFound";
import Dashboard from "./pages/Dashboard";

import DashboardHome from "./pages/dashboard/DashboardHome";
import Classes from "./pages/dashboard/Classes";
import Teachers from "./pages/dashboard/Teachers";
import Students from "./pages/dashboard/Students";
import Schedule from "./pages/dashboard/Schedule";
import Reports from "./pages/dashboard/Reports";
import Settings from "./pages/dashboard/Settings";
import Announcements from "./pages/dashboard/Announcements";
import Payment from "./pages/dashboard/Payment";
import Leave from "./pages/dashboard/Leave";
import AttendanceTable from "./pages/dashboard/Attendance";

import ScrollToTop from "./components/ScrollToTop";
import LoginForm from "./popups/Auth/LoginForm";
import RegisterForm from "./popups/Auth/RegisterForm";
import ForgotPasswordForm from "./popups/Auth/ForgotPasswordForm";
import ResetPasswordForm from "./popups/Auth/ResetPasswordForm";
import ProtectedRoute from "./components/ProtectedRoute";
import { AuthProvider } from "./context/AuthContext";
import { ErrorMonitorProvider, useErrorMonitor } from "./context/ErrorMonitorContext";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { ErrorMonitorButton } from "./components/ErrorMonitorButton";
import { ErrorMonitorModal } from "./components/ErrorMonitorModal";
import { useErrorPolling } from "./hooks/useErrorPolling";
import UnauthorizedPage from "./pages/UnauthorizedPage";
import MeetingRooms from "./pages/dashboard/Meeting";
import ChatPage from "./pages/ChatPage";
import VideoCallPage from "./pages/VideoCallPage";

const queryClient = new QueryClient();

const App: React.FC = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState<"login" | "register" | "forgot" | null>(null);
  const [errorMonitorOpen, setErrorMonitorOpen] = useState(false);

  const openLogin = () => {
    setModalType("login");
    setModalOpen(true);
  };

  const openRegister = () => {
    setModalType("register");
    setModalOpen(true);
  };

  const openForgotPassword = () => {
    setModalType("forgot");
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalType(null);
    setModalOpen(false);
  };

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <ScrollToTop />
          <AuthProvider>
            <ErrorMonitorProvider>
              <ErrorBoundaryWithContext
                errorMonitorOpen={errorMonitorOpen}
                setErrorMonitorOpen={setErrorMonitorOpen}
              >
                <AppContent 
                  modalOpen={modalOpen}
                  modalType={modalType}
                  closeModal={closeModal}
                  openLogin={openLogin}
                  openRegister={openRegister}
                  openForgotPassword={openForgotPassword}
                  setModalType={setModalType}
                />
              </ErrorBoundaryWithContext>
            </ErrorMonitorProvider>
          </AuthProvider>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  );
};

interface AppContentProps {
  modalOpen: boolean;
  modalType: "login" | "register" | "forgot" | null;
  closeModal: () => void;
  openLogin: () => void;
  openRegister: () => void;
  openForgotPassword: () => void;
  setModalType: React.Dispatch<React.SetStateAction<"login" | "register" | "forgot" | null>>;
}

const AppContent: React.FC<AppContentProps> = ({
  modalOpen,
  modalType,
  closeModal,
  openLogin,
  openRegister,
  openForgotPassword,
  setModalType,
}) => {
  // Set up error polling
  useErrorPolling(5000);

  return (
    <>
            <Modal isOpen={modalOpen} onClose={closeModal} priority="low">
              {modalType === "register" && (
                <RegisterForm
                  onClose={closeModal}
                  onSwitch={() => setModalType("login")}
                />
              )}
              {modalType === "login" && (
                <LoginForm
                  onClose={closeModal}
                  onSwitch={() => setModalType("register")}
                  onForgotPassword={openForgotPassword}
                />
              )}
              {modalType === "forgot" && (
                <ForgotPasswordForm
                  onClose={closeModal}
                  onSuccess={() => {
                    closeModal();
                    // Optionally show success message
                  }}
                />
              )}
            </Modal>

            <Routes>
              {/* Public Routes */}
              <Route
                path="/"
                element={
                  <PageLayout
                    onLoginClick={openLogin}
                    onRegisterClick={openRegister}
                  >
                    <Home />
                  </PageLayout>
                }
              />
              <Route
                path="/about"
                element={
                  <PageLayout
                    onLoginClick={openLogin}
                    onRegisterClick={openRegister}
                  >
                    <About />
                  </PageLayout>
                }
              />
              <Route
                path="/pricing"
                element={
                  <PageLayout
                    onLoginClick={openLogin}
                    onRegisterClick={openRegister}
                  >
                    <Pricing />
                  </PageLayout>
                }
              />
              <Route
                path="/contact"
                element={
                  <PageLayout
                    onLoginClick={openLogin}
                    onRegisterClick={openRegister}
                  >
                    <Contact />
                  </PageLayout>
                }
              />

              {/* Protected Dashboard Routes - All authenticated users */}
              <Route element={<ProtectedRoute />}>
                <Route path="/dashboard" element={<Dashboard />}>
                  <Route index element={<DashboardHome />} />
                  <Route path="announcements" element={<Announcements />} />
                  <Route path="settings" element={<Settings />} />
                </Route>
                <Route path="/chat/:id" element={<ChatPage />} />
                <Route path="/video-call/:id" element={<VideoCallPage />} />
              </Route>

              {/* Admin/Principal/SchoolIncharge Routes */}
              <Route element={<ProtectedRoute allowedRoles={['Admin', 'SuperAdmin', 'Principal', 'SchoolIncharge']} />}>
                <Route path="/dashboard" element={<Dashboard />}>
                  <Route path="classes" element={<Classes />} />
                  <Route path="teachers" element={<Teachers />} />
                  <Route path="students" element={<Students />} />
                  <Route path="attendance" element={<AttendanceTable />} />
                  <Route path="reports" element={<Reports />} />
                  <Route path="payment" element={<Payment />} />
                </Route>
              </Route>

              {/* Teacher Routes - can view students, not CRUD */}
              <Route element={<ProtectedRoute allowedRoles={['Teacher']} />}>
                <Route path="/dashboard" element={<Dashboard />}>
                  <Route path="students" element={<Students />} />
                  <Route path="attendance" element={<AttendanceTable />} />
                  <Route path="schedule" element={<Schedule />} />
                </Route>
              </Route>

              {/* Meeting and Leave accessible to Teachers and Admins */}
              <Route element={<ProtectedRoute allowedRoles={['Admin', 'SuperAdmin', 'Principal', 'SchoolIncharge', 'Teacher']} />}>
                <Route path="/dashboard" element={<Dashboard />}>
                  <Route path="meeting" element={<MeetingRooms />} />
                  <Route path="leave" element={<Leave />} />
                </Route>
              </Route>

              {/* Password Reset Route (Public) */}
              <Route
                path="/reset-password"
                element={
                  <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-blue-50">
                    <ResetPasswordForm />
                  </div>
                }
              />

              {/* 404 */}
              <Route path="*" element={<NotFound />} />

              {/* Unauthorized Path */}
              <Route path="/unauthorized" element={<UnauthorizedPage />} />
            </Routes>
    </>
  );
};

interface PageLayoutProps {
  onLoginClick: () => void;
  onRegisterClick: () => void;
  children: React.ReactNode;
}

const PageLayout: React.FC<PageLayoutProps> = ({
  onLoginClick,
  onRegisterClick,
  children,
}) => (
  <div className="min-h-screen flex flex-col">
    <Navigation onLoginClick={onLoginClick} onRegisterClick={onRegisterClick} />
    <main className="flex-1">{children}</main>
    <Footer />
  </div>
);

// Wrapper to connect ErrorBoundary with ErrorMonitorContext
interface ErrorBoundaryWithContextProps {
  children: React.ReactNode;
  errorMonitorOpen: boolean;
  setErrorMonitorOpen: (open: boolean) => void;
}

const ErrorBoundaryWithContext: React.FC<ErrorBoundaryWithContextProps> = ({ 
  children, 
  errorMonitorOpen, 
  setErrorMonitorOpen 
}) => {
  const { addError } = useErrorMonitor();
  
  return (
    <ErrorBoundary addError={addError}>
      {children}
      <ErrorMonitorButton onClick={() => setErrorMonitorOpen(true)} />
      <ErrorMonitorModal isOpen={errorMonitorOpen} onClose={() => setErrorMonitorOpen(false)} />
    </ErrorBoundary>
  );
};

export default App;

// This is how we will upgrade the protected routes later on

// import ProtectedRoute from "@/components/ProtectedRoute";

// <Routes>
//   {/* Public Route */}
//   <Route path="/" element={<LoginPage />} />

//   {/* Protected Route for any authenticated user */}
//   <Route element={<ProtectedRoute />}>
//     <Route path="/dashboard" element={<Dashboard />} />
//   </Route>

//   {/* Protected Route for Admin and SuperAdmin only */}
//   <Route element={<ProtectedRoute allowedRoles={['Admin', 'SuperAdmin']} />}>
//     <Route path="/admin" element={<AdminPanel />} />
//   </Route>

//   {/* Protected Route for Teachers */}
//   <Route element={<ProtectedRoute allowedRoles={['Teacher']} />}>
//     <Route path="/teacher" element={<TeacherPanel />} />
//   </Route>

//   {/* Protected Route for Students and Parents */}
//   <Route element={<ProtectedRoute allowedRoles={['Student', 'Parent']} />}>
//     <Route path="/student" element={<StudentDashboard />} />
//     <Route path="/parent" element={<ParentDashboard />} />
//   </Route>

//   {/* Unauthorized Page */}
//   <Route path="/unauthorized" element={<UnauthorizedPage />} />
// </Routes>
