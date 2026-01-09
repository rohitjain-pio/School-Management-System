import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

const server_url = import.meta.env.VITE_API_URL;

const fetchClasses = async () => {
  const res = await fetch(`${server_url}/api/Class`, {
    credentials: "include", // 🔐 Include cookies
  });
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content ?? [];
};
const createClass = async ({ newClass }: { newClass: any }) => {
  const res = await fetch(`${server_url}/api/Class`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 🔐 Include cookies
    body: JSON.stringify(newClass),
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

const updateClass = async ({ updatedClass }: { updatedClass: any }) => {
  const res = await fetch(`${server_url}/api/Class/${updatedClass.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include", // 🔐 Include cookies
    body: JSON.stringify(updatedClass),
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

const removeClass = async ({ id }: { id: string }) => {
  const res = await fetch(`${server_url}/api/Class/${id}`, {
    method: "DELETE",
    credentials: "include", // 🔐 Include cookies
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

export const useClasses = () => {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["classes"],
    queryFn: fetchClasses,
  });

  const addClass = useMutation({
    mutationFn: ({ newClass }: { newClass: any }) => createClass({ newClass }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });

  const editClass = useMutation({
    mutationFn: ({ updatedClass }: { updatedClass: any }) =>
      updateClass({ updatedClass }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });

  const deleteClass = useMutation({
    mutationFn: ({ id }: { id: string }) => removeClass({ id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });

  return {
    ...query,
    addClass: addClass.mutateAsync,
    editClass: editClass.mutateAsync,
    deleteClass: deleteClass.mutateAsync,
  };
};
