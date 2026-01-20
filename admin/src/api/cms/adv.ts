import requestClient from "../http";

export const fetchAdvInfoPage = (params: any) =>
  requestClient.get('/sysadvinfo/pages', {
    params,
  });

export const fetchAdvInfoById = (id: string) =>
  requestClient.get(`/sysadvinfo/${id}`);

export async function addAdvInfo(data: any) {
  return requestClient.post('/sysadvinfo', data);
}

export async function updateAdvInfo(data: any) {
  return requestClient.put('/sysadvinfo', data);
}

export async function deleteAdvInfo(data: any) {
  return requestClient.request('/sysadvinfo', {
    data: data,
    method: 'DELETE',
  });
}

export const fetchAdvColumnList = (params?: any) =>
  requestClient.get('/sysadvcolumn/list', {
    params,
  });

export const fetchAdvColumnById = (id: string) =>
  requestClient.get(`/sysadvcolumn/${id}`);

export async function addAdvColumn(data: any) {
  return requestClient.post('/sysadvcolumn', data);
}

export async function updateAdvColumn(data: any) {
  return requestClient.put('/sysadvcolumn', data);
}

export const deleteAdvColumn = (id: string) =>
  requestClient.delete<boolean>(`/sysadvcolumn?id=${id}`);
