import requestClient from "../http";

// 供应商
export const fetchAmVendorPage = (data?: any) =>
  requestClient.post("/amvendor/pages", data);

export const fetchAmVendorList = (params?: any) =>
  requestClient.get("/amvendor/list", { params });

export const fetchAmVendorById = (id: string) =>
  requestClient.get(`/amvendor/${id}`);

export async function addAmVendor(data: any) {
  return requestClient.post("/amvendor", data);
}

export async function updateAmVendor(data: any) {
  return requestClient.put("/amvendor", data);
}

export async function deleteAmVendor(data: any) {
  return requestClient.request("/amvendor", {
    data,
    method: "DELETE",
  });
}

