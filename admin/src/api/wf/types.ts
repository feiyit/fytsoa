import type { AxiosRequestConfig } from "axios";

/**
 * 工作流：流程定义 Dto
 * 对应后端 WorkflowDefinitionDto
 */
export interface WorkflowDefinitionDto {
  id?: string;
  tenantId?: string;
  /** 流程编码，如 LEAVE_APPLY */
  defKey?: string | null;
  /** 流程名称 */
  defName?: string | null;
  /** 版本号，从 1 递增 */
  version?: number;
  /** 状态：0=草稿,1=已发布,2=停用 */
  status?: number;
  /** 所属模块 / 分类 */
  category?: string | null;
  /** 绑定表单 / 页面标识 */
  formSchemaId?: string | null;
  /** 备注 */
  remark?: string | null;
  createdBy?: number;
  createdAt?: string;
  updatedBy?: number;
  updatedAt?: string | null;
}

/**
 * 工作流：流程定义模型 Dto
 * 对应后端 WorkflowDefinitionModelDto
 */
export interface WorkflowDefinitionModelDto {
  id?: string;
  tenantId?: string;
  /** 所属流程定义 Id */
  definitionId?: string;
  /** 流程设计 JSON */
  modelJson?: string | null;
  /** 是否最新模型：true=最新 */
  isLatest?: boolean;
  createdBy?: number;
  createdAt?: string;
}

/**
 * 工作流：流程实例 Dto
 */
export interface WorkflowInstanceDto {
  id?: string | null;
  tenantId?: string | null;
  definitionId?: string | null;
  definitionKey?: string | null;
  /** 业务主键（单据 Id 等） */
  businessKey?: string | null;
  /** 流程标题 */
  title?: string | null;
  startUserId?: string | null;
  startUserName?: string | null;
  startTime?: string;
  endTime?: string | null;
  /** 实例状态 */
  status?: number;
  /** 当前活动节点 Id 集合（逗号分隔） */
  currentNodeIds?: string | null;
  createdAt?: string;
  updatedAt?: string | null;
}

/**
 * 工作流：流程实例历史 Dto
 */
export interface WorkflowInstanceHistoryDto {
  id?: string;
  tenantId?: string;
  instanceId?: string;
  /** 事件类型，如 Start/Complete/Cancel 等 */
  eventType?: string | null;
  fromStatus?: number | null;
  toStatus?: number | null;
  operatorId?: string;
  operatorName?: string | null;
  remark?: string | null;
  createdAt?: string;
}

/**
 * 工作流：任务 Dto
 */
export interface WorkflowTaskDto {
  id?: string;
  tenantId?: string;
  instanceId?: string;
  nodeId?: string | null;
  nodeName?: string | null;
  assigneeId?: string;
  assigneeName?: string | null;
  /** 任务状态 */
  status?: number;
  /** 审批动作，例如 Agree/Reject */
  action?: string | null;
  comment?: string | null;
  createdAt?: string;
  completedAt?: string | null;
}

/**
 * 工作流：任务候选人 Dto
 */
export interface WorkflowTaskCandidateDto {
  id?: string;
  tenantId?: string;
  taskId?: string;
  userId?: string;
  userName?: string | null;
  createdAt?: string;
}

/**
 * 工作流：任务历史 Dto
 */
export interface WorkflowTaskHistoryDto {
  id?: string;
  tenantId?: string;
  instanceId?: string;
  taskId?: string;
  nodeId?: string | null;
  nodeName?: string | null;
  assigneeId?: string;
  assigneeName?: string | null;
  action?: string | null;
  comment?: string | null;
  createdAt?: string;
  completedAt?: string | null;
}

/**
 * 工作流：表单定义 Dto
 */
export interface WorkflowFormDto {
  id?: string;
  tenantId?: string;
  /** 表单编码（唯一），例如：LEAVE_FORM */
  code?: string | null;
  /** 表单名称 */
  name?: string | null;
  /** 表单设计 JSON（包含画布结构、表单配置等） */
  schemaJson?: string | null;
  /** 对应的前端路由路径（可选），例如：/biz/leave/apply */
  routePath?: string | null;
  /** 状态：0=草稿,1=启用,2=停用 */
  status?: number;
  /** 备注 */
  remark?: string | null;
  createdBy?: number;
  createdAt?: string;
  updatedBy?: number;
  updatedAt?: string | null;
}

// 公共查询参数类型（方便复用）
export interface TenantQuery {
  tenantId: number | string;
}

export type RequestConfig<D = any> = AxiosRequestConfig<D>;
