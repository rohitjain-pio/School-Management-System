import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { PagedResult, PaginationParams } from "@/types/pagination";
import { fetchWithCorrelation } from "@/lib/correlationId";

const server_url = import.meta.env.VITE_API_URL;


// Fetch all students (paginated)
const fetchStudents = async (params?: PaginationParams) => {
  const queryParams = new URLSearchParams();
  if (params?.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString());
  if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
  
  const url = `${server_url}/api/Student${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
  const res = await fetchWithCorrelation(url, {
    credentials: "include", // 👈 send cookies
  });
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  
  // Handle both paginated and non-paginated responses for backward compatibility
  if (json.content?.items) {
    return json.content as PagedResult<any>;
  }
  return json.content ?? [];
};

// Create a student
const createStudent = async ({ newStudent }: { newStudent: any }) => {
  const payload = {
    ...newStudent,
  };

  const res = await fetchWithCorrelation(`${server_url}/api/Student`, {
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
  const res = await fetchWithCorrelation(`${server_url}/api/Student/${updatedStudent.id}`, {
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
  const res = await fetchWithCorrelation(`${server_url}/api/Student/${id}`, {
    method: "DELETE",
    credentials: "include", // 👈 send cookies
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

// Hook
export const useStudents = (paginationParams?: PaginationParams) => {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["students", paginationParams],
    queryFn: () => fetchStudents(paginationParams),
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
