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
  import { useClasses } from "@/hooks/useClasses";

  interface AddStudentPopupProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (studentData: StudentFormData) => void;
  }

  interface StudentFormData {
    srNumber: string;
    rollNumber: string;
    firstName: string;
    lastName: string;
    email: string;
    classId: string;
    dob: string;
    gender: string;
  }

  const AddStudentPopup: React.FC<AddStudentPopupProps> = ({
    isOpen,
    onClose,
    onSubmit,
  }) => {
    const { toast } = useToast();
    const { data: classes, isLoading, error } = useClasses();

    const [formData, setFormData] = useState<StudentFormData>({
      srNumber: "",
      rollNumber: "",
      firstName: "",
      lastName: "",
      email: "",
      classId: "",
      dob: "",
      gender: "",
    });

    const [formErrors, setFormErrors] = useState<Partial<StudentFormData>>({});

    const handleChange = (field: keyof StudentFormData, value: string) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setFormErrors((prev) => ({ ...prev, [field]: "" }));
    };

    const validate = () => {
      const errors: Partial<StudentFormData> = {};

      if (!formData.srNumber.trim()) errors.srNumber = "Sr No is required";
      if (!formData.rollNumber.trim()) errors.rollNumber = "Roll No is required";
      if (!formData.firstName.trim()) errors.firstName = "First name is required";
      if (!formData.lastName.trim()) errors.lastName = "Last name is required";
      if (!formData.email.trim()) {
        errors.email = "Email is required";
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
        errors.email = "Invalid email format";
      }
      if (!formData.classId) errors.classId = "Please select a class";
      if (!formData.gender) errors.gender = "Please select a gender";
      if (!formData.dob) errors.dob = "Date of Birth is required";

      return errors;
    };

    const handleSubmit = (e: React.FormEvent) => {
      e.preventDefault();
      const errors = validate();

      if (Object.keys(errors).length > 0) {
        setFormErrors(errors);
        toast({
          title: "Validation Error",
          description: "Please correct the highlighted fields.",
          variant: "destructive",
        });
        return;
      }

      onSubmit(formData);
      setFormData({
        srNumber: "",
        rollNumber: "",
        firstName: "",
        lastName: "",
        email: "",
        classId: "",
        dob: "",
        gender: "",
      });
      setFormErrors({});
      onClose();
    };

    return (
      <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent className="sm:max-w-[600px]">
          <DialogHeader>
            <DialogTitle>Add New Student</DialogTitle>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-y-4 pb-4">
            {/* Sr No */}
            <div className="space-y-1">
              <Label htmlFor="srNumber">Sr No *</Label>
              {formErrors.srNumber && (
                <p className="text-sm text-red-500">{formErrors.srNumber}</p>
              )}
              <Input
                id="srNumber"
                value={formData.srNumber}
                onChange={(e) => handleChange("srNumber", e.target.value)}
                placeholder="e.g., 2001"
              />
            </div>

            {/* Roll Number */}
            <div className="space-y-1">
              <Label htmlFor="rollNumber">Roll Number *</Label>
              {formErrors.rollNumber && (
                <p className="text-sm text-red-500">{formErrors.rollNumber}</p>
              )}
              <Input
                id="rollNumber"
                value={formData.rollNumber}
                onChange={(e) => handleChange("rollNumber", e.target.value)}
                placeholder="e.g., R2001"
              />
            </div>

            {/* Name Fields */}
            <div className="grid grid-cols-2 gap-x-6">
              <div className="space-y-1">
                <Label htmlFor="firstName">First Name *</Label>
                {formErrors.firstName && (
                  <p className="text-sm text-red-500">{formErrors.firstName}</p>
                )}
                <Input
                  id="firstName"
                  value={formData.firstName}
                  onChange={(e) => handleChange("firstName", e.target.value)}
                  placeholder="e.g., John"
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="lastName">Last Name *</Label>
                {formErrors.lastName && (
                  <p className="text-sm text-red-500">{formErrors.lastName}</p>
                )}
                <Input
                  id="lastName"
                  value={formData.lastName}
                  onChange={(e) => handleChange("lastName", e.target.value)}
                  placeholder="e.g., Doe"
                />
              </div>
            </div>

            {/* Email */}
            <div className="space-y-1">
              <Label htmlFor="email">Email *</Label>
              {formErrors.email && (
                <p className="text-sm text-red-500">{formErrors.email}</p>
              )}
              <Input
                id="email"
                type="email"
                value={formData.email}
                onChange={(e) => handleChange("email", e.target.value)}
                placeholder="e.g., john.doe@example.com"
              />
            </div>

            {/* Class & Gender */}
            <div className="grid grid-cols-2 gap-x-6">
              <div className="space-y-1">
                <Label htmlFor="classId">Class *</Label>
                {formErrors.classId && (
                  <p className="text-sm text-red-500">{formErrors.classId}</p>
                )}
                {isLoading ? (
                  <div>Loading classes...</div>
                ) : error ? (
                  <div className="text-red-500">Error loading classes</div>
                ) : (
                  <select
                    id="classId"
                    value={formData.classId}
                    onChange={(e) => handleChange("classId", e.target.value)}
                    className="w-full rounded-md border border-gray-300 px-3 py-2"
                  >
                    <option value="">Select a class</option>
                    {classes?.map((cls) => (
                      <option key={cls.id} value={cls.id}>
                        {cls.name} {cls.section}
                      </option>
                    ))}
                  </select>
                )}
              </div>
              {/* Gender */} 
              <div className="space-y-1">
                <Label htmlFor="gender">Gender *</Label>
                {formErrors.gender && (
                  <p className="text-sm text-red-500">{formErrors.gender}</p>
                )}
                <select
                  id="gender"
                  value={formData.gender}
                  onChange={(e) => handleChange("gender", e.target.value)}
                  className="w-full rounded-md border border-gray-300 px-3 py-2"
                >
                  <option value="">Select Gender</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </div>
            </div>

            {/* DOB */}
            <div className="space-y-1">
              <Label htmlFor="dob">Date of Birth *</Label>
              {formErrors.dob && (
                <p className="text-sm text-red-500">{formErrors.dob}</p>
              )}
              <Input
                id="dob"
                type="date"
                value={formData.dob}
                onChange={(e) => handleChange("dob", e.target.value)}
              />
            </div>

            {/* Actions */}
            <DialogFooter className="flex justify-end gap-2 mt-4">
              <Button type="button" variant="outline" onClick={onClose}>
                Cancel
              </Button>
              <Button type="submit">Add Student</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    );
  };

  export default AddStudentPopup;
    