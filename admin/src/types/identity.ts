export interface PageResult<T> {
  items: T[];
  totalItems: number;
  totalPages: number;
}

export interface PageQuery {
  page: number;
  limit: number;
  key?: string;
}

export interface Tenant {
  id: string;
  code: string;
  name: string;
  createdAt: string;
}

export interface CreateTenantInput {
  code: string;
  name: string;
}

export interface UpdateTenantInput extends CreateTenantInput {
  id: string;
}

export interface UserAccount {
  id: string;
  tenantId: string;
  userName: string;
  displayName: string;
  email?: string;
  phone?: string;
  isActive: boolean;
  isLocked: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserAccountInput {
  tenantId: string;
  userName: string;
  displayName: string;
  email?: string;
  phone?: string;
  password: string;
  isActive?: boolean;
}

export interface UpdateUserAccountInput {
  id: string;
  displayName: string;
  email?: string;
  phone?: string;
  isActive: boolean;
  isLocked: boolean;
}

export interface OrgUnit {
  id: string;
  tenantId: string;
  code: string;
  name: string;
  parentId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrgUnitInput {
  tenantId: string;
  code: string;
  name: string;
  parentId?: string;
  isActive: boolean;
}

export interface UpdateOrgUnitInput extends CreateOrgUnitInput {
  id: string;
}

export interface Position {
  id: string;
  tenantId: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePositionInput {
  tenantId: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface UpdatePositionInput extends CreatePositionInput {
  id: number;
}

export interface Employment {
  id: string;
  tenantId: string;
  userId: string | undefined;
  orgId: string | undefined;
  positionId: string | undefined;
  isPrimary: boolean;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmploymentInput {
  tenantId: string;
  userId: string | undefined;
  orgId: string | undefined;
  positionId: string | undefined;
  isPrimary: boolean;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
}

export interface UpdateEmploymentInput extends CreateEmploymentInput {
  id: number;
}

export interface OrgLeader {
  id: string;
  tenantId: string;
  orgId: string;
  leaderUserId: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrgLeaderInput {
  tenantId: string;
  orgId: string;
  leaderUserId: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
}

export interface UpdateOrgLeaderInput extends CreateOrgLeaderInput {
  id: string;
}

export interface ReportingRelation {
  id: string;
  tenantId: string;
  reporterUserId: string;
  managerUserId: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateReportingRelationInput {
  tenantId: string;
  reporterUserId: string;
  managerUserId: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
}

export interface UpdateReportingRelationInput
  extends CreateReportingRelationInput {
  id: string;
}

export interface Role {
  id: string;
  tenantId: string;
  code: string;
  name: string;
  isSystem: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRoleInput {
  tenantId: string;
  code: string;
  name: string;
}

export interface UpdateRoleInput extends CreateRoleInput {
  id: string;
  isSystem: boolean;
}

export interface Delegation {
  id: string;
  tenantId: string;
  delegatorUserId: string;
  delegateeUserId: string;
  scope: string;
  startAt: string;
  endAt: string;
  isActive: boolean;
  note?: string;
}

export interface CreateDelegationInput {
  tenantId: string;
  delegatorUserId: string;
  delegateeUserId: string;
  scope: string;
  startAt: string;
  endAt: string;
  isActive: boolean;
  note?: string;
}

export interface UpdateDelegationInput extends CreateDelegationInput {
  id: string;
}

export interface UserAccount {
  id: string;
  tenantId: string;
  fullName: string;
  userName: string;
  displayName: string;
  email?: string;
  phone?: string;
  isActive: boolean;
  isLocked: boolean;
  createdAt: string;
  updatedAt: string;
}
export type EmploymentRelation = 'LINE' | 'FUNCTIONAL' | string;
export interface CreateEmploymentReportingInput {
  tenantId: string;
  subordinateEmploymentId: string;
  managerEmploymentId: string;
  relation: EmploymentRelation;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
}

export interface UpdateEmploymentReportingInput
  extends CreateEmploymentReportingInput {
  id: string;
}
export type OrgHeadType = 'PRIMARY' | 'DEPUTY' | 'ACTING' | string;
export interface CreateOrgUnitHeadInput {
  tenantId: string;
  orgId: string;
  employmentId: string;
  headType: OrgHeadType;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
  employment:any
}

export interface UpdateOrgUnitHeadInput extends CreateOrgUnitHeadInput {
  id: string;
}

export interface OrgUnitHead {
  id: string;
  tenantId: string;
  orgId: string;
  employmentId: string;
  headType: OrgHeadType;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrgUnitHeadInput {
  tenantId: string;
  orgId: string;
  employmentId: string;
  headType: OrgHeadType;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
}

export interface UpdateOrgUnitHeadInput extends CreateOrgUnitHeadInput {
  id: string;
}

export interface EmploymentReporting {
  id: string;
  tenantId: string;
  subordinateEmploymentId: string;
  managerEmploymentId: string;
  relation: EmploymentRelation;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmploymentReportingInput {
  tenantId: string;
  subordinateEmploymentId: string;
  managerEmploymentId: string;
  relation: EmploymentRelation;
  validFrom: string;
  validTo?: string | null;
  note?: string | null;
}

export interface UpdateEmploymentReportingInput
  extends CreateEmploymentReportingInput {
  id: string;
}
