import requestClient from "../http";

// 盘点计划
export const fetchAmInventoryPlanPage = (data?: any) =>
  requestClient.post("/aminventoryplan/pages", data);

export const fetchAmInventoryPlanById = (id: string) =>
  requestClient.get(`/aminventoryplan/${id}`);

export async function addAmInventoryPlan(data: any) {
  return requestClient.post("/aminventoryplan", data);
}

export async function updateAmInventoryPlan(data: any) {
  return requestClient.put("/aminventoryplan", data);
}

// 扫码/录入结果（后端：ScanAsync）
export async function scanAmInventoryItem(data: any) {
  return requestClient.post("/aminventoryplan/scan", data);
}

export async function deleteAmInventoryPlan(data: any) {
  return requestClient.request("/aminventoryplan", {
    data,
    method: "DELETE",
  });
}

