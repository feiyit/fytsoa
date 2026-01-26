import requestClient from "../http";

// 资产分类扩展信息
export const fetchAmAssetCategoryProfilePage = (data?: any) =>
  requestClient.post("/amassetcategoryprofile/pages", data);

export const fetchAmAssetCategoryProfileList = (params?: any) =>
  requestClient.get("/amassetcategoryprofile/list", { params });

export const fetchAmAssetCategoryProfileById = (id: string) =>
  requestClient.get(`/amassetcategoryprofile/${id}`);

export async function addAmAssetCategoryProfile(data: any) {
  return requestClient.post("/amassetcategoryprofile", data);
}

export async function updateAmAssetCategoryProfile(data: any) {
  return requestClient.put("/amassetcategoryprofile", data);
}

export async function deleteAmAssetCategoryProfile(data: any) {
  return requestClient.request("/amassetcategoryprofile", {
    data,
    method: "DELETE",
  });
}

