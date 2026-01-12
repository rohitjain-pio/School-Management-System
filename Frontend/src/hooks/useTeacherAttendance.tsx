import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

const server_url = import.meta.env.VITE_API_URL;

// Get all teacher attendance
const getAllTeacherAttendance = async () => {
  const res = await fetch(`${server_url}/api/TeacherAttendance/GetTeacherAttendance`, {
    credentials: "include",
  });
  const data = await res.json();
  return data.content;
};

// Create teacher attendance
const addNewTeacherAttendance = async (newAttendance: any) => {
  const res = await fetch(`${server_url}/api/TeacherAttendance/createTeacherAttendance`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(newAttendance),
  });
  const data = await res.json();
  return data.content;
};

// Update teacher attendance
const editTeacherAttendance = async (updatedAttendance: any) => {
  const res = await fetch(`${server_url}/api/TeacherAttendance/${updatedAttendance.teacherId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(updatedAttendance),
  });
  const data = await res.json();
  return data.content;
};

// Delete teacher attendance
const deleteTeacherAttendance = async (attendanceId: string) => {
  const res = await fetch(`${server_url}/api/TeacherAttendance/${attendanceId}`, {
    method: "DELETE",
    credentials: "include",
  });
  const data = await res.json();
  return data.content;
};

// Hook
export function useTeacherAttendance() {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["teacherAttendance"],
    queryFn: getAllTeacherAttendance,
  });

  const addAttendance = useMutation({
    mutationFn: addNewTeacherAttendance,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacherAttendance"] });
    },
  });

  const editAttendance = useMutation({
    mutationFn: editTeacherAttendance,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacherAttendance"] });
    },
  });

  const deleteAttendance = useMutation({
    mutationFn: deleteTeacherAttendance,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacherAttendance"] });
    },
  });

  return {
    ...query,
    addAttendance,
    editAttendance,
    deleteAttendance,
  };
}
