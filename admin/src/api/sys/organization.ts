import requestClient from "../http";
import type {
  CreateOrgUnitInput,
  CreatePositionInput,
  CreateEmploymentInput,
  CreateEmploymentReportingInput,
  CreateOrgUnitHeadInput,
  OrgUnit,
  PageQuery,
  PageResult,
  Position,
  Employment,
  EmploymentReporting,
  OrgUnitHead,
  UpdateOrgUnitInput,
  UpdatePositionInput,
  UpdateEmploymentInput,
  UpdateEmploymentReportingInput,
  UpdateOrgUnitHeadInput,
} from '../../types/identity';

export const fetchOrgUnitPage = (params: PageQuery) =>
  requestClient.get<PageResult<OrgUnit>>('/sysorganization/orgunitpage', {
    params,
  });
export const fetchOrgUnitList = (params: PageQuery) =>
  requestClient.get<Array<OrgUnit>>('/sysorganization/orgunitlist', {
    params,
  });

export const fetchOrgUnitTree = (tenantId: number) =>
  requestClient.get<OrgUnit[]>(`/sysorganization/${tenantId}/tree`);

export const createOrgUnit = (input: CreateOrgUnitInput) =>
  requestClient.post<number>('/sysorganization/orgunit', input);

export const updateOrgUnit = (input: UpdateOrgUnitInput) =>
  requestClient.put<boolean>('/sysorganization/orgunit', input);

export const deleteOrgUnit = (id: string) =>
  requestClient.delete<boolean>(`/sysorganization/orgunit?orgUnitId=${id}`);

export const fetchPositionPage = (params: PageQuery) =>
  requestClient.get<PageResult<Position>>('/sysorganization/positionpage', {
    params,
  });

export const fetchPositionList = () =>
  requestClient.get<Position[]>('/sysorganization/positions');

export const createPosition = (input: CreatePositionInput) =>
  requestClient.post<number>('/sysorganization/position', input);

export const updatePosition = (input: UpdatePositionInput) =>
  requestClient.put<boolean>('/sysorganization/position', input);

export const deletePosition = (id: string) =>
  requestClient.delete<boolean>(`/sysorganization/position?positionId=${id}`);

export interface EmploymentQuery {
  id?: string;
}

export const fetchEmploymentPage = (params: PageQuery) =>
  requestClient.get<PageResult<Employment>>('/sysorganization/employmentpage', {
    params,
  });
export const fetchEmploymentList = (params: EmploymentQuery) =>
  requestClient.get<Employment[]>('/sysorganization/employmentbyuser', {
    params,
  });

export const createEmployment = (input: CreateEmploymentInput) =>
  requestClient.post<number>('/sysorganization/employment', input);

export const updateEmployment = (input: UpdateEmploymentInput) =>
  requestClient.put<boolean>('/sysorganization/employment', input);

export const deleteEmployment = (id: string) =>
  requestClient.delete<boolean>(
    `/sysorganization/employment/${id}`,
  );

export interface ReportingQuery {
  tenantId: number;
  subordinateEmploymentId?: number;
  managerEmploymentId?: number;
  relation?: string;
}

export const fetchReportingList = (params: ReportingQuery) =>
  requestClient.get<EmploymentReporting[]>('/sysorganization/reportinglist', {
    params,
  });

export const createReporting = (input: CreateEmploymentReportingInput) =>
  requestClient.post<number>('/sysorganization/reporting', input);

export const updateReporting = (input: UpdateEmploymentReportingInput) =>
  requestClient.put<boolean>('/sysorganization/reporting', input);

export const deleteReporting = (id: number) =>
  requestClient.delete<boolean>(`/sysorganization/reporting/${id}`);

export interface OrgHeadQuery {
  tenantId: number;
  orgId?: number;
  headType?: string;
}

export const fetchOrgHeadList = (params: OrgHeadQuery) =>
  requestClient.get<OrgUnitHead[]>('/sysorganization/orgheads', { params });

export const createOrgHead = (input: CreateOrgUnitHeadInput) =>
  requestClient.post<number>('/sysorganization/orghead', input);

export const updateOrgHead = (input: UpdateOrgUnitHeadInput) =>
  requestClient.put<boolean>('/sysorganization/orghead', input);

export const deleteOrgHead = (id: string) =>
  requestClient.delete<boolean>(`/sysorganization/orghead/${id}`);
