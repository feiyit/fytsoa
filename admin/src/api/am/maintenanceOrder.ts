import requestClient from "../http";

// 维修/保养工单
export const fetchAmMaintenanceOrderPage = (data?: any) =>
  requestClient.post("/ammaintenanceorder/pages", data);

export const fetchAmMaintenanceOrderById = (id: string) =>
  requestClient.get(`/ammaintenanceorder/${id}`);

export async function addAmMaintenanceOrder(data: any) {
  return requestClient.post("/ammaintenanceorder", data);
}

export async function updateAmMaintenanceOrder(data: any) {
  return requestClient.put("/ammaintenanceorder", data);
}

export async function deleteAmMaintenanceOrder(data: any) {
  return requestClient.request("/ammaintenanceorder", {
    data,
    method: "DELETE",
  });
}

