import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";

const server_url = import.meta.env.VITE_API_URL;

export const useRooms = () => {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  
  const {
    data,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["meetingRooms"],
    queryFn: async () => {
      const res = await fetch(`${server_url}/api/ChatRooms`, {
        credentials: "include",
      });
      if (res.status === 401) {
        navigate("/login");
        throw new Error("Unauthorized");
      }
      if (!res.ok) throw new Error("Failed to fetch meeting rooms");
      return res.json();
    },
  });

  // ➕ Create a new room
  const addRoom = useMutation({
    mutationFn: async ({ room }: { room: any }) => {
      const res = await fetch(`${server_url}/api/ChatRooms`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(room),
      });
      if (res.status === 401) {
        navigate("/login");
        const error = await res.json().catch(() => ({ message: "Unauthorized" }));
        throw new Error(error.message || "Unauthorized");
      }
      if (!res.ok) {
        const error = await res.json().catch(() => ({}));
        throw new Error(error.message || "Failed to create room");
      }
      return res.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["meetingRooms"] });
    },
  });

  // 🔐 Join a room - now returns room access token
  const joinRoom = useMutation({
    mutationFn: async ({
      roomId,
      password,
    }: {
      roomId: string;
      password: string;
    }) => {
      const res = await fetch(`${server_url}/api/ChatRooms/join`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ roomId, password }),
      });

      if (res.status === 401) {
        navigate("/login");
        const json = await res.json().catch(() => ({ message: "Unauthorized" }));
        throw new Error(json.message || "Unauthorized");
      }

      const json = await res.json();
      if (!res.ok) {
        throw new Error(json.message || "Failed to join room");
      }

      return json; // Returns: { ok, message, roomAccessToken, roomDetails }
    },
  });

  // ❌ Delete a room
  const deleteRoom = useMutation({
    mutationFn: async ({ id }: { id: string }) => {
      const res = await fetch(`${server_url}/api/ChatRooms/${id}`, {
        method: "DELETE",
        credentials: "include",
      });
      if (res.status === 401) {
        navigate("/login");
        throw new Error("Unauthorized");
      }
      if (!res.ok) {
        if (res.status === 403) {
          throw new Error("You don't have permission to delete this room");
        }
        throw new Error("Failed to delete room");
      }
      return res.status === 204 ? null : res.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["meetingRooms"] });
    },
  });

  return {
    data,
    isLoading,
    error,
    addRoom: addRoom.mutateAsync,
    joinRoom: joinRoom.mutateAsync,
    deleteRoom: deleteRoom.mutateAsync,
  };
};
