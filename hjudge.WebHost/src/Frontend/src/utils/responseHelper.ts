export async function tryJson(res: Response) {
  try {
    return await res.json();
  }
  catch (err) {
    if (res.ok) return JSON.parse("{}");
    throw err;
  }
}