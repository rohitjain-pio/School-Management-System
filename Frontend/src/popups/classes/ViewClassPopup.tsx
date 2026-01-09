import React, { useEffect, useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Users, UserCheck, Book, MapPin, Clock } from 'lucide-react';
const server_url = import.meta.env.VITE_API_URL;

interface ViewClassPopupProps {
  isOpen: boolean;
  onClose: () => void;
  classData: any;
}

const ViewClassPopup: React.FC<ViewClassPopupProps> = ({ isOpen, onClose, classData }) => {
const [students,setStudents] =  useState([]);

   const fetchStudents = async() =>{
    const res = await fetch(`${server_url}/api/Student`);
    if(!res.ok) throw new Error(res.statusText);
    const json = await res.json();
    if(!json.isSuccess) throw new Error(json.errorMessage);
    console.log("Students"+json.content);
    setStudents(json.content);
    return json;
  }

  useEffect(()=>{
    const timeout =  setTimeout(()=>fetchStudents(),1000);
    return () => clearTimeout(timeout);
  },[])

  if (!classData) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Book className="h-5 w-5" />
            {classData.name}{classData.section}
          </DialogTitle>
        </DialogHeader>
        <div className="space-y-6"> 
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <Users className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Class Teacher</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
                {classData.classTeacher.name || 'Not assigned'}
              </p>
            </div>
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <UserCheck className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Total Students</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
               {students.filter((student)=>student.classId === classData.id).length ?? "N/A"}
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <Users className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Assigned Teachers</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
                {classData.assignedTeacher?.length || 0}
              </p>
            </div>
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <UserCheck className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Attendance</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
                {classData.attendance || '0/0'}
              </p>
            </div>
          </div>

          {/* {classData.room && (
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <MapPin className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Room</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
                {classData.room}
              </p>
            </div>
          )}

          {classData.schedule && (
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">Schedule</span>
              </div>
              <p className="text-sm text-muted-foreground pl-6">
                {classData.schedule}
              </p>
            </div>
          )}

          {classData.description && (
            <div className="space-y-3">
              <span className="text-sm font-medium">Description</span>
              <p className="text-sm text-muted-foreground">
                {classData.description}
              </p>
            </div>
          )} */}

          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">Status:</span>
            <Badge variant={classData.status === 'Active' ? 'default' : 'secondary'}>
              {classData.status || 'Active'}
            </Badge>
          </div>
        </div>
        <DialogFooter>
          <Button onClick={onClose}>Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default ViewClassPopup;