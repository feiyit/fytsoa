import requestClient from "../http";
/**
 * 角色列表
 */
export async function fetchRoleList(params?: any) {
  return requestClient.get('/sysrole/list', { params });
}

/**
 * 创建
 */
export async function createRole(data: any) {
  return requestClient.post('/sysrole', data);
}

/**
 * 修改
 */
export async function updateRole(data: any) {
  return requestClient.put('/sysrole', data);
}

/**
 * 删除
 */
export async function deleteRole(data: any) {
  return requestClient.request('/sysrole', {
    data: data,
    method: 'DELETE',
  });
}
