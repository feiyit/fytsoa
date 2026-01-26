import requestClient from "../http";

// 资产变更历史（留痕）
export const fetchAmAssetHistoryPage = (data?: any) =>
  requestClient.post("/amassethistory/pages", data);

export const fetchAmAssetHistoryById = (id: string) =>
  requestClient.get(`/amassethistory/${id}`);

export async function addAmAssetHistory(data: any) {
  return requestClient.post("/amassethistory", data);
}

export async function deleteAmAssetHistory(data: any) {
  return requestClient.request("/amassethistory", {
    data,
    method: "DELETE",
  });
}

// Export (Excel)
export const exportAmAssetHistory = (data?: any) =>
  requestClient.post("/amassethistory/export", data, { responseType: "blob" });
