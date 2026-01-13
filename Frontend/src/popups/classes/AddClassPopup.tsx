import React, { useEffect, useState } from "react";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
const server_url = import.meta.env.VITE_API_URL;

interface AddClassPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (classData) => void;
}

const AddClassPopup: React.FC<AddClassPopupProps> = ({
  isOpen,
  onClose,
  onSubmit,
}) => {
  const [formData, setFormData] = useState({
    name: "",
    section: "",
    classTeacherId: "",
    // capacity: "",
    // room: "",
    // description: "",
  });
  const { toast } = useToast();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name || !formData.section) {
      toast({
        title: "Error",
        description: "Please fill in all required fields",
        variant: "destructive",
      });
      return;
    }

    onSubmit(formData);
    setFormData({
      name: "",
      section: "",
      classTeacherId: "",
      // capacity: "",
      // room: "",
      // description: "",
    });
    onClose();
  };

  const [teachers, setTeachers] = useState<any[]>([]);

  const fetchTeachers = async () => {
    try {
      const res = await fetch(`${server_url}/api/Teacher`, {
        credentials: "include",
      });
      if (!res.ok) {
        console.error("Failed to fetch teachers:", res.statusText);
        return;
      }
      const json = await res.json();
      if (!json.isSuccess) {
        console.error("API error:", json.errorMessage);
        return;
      }
      // Ensure we're setting an array
      const teachersData = Array.isArray(json.content) ? json.content : [];
      setTeachers(teachersData);
    } catch (error) {
      console.error("Error fetching teachers:", error);
      setTeachers([]);
    }
  };

  useEffect(() => {
    if (isOpen) {
      fetchTeachers();
    }
  }, [isOpen])

  const handleChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Add New Class</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="name">Class Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => handleChange("name", e.target.value)}
                placeholder="e.g., Grade 10"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="section">Section *</Label>
              <select
                id="section"
                value={formData.section}
                onChange={(e) => handleChange("section", e.target.value)}
                required
                className="w-full rounded border px-3 py-2"
              >
                <option value="">Select Section</option>
                {[...Array(26)].map((_, i) => {
                  const letter = String.fromCharCode(65 + i); // 65 = 'A'
                  return (
                    <option key={letter} value={letter}>
                      {letter}
                    </option>
                  );
                })}
              </select>

            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="classTeacherId">Class Teacher</Label>
            <Select
              onValueChange={(value) => handleChange("classTeacherId", value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select class teacher" />
              </SelectTrigger>
              <SelectContent>
                {teachers && teachers.length > 0 ? (
                  teachers.map((teacher) => (
                    <SelectItem key={teacher.id} value={teacher.id}>
                      {teacher.name}
                    </SelectItem>
                  ))
                ) : (
                  <SelectItem value="no-teachers" disabled>
                    No teachers available
                  </SelectItem>
                )}
              </SelectContent>
            </Select>
          </div>
          {/* <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="capacity">Capacity</Label>
              <Input
                id="capacity"
                type="number"
                value={formData.capacity}
                onChange={(e) => handleChange("capacity", e.target.value)}
                placeholder="30"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="room">Room</Label>
              <Input
                id="room"
                value={formData.room}
                onChange={(e) => handleChange("room", e.target.value)}
                placeholder="Room A-101"
              />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input
              id="description"
              value={formData.description}
              onChange={(e) => handleChange("description", e.target.value)}
              placeholder="Class description"
            />
          </div> */}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Add Class</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddClassPopup;
