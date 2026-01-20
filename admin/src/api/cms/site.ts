import requestClient from "../http";

export const fetchCmsSiteList = (params?: any) =>
  requestClient.get('/cmssite/list', {
    params,
  });

export const fetchCmsSiteById = (id: string) =>
  requestClient.get(`/cmssite/${id}`);

export async function addCmsSite(data: any) {
  return requestClient.post('/cmssite', data);
}

export async function updateCmsSite(data: any) {
  return requestClient.put('/cmssite', data);
}

export async function deleteCmsSite(data: any) {
  return requestClient.request('/cmssite', {
    data: data,
    method: 'DELETE',
  });
}
