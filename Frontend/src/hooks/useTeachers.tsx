import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

const server_url = import.meta.env.VITE_API_URL;

// Fetch all teachers
const fetchTeachers = async () => {
  const res = await fetch(`${server_url}/api/Teacher`, {
    credentials: "include", // 👈 Include auth cookies
  });
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  console.log(json.content);
  return json.content ?? [];
};

// Create teacher
const createTeacher = async ({ newTeacher }: { newTeacher: any }) => {
  const payload = {
    ...newTeacher,
  };

  const res = await fetch(`${server_url}/api/Teacher`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 👈 Include auth cookies
    body: JSON.stringify(payload),
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Update teacher
const updateTeacher = async ({ updatedTeacher }: { updatedTeacher: any }) => {
  const res = await fetch(`${server_url}/api/Teacher/${updatedTeacher.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 👈 Include auth cookies
    body: JSON.stringify(updatedTeacher),
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Delete teacher
const removeTeacher = async ({ id }: { id: string }) => {
  const res = await fetch(`${server_url}/api/Teacher/${id}`, {
    method: "DELETE",
    credentials: "include", // 👈 Include auth cookies
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};


// Hook
export const useTeachers = () => {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["teachers"],
    queryFn: fetchTeachers,
    staleTime: 60000,
    refetchOnWindowFocus: true,
    retry: 1,
  });

  const addTeacher = useMutation({
    mutationFn: ({ newTeacher }: { newTeacher: any }) =>
      createTeacher({ newTeacher }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });

  const editTeacher = useMutation({
    mutationFn: ({ updatedTeacher }: { updatedTeacher: any }) =>
      updateTeacher({ updatedTeacher }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });

  const deleteTeacher = useMutation({
    mutationFn: ({ id }: { id: string }) => removeTeacher({ id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });

  return {
    ...query,
    addTeacher: addTeacher.mutateAsync,
    editTeacher: editTeacher.mutateAsync,
    deleteTeacher: deleteTeacher.mutateAsync,
  };
};
