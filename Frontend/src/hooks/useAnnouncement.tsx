import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

const server_url = import.meta.env.VITE_API_URL;

const fetchAnnouncement = async () => {
  const res = await fetch(`${server_url}/api/Announcement`, {
    credentials: "include",
  });
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content ?? [];
};

const createAnnouncement = async ({ newAnnouncement }: { newAnnouncement: any }) => {
  const res = await fetch(`${server_url}/api/Announcement/CreateAnnouncement`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(newAnnouncement),
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

const updateAnnouncement = async ({ updatedAnnouncement }: { updatedAnnouncement: any }) => {
  const res = await fetch(`${server_url}/api/Announcement/${updatedAnnouncement.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(updatedAnnouncement),
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

const deleteAnnounc = async ({ id }: { id: string }) => {
  const res = await fetch(`${server_url}/api/Announcement/${id}`, {
    method: "DELETE",
    credentials: "include",
  });

  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

export const useAnnouncement = () => {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ["announcements"],
    queryFn: fetchAnnouncement,
  });

  const addAnnouncement = useMutation({
    mutationFn: ({ newAnnouncement }: { newAnnouncement: any }) =>
      createAnnouncement({ newAnnouncement }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["announcements"] });
    },
  });

  const editAnnouncement = useMutation({
    mutationFn: ({ updatedAnnouncement }: { updatedAnnouncement: any }) =>
      updateAnnouncement({ updatedAnnouncement }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["announcements"] });
    },
  });

  const deleteAnnouncement = useMutation({
    mutationFn: ({ id }: { id: string }) => deleteAnnounc({ id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["announcements"] });
    },
  });

  return {
    ...query,
    addAnnouncement: addAnnouncement.mutateAsync,
    editAnnouncement: editAnnouncement.mutateAsync,
    deleteAnnouncement: deleteAnnouncement.mutateAsync,
  };
};
