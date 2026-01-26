import requestClient from "../http";

// 仓位
export const fetchAmWarehouseBinPage = (data?: any) =>
  requestClient.post("/amwarehousebin/pages", data);

export const fetchAmWarehouseBinList = (params?: any) =>
  requestClient.get("/amwarehousebin/list", { params });

export const fetchAmWarehouseBinById = (id: string) =>
  requestClient.get(`/amwarehousebin/${id}`);

export async function addAmWarehouseBin(data: any) {
  return requestClient.post("/amwarehousebin", data);
}

export async function updateAmWarehouseBin(data: any) {
  return requestClient.put("/amwarehousebin", data);
}

export async function deleteAmWarehouseBin(data: any) {
  return requestClient.request("/amwarehousebin", {
    data,
    method: "DELETE",
  });
}

