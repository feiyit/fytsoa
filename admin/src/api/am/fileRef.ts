import requestClient from "../http";

// 附件关联
export const fetchAmFileRefPage = (data?: any) =>
  requestClient.post("/amfileref/pages", data);

export const fetchAmFileRefById = (id: string) =>
  requestClient.get(`/amfileref/${id}`);

export const fetchAmFileRefList = (params?: any) =>
  requestClient.get("/amfileref/list", { params });

export async function addAmFileRef(data: any) {
  return requestClient.post("/amfileref", data);
}

// 批量绑定（后端：BindAsync）
export async function bindAmFileRef(data: any) {
  return requestClient.post("/amfileref/bind", data);
}

export async function deleteAmFileRef(data: any) {
  return requestClient.request("/amfileref", {
    data,
    method: "DELETE",
  });
}
