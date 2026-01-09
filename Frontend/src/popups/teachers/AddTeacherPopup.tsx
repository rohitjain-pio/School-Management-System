import React, { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useToast } from "@/hooks/use-toast";


interface AddTeacherPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (teacherData: TeacherFormData) => void;
}

interface TeacherFormData {
  name: string;
  email: string;
  phone: string;
  address: string;
  joiningDate: string;
  gender: string;
}

const AddTeacherPopup: React.FC<AddTeacherPopupProps> = ({
  isOpen,
  onClose,
  onSubmit,
}) => {
  const {toast} = useToast();
  const [formData, setFormData] = useState<TeacherFormData>({
    name: "",
    email: "",
    phone: "",
    address: "",
    joiningDate: "",
    gender: ""
  });

  const [errors, setErrors] = useState<Partial<TeacherFormData>>({});

  const handleChange = (field: keyof TeacherFormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: "" })); // clear error on change
  };

  const validate = () => {
    const newErrors: Partial<TeacherFormData> = {};

    if (!formData.name.trim()) newErrors.name = "Name is required";
    if (!formData.email.trim()) {
      newErrors.email = "Email is required";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = "Email is not valid";
    }

    if (!formData.phone.trim()) {
      newErrors.phone = "Phone number is required";
    } else if (!/^\d{10}$/.test(formData.phone)) {
      newErrors.phone = "Phone must be 10 digits";
    }

    if (!formData.gender) {
      newErrors.gender = "Gender  is required";
    }

    return newErrors;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const validationErrors = validate();

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      toast({
          title: "Validation Error",
          description: "Please correct the highlighted fields.",
          variant: "destructive",
        });
      return;
    }

    onSubmit(formData);
    setFormData({
      name: "",
      email: "",
      phone: "",
      address: "",
      joiningDate: "",
      gender: ""
    });
    setErrors({});
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add New Teacher</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Name */}
          <div className="space-y-1">
            <Label htmlFor="name">Name *</Label>
            {errors.name && (
              <p className="text-sm text-red-500 -mb-2">{errors.name}</p>
            )}
            <Input
              id="name"
              value={formData.name}
              onChange={(e) => handleChange("name", e.target.value)}
              placeholder="e.g., John Doe"
            />
          </div>

          {/* Email */}
          <div className="space-y-1">
            <Label htmlFor="email">Email *</Label>
            {errors.email && (
              <p className="text-sm text-red-500 -mb-2">{errors.email}</p>
            )}
            <Input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) => handleChange("email", e.target.value)}
              placeholder="e.g., john@example.com"
            />
          </div>

          {/* Phone */}
          <div className="space-y-1">
            <Label htmlFor="phone">Phone *</Label>
            {errors.phone && (
              <p className="text-sm text-red-500 -mb-2">{errors.phone}</p>
            )}
            <Input
              id="phone"
              value={formData.phone}
              onChange={(e) => handleChange("phone", e.target.value)}
              placeholder="e.g., 1234567890"
            />
          </div>

          {/* Address */}
          <div className="space-y-1">
            <Label htmlFor="address">Address</Label>
            <Input
              id="address"
              value={formData.address}
              onChange={(e) => handleChange("address", e.target.value)}
              placeholder="e.g., 123 Main St"
            />
          </div>

          {/* Gender */}
          <div className="space-y-1">
            <Label htmlFor="gender">Gender *</Label>
            {errors.gender && (
              <p className="text-sm text-red-500 -mb-2">{errors.gender}</p>
            )}
            <select
              id="gender"
              value={formData.gender.toString()} // to match select's string-based value
              onChange={(e) => handleChange("gender",   (e.target.value))}
              className="w-full rounded-md border border-gray-300 px-3 py-2"
            >
              <option value="">Select Gender</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="Other">Other</option>
            </select>

          </div>

          {/* Joining D>ate */}
          <div className="space-y-1">
            <Label htmlFor="joiningDate">Joining Date</Label>
            <Input
              id="joiningDate"
              type="date"
              value={formData.joiningDate}
              onChange={(e) => handleChange("joiningDate", e.target.value)}
            />
          </div>

          <DialogFooter className="pt-2">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Add Teacher</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddTeacherPopup;
