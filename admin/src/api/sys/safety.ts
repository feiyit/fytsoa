import requestClient from "../http";

export async function fetchSafetyFind() {
  return requestClient.get('/syssafety');
}

export async function addSafety(input: any) {
  return requestClient.post('/syssafety', input);
}
