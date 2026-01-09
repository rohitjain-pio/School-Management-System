import React, { useState, useEffect, useRef } from "react";
import { toast } from "sonner";

const server_url = import.meta.env.VITE_API_URL;

const roles = ["Admin", "SuperAdmin", "Teacher", "Student", "Parent"];

interface Props {
  onClose: () => void;
  onSwitch: () => void;
}

interface School {
  id: string;
  name: string;
}

const RegisterForm: React.FC<Props> = ({ onClose, onSwitch }) => {
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    password: "",
    role: "",
    schoolId: "",
  });

  const [formErrors, setFormErrors] = useState<{ [key: string]: string }>({});
  const [schoolSearch, setSchoolSearch] = useState("");
  const [showSchoolDropdown, setShowSchoolDropdown] = useState(false);
  const [schools, setSchools] = useState<School[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const debounceTimeout = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    if (!schoolSearch.trim()) {
      setSchools([]);
      setLoading(false);
      setError(null);
      return;
    }

    setLoading(true);
    setError(null);
    clearTimeout(debounceTimeout.current);

    debounceTimeout.current = setTimeout(() => {
      fetch(
        `${server_url}/api/School/search?schoolName=${encodeURIComponent(
          schoolSearch
        )}`
      )
        .then(async (res) => {
          if (!res.ok) throw new Error(res.statusText);
          const json = await res.json();
          if (!json.isSuccess)
            throw new Error(json.errorMessage || "Failed to fetch schools");
          setSchools(json.content || []);
          setLoading(false);
        })
        .catch((err) => {
          setError(err.message || "Unknown error");
          setSchools([]);
          setLoading(false);
        });
    }, 400);

    return () => {
      clearTimeout(debounceTimeout.current);
    };
  }, [schoolSearch]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSchoolSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSchoolSearch(e.target.value);
    setShowSchoolDropdown(true);
    setFormData((prev) => ({ ...prev, schoolId: "" }));
  };

  const handleSchoolSelect = (school: School) => {
    setFormData((prev) => ({ ...prev, schoolId: school.id }));
    setSchoolSearch(school.name);
    setShowSchoolDropdown(false);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validate form
    const errors: { [key: string]: string } = {};
    if (!formData.username.trim()) errors.username = "Username is required.";
    if (!formData.email.trim()) errors.email = "Email is required.";
    if (!formData.password.trim()) errors.password = "Password is required.";
    else if (formData.password.length < 6)
      errors.password = "Password must be at least 6 characters.";
    if (!formData.role) errors.role = "Please select a role.";
    if (!formData.schoolId) errors.schoolId = "Please select a school.";

    if (Object.keys(errors).length > 0) {
      setFormErrors(errors);
      return;
    }

    setFormErrors({});

    try {
      const payload = {
        userName: formData.username,
        email: formData.email,
        password: formData.password,
        role: formData.role,
        schoolId: formData.schoolId,
      };

      const res = await fetch(`${server_url}/api/Auth/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      const json = await res.json();

      if (!res.ok || !json.isSuccess) {
        throw new Error(json.errorMessage || "Registration failed.");
      }

      toast.success("🎉 Registration successful! You can now log in.");
      onSwitch(); // Show login form
    } catch (err: any) {
      toast.error(err.message || "Something went wrong. Please try again.");
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 relative px-4 sm:px-6 md:px-0 max-w-md mx-auto"
      autoComplete="off"
    >
      <h2 className="text-2xl font-bold mb-4 text-gray-800 text-center sm:text-left">
        Register
      </h2>

      {/* Username */}
      <input
        name="username"
        placeholder="Username"
        value={formData.username}
        onChange={handleChange}
        required
        className="w-full px-4 py-3 border border-gray-300 rounded-xl"
      />
      {formErrors.username && (
        <p className="text-sm text-red-600">{formErrors.username}</p>
      )}

      {/* Email */}
      <input
        name="email"
        type="email"
        placeholder="Email"
        value={formData.email}
        onChange={handleChange}
        required
        className="w-full px-4 py-3 border border-gray-300 rounded-xl"
      />
      {formErrors.email && (
        <p className="text-sm text-red-600">{formErrors.email}</p>
      )}

      {/* Password */}
      <input
        name="password"
        type="password"
        placeholder="Password"
        value={formData.password}
        onChange={handleChange}
        required
        className="w-full px-4 py-3 border border-gray-300 rounded-xl"
      />
      {formErrors.password && (
        <p className="text-sm text-red-600">{formErrors.password}</p>
      )}

      {/* Role */}
      <select
        name="role"
        value={formData.role}
        onChange={handleChange}
        required
        className="w-full px-4 py-3 border border-gray-300 rounded-xl"
      >
        <option value="">Select Role</option>
        {roles.map((r) => (
          <option key={r} value={r}>
            {r.charAt(0).toUpperCase() + r.slice(1)}
          </option>
        ))}
      </select>
      {formErrors.role && (
        <p className="text-sm text-red-600">{formErrors.role}</p>
      )}

      {/* School Search */}
      <div className="relative">
        <input
          type="text"
          placeholder="Search and select school"
          value={schoolSearch}
          onChange={handleSchoolSearchChange}
          onFocus={() => setShowSchoolDropdown(true)}
          onBlur={() => setTimeout(() => setShowSchoolDropdown(false), 150)}
          className="w-full px-4 py-3 border border-gray-300 rounded-xl"
          required
        />
        {formErrors.schoolId && (
          <p className="text-sm text-red-600">{formErrors.schoolId}</p>
        )}

        {showSchoolDropdown && (
          <div
            className="absolute z-10 w-full bg-white border border-gray-300 rounded-xl mt-1 shadow"
            onMouseDown={(e) => e.stopPropagation()} // prevent fall-through
          >
            {!schoolSearch.trim() && (
              <div className="px-4 py-3 text-gray-400">Type school name</div>
            )}

            {loading && (
              <div className="px-4 py-3 text-gray-500">Loading...</div>
            )}

            {error && (
              <div className="px-4 py-3 text-red-600 bg-red-50 rounded-xl">
                {error}
              </div>
            )}

            {!loading &&
              !error &&
              schoolSearch.trim() &&
              schools.length > 0 && (
                <ul className="max-h-48 overflow-auto">
                  {schools.map((school) => (
                    <li
                      key={school.id}
                      className="px-4 py-3 cursor-pointer hover:bg-primary-600 hover:text-white"
                      onMouseDown={(e) => {
                        e.preventDefault(); // prevent input blur
                        e.stopPropagation(); // stop bubbling to register button
                        handleSchoolSelect(school);
                      }}
                    >
                      {school.name}
                    </li>
                  ))}
                </ul>
              )}

            {!loading &&
              !error &&
              schoolSearch.trim() &&
              schools.length === 0 && (
                <div className="px-4 py-3 text-gray-500">No schools found</div>
              )}
          </div>
        )}
      </div>

      {/* Submit */}
      <button
        type="submit"
        className="w-full bg-gradient-to-r from-primary-600 to-primary-700 text-white py-3 rounded-xl font-bold hover:scale-105 transition"
      >
        Register
      </button>

      <p className="text-sm text-center text-gray-600 mt-4">
        Already have an account?{" "}
        <button
          type="button"
          onClick={onSwitch}
          className="text-primary-600 hover:underline font-medium"
        >
          Login
        </button>
      </p>
    </form>
  );
};

export default RegisterForm;
