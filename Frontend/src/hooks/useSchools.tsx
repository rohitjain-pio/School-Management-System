const server_url = import.meta.env.VITE_API_URL;

const fetchSchools = async (schoolName: string): Promise<string[]> => {
  const res = await fetch(`${server_url}/api/School/search?schoolName=${schoolName}`);
  
  if (!res.ok) throw new Error(res.statusText);

  const json = await res.json();

  if (!json.isSuccess) throw new Error(json.errorMessage || "Failed to fetch schools.");

  return json.content; 
};

export default fetchSchools;
