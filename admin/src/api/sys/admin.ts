import requestClient from "../http";

export const fetchAdminPage = (params: any) =>
  requestClient.get('/sysadmin/pages', {
    params,
  });

export const fetchAdminList = (params: any) =>
  requestClient.get('/sysadmin/list', {
    params,
  });

export const fetchAdminById = (id: number) =>
  requestClient.get(`/sysadmin/${id}`);

export const createAdmin = (input: any) =>
  requestClient.post<number>('/sysadmin', input);

export const updateUser = (input: any) =>
  requestClient.put<boolean>('/sysadmin', input);

export const deleteAdmin = (id: string) =>
  requestClient.delete<boolean>(`/sysadmin?ids=${id}`);

export const resetAdminPassword = (ids: any) =>
  requestClient.put(`/sysadmin/passreset`, ids);

export const setNewsPassword = (data: any) =>
  requestClient.put(`/sysadmin/settingpwd`, data);

export const updateBasicUser = (input: any) =>
  requestClient.put<boolean>('/sysadmin/basic', input);

export const assignAdminRoles = (data: any) =>
  requestClient.post(`/syspermission/role`, data);

export const addRolePermission = (data: any) =>
  requestClient.post(`/syspermission`, data);

export const fetchPermissionByRole = (roleId: string) =>
  requestClient.get(`/syspermission/byrole/${roleId}`);
