import React, { useState, useEffect } from "react";
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { useClasses } from "@/hooks/useClasses";

interface EditStudentPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (studentData: any) => void;
  studentData: {
    srNumber: string;
    firstName: string;
    lastName: string;
    email: string;
    rollNumber: string;
    classId: string;
    dob: string;
    gender: number;
  } | null;
}

const EditStudentPopup: React.FC<EditStudentPopupProps> = ({
  isOpen,
  onClose,
  onSubmit,
  studentData,
}) => {
  const { toast } = useToast();
  const { data: classes = [], isLoading: isClassLoading } = useClasses();

  const [formData, setFormData] = useState({
    srNumber:"",
    firstName: "",
    lastName: "",
    email: "",
    rollNumber: "",
    classId: "",
    dob: "",
    gender: null,
  });

  useEffect(() => {
    if (studentData) {
      setFormData({
        srNumber:studentData.srNumber || "",
        firstName: studentData.firstName || "",
        lastName: studentData.lastName || "",
        email: studentData.email || "",
        rollNumber: studentData.rollNumber || "",
        classId: studentData.classId || "",
        dob: studentData.dob || "",
        gender: studentData.gender || "",
      });
    }
  }, [studentData]);

  const handleChange = (field: keyof typeof formData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.firstName || !formData.lastName || !formData.email || !formData.gender || !formData.dob) {
      toast({
        title: "Error",
        description: "First Name, Last Name, and Email are required.",
        variant: "destructive",
      });
      return;
    }

    onSubmit({ ...formData, studentData });
    onClose();
  };
  const getGenderLabel = (gender: number) => {
    switch (gender) {
      case 0:
        return "Male";
      case 1:
        return "Female";
      case 2:
        return "Other";
      default:
        return "N/A";
    }
  };
  if (!studentData) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Edit Student</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-x-6 gap-y-4">
          <div className="space-y-2">
            <Label htmlFor="firstName">First Name *</Label>
            <Input
              id="firstName"
              value={formData.firstName}
              onChange={(e) => handleChange("firstName", e.target.value)}
              required
              placeholder="Student's first name"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="lastName">Last Name *</Label>
            <Input
              id="lastName"
              value={formData.lastName}
              onChange={(e) => handleChange("lastName", e.target.value)}
              required
              placeholder="Student's last name"
            />
          </div>

          <div className="space-y-2 col-span-2">
            <Label htmlFor="email">Email *</Label>
            <Input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) => handleChange("email", e.target.value)}
              required
              placeholder="student@example.com"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="rollNumber">Roll Number</Label>
            <Input
              id="rollNumber"
              value={formData.rollNumber}
              onChange={(e) => handleChange("rollNumber", e.target.value)}
              placeholder="Roll number"
            />
          </div>

          {/* Class dropdown */}
          <div className="space-y-2">
            <Label htmlFor="classId">Class</Label>
            <Select
              value={formData.classId}
              onValueChange={(value) => handleChange("classId", value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select class" />
              </SelectTrigger>
              <SelectContent>
                {classes.map((cls: any) => (
                  <SelectItem key={cls.id} value={cls.id}>
                    {cls.name} {cls.section}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="dob">Date of Birth</Label>
            <Input
              id="dob"
              type="date"
              value={formData.dob}
              onChange={(e) => handleChange("dob", e.target.value)}
            />
          </div>

          {/* Gender dropdown */}
          <div className="space-y-2">
            <Label htmlFor="gender">Gender</Label>
            <Select
              value={formData.gender}
              onValueChange={(value) => handleChange("gender", value)}
            >
              <SelectTrigger>
                <SelectValue placeholder={getGenderLabel(studentData.gender)}/>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Male">Male</SelectItem>
                <SelectItem value="Female">Female</SelectItem>
                <SelectItem value="Other">Other</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="col-span-2 flex justify-end gap-2 mt-4">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Save Changes</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default EditStudentPopup;
