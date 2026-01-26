import requestClient from "../http";

export const fetchSysCodePage = (params: any) =>
  requestClient.get('/syscode/pages', {
    params,
  });

// 字典值列表（不分页）
export const fetchSysCodeList = (params?: any) =>
  requestClient.get('/syscode/list', {
    params,
  });

export const fetchSysCodeById = (id: string) =>
  requestClient.get(`/syscode/${id}`);

export async function addSysCode(data: any) {
  return requestClient.post('/syscode', data);
}

export async function updateSysCode(data: any) {
  return requestClient.put('/syscode', data);
}

export async function deleteSysCode(data: any) {
  return requestClient.request('/syscode', {
    data: data,
    method: 'DELETE',
  });
}

export const fetchSysColumnList = (params?: any) =>
  requestClient.get('/syscodetype/list', {
    params,
  });

export const fetchSysColumnById = (id: string) =>
  requestClient.get(`/syscodetype/${id}`);

export async function addSysColumn(data: any) {
  return requestClient.post('/syscodetype', data);
}

export async function updateSysColumn(data: any) {
  return requestClient.put('/syscodetype', data);
}

export const deleteSysColumn = (id: string) =>
  requestClient.delete<boolean>(`/syscodetype?id=${id}`);
