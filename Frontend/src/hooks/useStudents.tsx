import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

const server_url = import.meta.env.VITE_API_URL;


// Fetch all students
const fetchStudents = async () => {
  const res = await fetch(`${server_url}/api/Student`, {
    credentials: "include", // 👈 send cookies
  });
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content ?? [];
};

// Create a student
const createStudent = async ({ newStudent }: { newStudent: any }) => {
  const payload = {
    ...newStudent,
  };

  const res = await fetch(`${server_url}/api/Student`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 👈 send cookies
    body: JSON.stringify(payload),
  });

  const json = await res.json();
  console.log("createStudent response:", json);

  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Update student
const updateStudent = async ({ updatedStudent }: { updatedStudent: any }) => {
  const res = await fetch(`${server_url}/api/Student/${updatedStudent.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 👈 send cookies
    body: JSON.stringify(updatedStudent),
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Delete student
const removeStudent = async ({ id }: { id: string }) => {
  const res = await fetch(`${server_url}/api/Student/${id}`, {
    method: "DELETE",
    credentials: "include", // 👈 send cookies
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Hook
export const useStudents = () => {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["students"],
    queryFn: fetchStudents,
    staleTime: 60000,
    refetchOnWindowFocus: true,
    retry: 1,
  });

  const addStudent = useMutation({
    mutationFn: ({ newStudent }: { newStudent: any }) => createStudent({ newStudent }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });

  const editStudent = useMutation({
    mutationFn: ({ updatedStudent }: { updatedStudent: any }) => updateStudent({ updatedStudent }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });

  const deleteStudent = useMutation({
    mutationFn: ({ id }: { id: string }) => removeStudent({ id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });

  return {
    ...query,
    addStudent: addStudent.mutateAsync,
    editStudent: editStudent.mutateAsync,
    deleteStudent: deleteStudent.mutateAsync,
  };
};
