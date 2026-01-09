import React, { useEffect, useState } from "react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button"; // Assuming you have a Button component
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"; // Assuming you have a Dialog component

interface Student {
  id: string;
  srNumber: string;
  rollNumber: number;
  email: string;
  firstName: string;
  lastName: string;
  dob: string;
  gender: string;
  classId: string;
}

interface AttendanceStatus {
  [studentId: string]: "Present" | "Absent";
}

const AttendanceTable: React.FC = () => {
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const location = useLocation();
  const { classId } = location.state || {};
  const server_url = import.meta.env.VITE_API_URL;

  // Modal open state
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Attendance states for students
  const [attendance, setAttendance] = useState<AttendanceStatus>({});

  // Fetch students
  const fetchData = async () => {
    try {
      setLoading(true);
      const res = await fetch(`${server_url}/api/Student/GetStudentByClassIdAsync/${classId}`);
      if (!res.ok) throw new Error(res.statusText);
      const json = await res.json();
      if (!json.isSuccess) throw new Error(json.errorMessage);
      setStudents(json.content);

      // Initialize attendance to "Absent" for all students when data loads
      const initialAttendance: AttendanceStatus = {};
      json.content.forEach((student: Student) => {
        initialAttendance[student.id] = "Absent";
      });
      setAttendance(initialAttendance);

      setLoading(false);
    } catch (err: any) {
      setError(err.message);
      setLoading(false);
    }
  };

  useEffect(() => {
    if (classId) {
      fetchData();
    }
  }, [classId]);

  if (loading) return <p>Loading students...</p>;
  if (error)
    return <p className="text-red-600">Error loading students: {error}</p>;

  // Handle attendance toggle
  const toggleAttendance = (studentId: string) => {
    setAttendance((prev) => ({
      ...prev,
      [studentId]: prev[studentId] === "Present" ? "Absent" : "Present",
    }));
  };

  // Handle save attendance
  const saveAttendance = () => {
    console.log("Attendance saved:", attendance);
    setIsModalOpen(false);
    // TODO: send attendance to backend here
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Attendance</h2>
          <p className="text-gray-600 mt-2">Records for this class</p>
        </div>

        <Button onClick={() => setIsModalOpen(true)}>Fill Attendance</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Attendance Table</CardTitle>
        </CardHeader>
        <CardContent>
          {students.length === 0 ? (
            <p className="text-center text-gray-500">No students found.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Sr. No.</TableHead>
                  <TableHead>Roll No.</TableHead>
                  <TableHead>Full Name</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {students.map((student, index) => (
                  <TableRow key={student.id}>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell>{student.rollNumber}</TableCell>
                    <TableCell className="font-medium text-gray-900">
                      {student.firstName} {student.lastName}
                    </TableCell>
                    <TableCell>
                      
                        <div>
                          17/07/2025
                        </div>
                      </TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${attendance[student.id] === "Present"
                            ? "bg-green-100 text-green-800"
                            : "bg-red-100 text-red-800"
                          }`}
                      >
                        {attendance[student.id]}
                      </span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Modal for filling attendance */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Fill Attendance</DialogTitle>
          </DialogHeader>
          <div className="space-y-2 max-h-96 overflow-auto">
            {students.map((student) => (
              <div
                key={student.id}
                className="flex justify-between items-center p-2 border rounded"
              >
                <div>
                  {student.firstName} {student.lastName}
                </div>
                <button
                  className={`px-3 py-1 rounded ${attendance[student.id] === "Present"
                      ? "bg-green-600 text-white"
                      : "bg-gray-300 text-gray-800"
                    }`}
                  onClick={() => toggleAttendance(student.id)}
                >
                  {attendance[student.id]}
                </button>
              </div>
            ))}
          </div>
          <DialogFooter className="mt-4 flex justify-end space-x-2">
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={saveAttendance}>Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default AttendanceTable;
