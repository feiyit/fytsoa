import requestClient from "../http";

// 资产折旧台账/配置
export const fetchAmAssetDepreciationPage = (data?: any) =>
  requestClient.post("/amassetdepreciation/pages", data);

export const fetchAmAssetDepreciationById = (id: string) =>
  requestClient.get(`/amassetdepreciation/${id}`);

export async function addAmAssetDepreciation(data: any) {
  return requestClient.post("/amassetdepreciation", data);
}

export async function updateAmAssetDepreciation(data: any) {
  return requestClient.put("/amassetdepreciation", data);
}

export async function deleteAmAssetDepreciation(data: any) {
  return requestClient.request("/amassetdepreciation", {
    data,
    method: "DELETE",
  });
}

