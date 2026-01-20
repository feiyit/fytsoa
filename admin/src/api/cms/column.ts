import requestClient from "../http";

export const fetchCmsColumnList = (params?: any) =>
  requestClient.get('/cmscolumn/list', {
    params,
  });

export const fetchCmsColumnById = (id: string) =>
  requestClient.get(`/cmscolumn/${id}`);

export async function addCmsColumn(data: any) {
  return requestClient.post('/cmscolumn', data);
}

export async function updateCmsColumn(data: any) {
  return requestClient.put('/cmscolumn', data);
}

export async function deleteCmsColumn(data: any) {
  return requestClient.request('/cmscolumn', {
    data: data,
    method: 'DELETE',
  });
}
