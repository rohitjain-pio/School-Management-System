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
import { toast } from "@/hooks/use-toast";
const server_url = import.meta.env.VITE_API_URL;

interface AddAnnouncementPopupProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (announcementData: any) => void;
}

const AddAnnouncementPopup: React.FC<AddAnnouncementPopupProps> = ({
  isOpen,
  onClose,
  onSubmit,
}) => {
  const [schoolName, setSchoolName] = useState("");
  const [announcedBy, setAnnouncedBy] = useState("");
  const [schoolId, setSchoolId] = useState("");

  const [formData, setFormData] = useState({
    title: "",
    detail: "",
    date: new Date().toISOString().split("T")[0],
  });

  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const res = await fetch(`${server_url}/api/Authentication/me`, {
          credentials: "include",
          cache: "no-store", 
        });

        if (!res.ok) {
          throw new Error("Failed to fetch user info");
        }

        const userData = await res.json(); 
        console.log("Auth check:", userData);

        setAnnouncedBy(userData.username || userData.name);
        setSchoolId(userData.schoolId);  

        if (userData.schoolId) {
          const schoolRes = await fetch(`${server_url}/api/School/getbyId/${userData.schoolId}`, {
            cache: "no-store",
          });

          if (!schoolRes.ok) throw new Error("Failed to fetch school name");

          const schoolData = await schoolRes.json();
          setSchoolName(schoolData.content.name);
        }

      } catch (error) {
        console.error("Error:", error);
        setSchoolName("Unknown School");
      }
    };

    fetchUserInfo();
  }, []);



  const handleChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.title || !formData.detail || !formData.date) {
      toast({
        title: "Error",
        description: "Please fill in all required fields.",
        variant: "destructive",
      });
      return;
    }

    const announcementPayload = {
      ...formData,
      announcedBy,
      schoolId,
    };

    onSubmit(announcementPayload);

    // Reset form
    setFormData({
      title: "",
      detail: "",
      date: new Date().toISOString().split("T")[0],
    });

    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Add New Announcement</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="title">Title *</Label>
            <Input
              id="title"
              value={formData.title}
              onChange={(e) => handleChange("title", e.target.value)}
              placeholder="Enter announcement title"
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="detail">Detail *</Label>
            <Input
              id="detail"
              value={formData.detail}
              onChange={(e) => handleChange("detail", e.target.value)}
              placeholder="Enter announcement details"
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="date">Date *</Label>
            <Input
              id="date"
              type="date"
              min={new Date().toISOString().split("T")[0]}
              value={formData.date}
              onChange={(e) => handleChange("date", e.target.value)}
              required
            />


          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="announcedBy">Announced By</Label>
              <Input id="announcedBy" value={announcedBy} disabled />
            </div>
            <div className="space-y-2">
              <Label htmlFor="schoolId">School</Label>
              <Input id="schoolId" value={schoolName} disabled />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit">Add Announcement</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default AddAnnouncementPopup;
