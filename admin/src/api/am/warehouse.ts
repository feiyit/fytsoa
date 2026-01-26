import requestClient from "../http";

// 仓库
export const fetchAmWarehousePage = (data?: any) =>
  requestClient.post("/amwarehouse/pages", data);

export const fetchAmWarehouseList = (params?: any) =>
  requestClient.get("/amwarehouse/list", { params });

export const fetchAmWarehouseById = (id: string) =>
  requestClient.get(`/amwarehouse/${id}`);

export async function addAmWarehouse(data: any) {
  return requestClient.post("/amwarehouse", data);
}

export async function updateAmWarehouse(data: any) {
  return requestClient.put("/amwarehouse", data);
}

export async function deleteAmWarehouse(data: any) {
  return requestClient.request("/amwarehouse", {
    data,
    method: "DELETE",
  });
}

