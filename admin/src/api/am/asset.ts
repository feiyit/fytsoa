import requestClient from "../http";

// 资产台账
export const fetchAmAssetPage = (data?: any) =>
  requestClient.post("/amasset/pages", data);

export const fetchAmAssetList = (params?: any) =>
  requestClient.get("/amasset/list", { params });

export const fetchAmAssetById = (id: string) =>
  requestClient.get(`/amasset/${id}`);

export async function addAmAsset(data: any) {
  return requestClient.post("/amasset", data);
}

export async function updateAmAsset(data: any) {
  return requestClient.put("/amasset", data);
}

export async function deleteAmAsset(data: any) {
  return requestClient.request("/amasset", {
    data,
    method: "DELETE",
  });
}

// Export (Excel)
export const exportAmAsset = (data?: any) =>
  requestClient.post("/amasset/export", data, { responseType: "blob" });
