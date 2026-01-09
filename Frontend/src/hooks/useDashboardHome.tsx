import { useQuery } from "@tanstack/react-query";
const server_url = import.meta.env.VITE_API_URL;

const fetchData = async (schoolId: string) => {
  const res = await fetch(`${server_url}/api/Combine/${schoolId}`);
  if (!res.ok) throw new Error(res.statusText);
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.errorMessage);
  return json.content;
};

export const useDashboardHome = (schoolId: string) => {
  return useQuery({
    queryKey: ["stats", schoolId],
    queryFn: () => fetchData(schoolId),
    staleTime: 60000,
    refetchOnWindowFocus: true,
    retry: 1,
  });
};
