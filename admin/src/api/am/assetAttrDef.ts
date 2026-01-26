import requestClient from "../http";

// 资产分类扩展字段定义
export const fetchAmAssetAttrDefPage = (data?: any) =>
  requestClient.post("/amassetattrdef/pages", data);

export const fetchAmAssetAttrDefList = (params?: any) =>
  requestClient.get("/amassetattrdef/list", { params });

export const fetchAmAssetAttrDefById = (id: string) =>
  requestClient.get(`/amassetattrdef/${id}`);

export async function addAmAssetAttrDef(data: any) {
  return requestClient.post("/amassetattrdef", data);
}

export async function updateAmAssetAttrDef(data: any) {
  return requestClient.put("/amassetattrdef", data);
}

export async function deleteAmAssetAttrDef(data: any) {
  return requestClient.request("/amassetattrdef", {
    data,
    method: "DELETE",
  });
}

