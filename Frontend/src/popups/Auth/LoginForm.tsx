import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { useAuth } from "@/context/AuthContext";

interface Props {
  onClose: () => void;
  onSwitch: () => void;
  onForgotPassword?: () => void;
}

const LoginForm: React.FC<Props> = ({ onClose, onSwitch, onForgotPassword }) => {
  const [formData, setFormData] = useState({ username: "", password: "" });
  const [errors, setErrors] = useState<{
    username?: string;
    password?: string;
    general?: string;
  }>({});
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    setErrors({}); // clear errors on input change
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setLoading(true);

    // Basic form validation
    if (!formData.username.trim() || !formData.password) {
      setErrors({
        username: !formData.username.trim()
          ? "Username is required."
          : undefined,
        password: !formData.password ? "Password is required." : undefined,
      });
      setLoading(false);
      return;
    }

    try {
      await login(formData.username, formData.password);
      toast.success("✅ Logged in successfully!");
      onClose();
      navigate("/dashboard");
    } catch (err: any) {
      console.error(err);
      const errorMessage = err.message || "Invalid username or password.";
      toast.error(errorMessage);
      setErrors({ general: errorMessage });
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <h2 className="text-2xl font-bold mb-4 text-gray-800">Login</h2>

      {/* General error */}
      {errors.general && (
        <div className="text-sm text-red-600 bg-red-50 border border-red-200 p-2 rounded-md">
          {errors.general}
        </div>
      )}

      {/* Username */}
      <div className="space-y-1">
        {errors.username && (
          <p className="text-sm text-red-600">{errors.username}</p>
        )}
        <input
          name="username"
          value={formData.username}
          onChange={handleChange}
          required
          placeholder="Username"
          autoComplete="username"
          className={`w-full px-4 py-3 border rounded-xl ${
            errors.username ? "border-red-500" : "border-gray-300"
          }`}
        />
      </div>

      {/* Password */}
      <div className="space-y-1">
        {errors.password && (
          <p className="text-sm text-red-600">{errors.password}</p>
        )}
        <input
          name="password"
          type="password"
          value={formData.password}
          onChange={handleChange}
          required
          placeholder="Password"
          autoComplete="current-password"
          className={`w-full px-4 py-3 border rounded-xl ${
            errors.password ? "border-red-500" : "border-gray-300"
          }`}
        />
        
        {/* Forgot Password Link */}
        {onForgotPassword && (
          <div className="text-right">
            <button
              type="button"
              onClick={onForgotPassword}
              className="text-sm text-primary-600 hover:underline"
            >
              Forgot password?
            </button>
          </div>
        )}
      </div>

      {/* Submit */}
      <button
        type="submit"
        disabled={loading}
        className="w-full bg-gradient-to-r from-primary-600 to-primary-700 text-white py-3 rounded-xl font-bold hover:scale-105 transition disabled:opacity-60"
      >
        {loading ? "Logging in..." : "Login"}
      </button>

      {/* Switch to Register */}
      <p className="text-sm text-center text-gray-600 mt-4">
        Don't have an account?{" "}
        <button
          type="button"
          onClick={onSwitch}
          className="text-primary-600 hover:underline font-medium"
        >
          Register
        </button>
      </p>
    </form>
  );
};

export default LoginForm;
