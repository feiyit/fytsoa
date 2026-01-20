import requestClient from "../http";
import type {
  CreateTenantInput,
  PageQuery,
  PageResult,
  Tenant,
  UpdateTenantInput,
} from '@/types/identity';

export const fetchTenantPage = (params: PageQuery) =>
  requestClient.get<PageResult<Tenant>>('/systenant/pages', { params });

export const fetchTenants = () =>
  requestClient.get<Tenant[]>('/systenant/list');

export const fetchTenantById = (id: number) =>
  requestClient.get<Tenant>(`/systenant/${id}`);

export const createTenant = (input: CreateTenantInput) =>
  requestClient.post<number>('/systenant', input);

export const updateTenant = (input: UpdateTenantInput) =>
  requestClient.put<boolean>('/systenant', input);

export const deleteTenant = (id: number) =>
  requestClient.delete<boolean>(`/systenant/${id}`);
