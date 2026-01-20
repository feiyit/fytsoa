import requestClient from "../http";

export const fetchCmsVariatePage = (params?: any) =>
  requestClient.get('/cmsvariate/pages', {
    params,
  });

export const fetchCmsVariateById = (id: string) =>
  requestClient.get(`/cmsvariate/${id}`);

export async function addCmsVariate(data: any) {
  return requestClient.post('/cmsvariate', data);
}

export async function updateCmsVariate(data: any) {
  return requestClient.put('/cmsvariate', data);
}

export async function deleteCmsVariate(data: any) {
  return requestClient.request('/cmsvariate', {
    data: data,
    method: 'DELETE',
  });
}
