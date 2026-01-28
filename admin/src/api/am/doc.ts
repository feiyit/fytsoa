import requestClient from "../http";

// 资产业务单据
export const fetchAmDocPage = (data?: any) =>
  requestClient.post("/amdoc/pages", data);

export const fetchAmDocById = (id: string) =>
  requestClient.get(`/amdoc/${id}`);

export async function addAmDoc(data: any) {
  return requestClient.post("/amdoc", data);
}

export async function updateAmDoc(data: any) {
  return requestClient.put("/amdoc", data);
}

export async function deleteAmDoc(data: any) {
  return requestClient.request("/amdoc", {
    data,
    method: "DELETE",
  });
}

export async function updateAmDocStatus(data: {
  ids: Array<string | number>;
  status: number;
}) {
  return requestClient.put("/amdoc/status", data);
}

// Export (Excel)
export const exportAmDoc = (data?: any) =>
  requestClient.post("/amdoc/export", data, { responseType: "blob" });
