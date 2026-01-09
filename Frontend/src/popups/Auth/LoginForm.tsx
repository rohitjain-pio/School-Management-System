import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { useAuth } from "@/context/AuthContext";

const server_url = import.meta.env.VITE_API_URL;

interface Props {
  onClose: () => void;
  onSwitch: () => void;
}

const LoginForm: React.FC<Props> = ({ onClose, onSwitch }) => {
  const [formData, setFormData] = useState({ username: "", password: "" });
  const [errors, setErrors] = useState<{
    username?: string;
    password?: string;
    general?: string;
  }>({});
  const [loading, setLoading] = useState(false);

  const { setIsAuthenticated, setUser } = useAuth();
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
      const payload = {
        username: formData.username,
        password: formData.password,
      };

      const res = await fetch(`${server_url}/api/Auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      // Handle both JSON and text responses
      let json;
      const contentType = res.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        json = await res.json();
      } else {
        const text = await res.text();
        json = { message: text };
      }

      // Safer check for success
      const success =
        res.ok &&
        (json.isSuccess === undefined ||
          json.isSuccess === true ||
          (typeof json.message === "string" &&
            json.message.toLowerCase().includes("success")));

      if (!success) {
        const errorMessage =
          json?.errorMessage ||
          json?.message ||
          "Invalid username or password.";
        setErrors({
          general: errorMessage,
        });
        toast.error(errorMessage);
        setLoading(false);
        return;
      }

      // Fetch user info after successful login
      const meRes = await fetch(`${server_url}/api/Auth/me`, {
        method: "GET",
        credentials: "include",
      });

      if (!meRes.ok) {
        throw new Error("Authentication verification failed after login.");
      }

      const meData = await meRes.json();
      console.log("Authenticated user:", meData);

      setUser(meData);
      setIsAuthenticated(true);

      toast.success("✅ Logged in successfully!");
      onClose();

      navigate("/dashboard");
    } catch (err: any) {
      console.error(err);
      toast.error(err.message || "Something went wrong.");
      setErrors({ general: err.message || "Something went wrong." });
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
