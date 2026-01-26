import requestClient from "../http";

// 折旧计提批次
export const fetchAmDepreciationRunPage = (data?: any) =>
  requestClient.post("/amdepreciationrun/pages", data);

export const fetchAmDepreciationRunById = (id: string) =>
  requestClient.get(`/amdepreciationrun/${id}`);

export async function addAmDepreciationRun(data: any) {
  return requestClient.post("/amdepreciationrun", data);
}

export async function updateAmDepreciationRun(data: any) {
  return requestClient.put("/amdepreciationrun", data);
}

// 确认/过账（后端：ConfirmAsync）
export async function confirmAmDepreciationRun(id: string) {
  return requestClient.post("/amdepreciationrun/confirm", null, {
    params: { id },
  });
}

export async function deleteAmDepreciationRun(data: any) {
  return requestClient.request("/amdepreciationrun", {
    data,
    method: "DELETE",
  });
}

