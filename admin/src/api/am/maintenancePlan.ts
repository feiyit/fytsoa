import requestClient from "../http";

// 保养计划
export const fetchAmMaintenancePlanPage = (data?: any) =>
  requestClient.post("/ammaintenanceplan/pages", data);

export const fetchAmMaintenancePlanById = (id: string) =>
  requestClient.get(`/ammaintenanceplan/${id}`);

export async function addAmMaintenancePlan(data: any) {
  return requestClient.post("/ammaintenanceplan", data);
}

export async function updateAmMaintenancePlan(data: any) {
  return requestClient.put("/ammaintenanceplan", data);
}

export async function deleteAmMaintenancePlan(data: any) {
  return requestClient.request("/ammaintenanceplan", {
    data,
    method: "DELETE",
  });
}

