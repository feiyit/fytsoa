import requestClient from "../http";

/**
 * 查询
 */
export async function sysmenuList(data?: any) {
  return requestClient.get('/sysmenu/list', data);
}

/**
 * 添加
 */
export async function sysmenuAdd(data: any) {
  return requestClient.post('/sysmenu/temp', data);
}

/**
 * 修改
 */
export async function sysmenuPut(data: any) {
  return requestClient.put('/sysmenu', data);
}

/**
 * 删除
 */
export async function sysmenuDelete(data: any) {
  //return requestClient.delete('/sysmenu', data);
  return requestClient.request('/sysmenu', {
    data: data,
    method: 'DELETE',
  });
}
