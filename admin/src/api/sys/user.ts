import requestClient from "../http";
import type {
  CreateUserAccountInput,
  PageResult,
  PageQuery,
  UpdateUserAccountInput,
  UserAccount,
} from '@/types/identity';

export interface UserAccountQuery extends PageQuery {
  tenantId: number;
  isActive?: boolean;
}

export const fetchUserAccountPage = (params: UserAccountQuery) =>
  requestClient.get<PageResult<UserAccount>>('/sysuseraccount/accountpage', {
    params,
  });

export const fetchUserAccountById = (id: number) =>
  requestClient.get<UserAccount>(`/sysuseraccount/${id}`);

export const createUserAccount = (input: CreateUserAccountInput) =>
  requestClient.post<number>('/sysuseraccount', input);

export const updateUserAccount = (input: UpdateUserAccountInput) =>
  requestClient.put<boolean>('/sysuseraccount', input);

export const deleteUserAccount = (id: number) =>
  requestClient.delete<boolean>(`/sysuseraccount/${id}`);

export const resetUserPassword = (id: number, newPassword: string) =>
  requestClient.post<boolean>(`/sysuseraccount/${id}/reset-password`, {
    password: newPassword,
  });

export const fetchUserAccountList = (tenantId: number) =>
  requestClient.get<UserAccount[]>('/sysuseraccount/list', {
    params: { tenantId },
  });
