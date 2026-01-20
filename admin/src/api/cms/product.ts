import requestClient from "../http";

export const fetchCmsProductPage = (data?: any) =>
  requestClient.post("/cmsproduct/pages", data);

export const fetchCmsProductById = (id: string) =>
  requestClient.get(`/cmsproduct/${id}`);

export async function addCmsProduct(data: any) {
  return requestClient.post("/cmsproduct", data);
}

export async function updateCmsProduct(data: any) {
  return requestClient.put("/cmsproduct", data);
}

export async function deleteCmsProduct(data: any) {
  return requestClient.request("/cmsproduct", {
    data,
    method: "DELETE",
  });
}

