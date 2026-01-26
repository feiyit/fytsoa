import requestClient from "../http";

// 地点
export const fetchAmLocationPage = (data?: any) =>
  requestClient.post("/amlocation/pages", data);

export const fetchAmLocationList = (params?: any) =>
  requestClient.get("/amlocation/list", { params });

export const fetchAmLocationById = (id: string) =>
  requestClient.get(`/amlocation/${id}`);

export async function addAmLocation(data: any) {
  return requestClient.post("/amlocation", data);
}

export async function updateAmLocation(data: any) {
  return requestClient.put("/amlocation", data);
}

export async function deleteAmLocation(data: any) {
  return requestClient.request("/amlocation", {
    data,
    method: "DELETE",
  });
}

