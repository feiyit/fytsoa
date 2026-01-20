import requestClient from "../http";
import type {
  WorkflowFormDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface WorkflowFormListParams extends TenantQuery {
  /** 关键字，可匹配 code / name */
  keyword?: string;
  /** 状态：0=草稿,1=启用,2=停用 */
  status?: number;
}

/**
 * 根据 Id 获取表单定义
 */
export function getWorkflowForm(id: number | string) {
  return requestClient.get<WorkflowFormDto>("/WorkflowForm", {
    params: { id },
  });
}

/**
 * 表单定义列表（按关键字 / 状态）
 */
export function getWorkflowFormList(params: WorkflowFormListParams) {
  return requestClient.get<WorkflowFormDto[]>("/WorkflowForm/List", {
    params,
  });
}

/**
 * 创建表单定义（默认状态：草稿）
 */
export function createWorkflowForm(
  data: WorkflowFormDto,
  config?: RequestConfig<WorkflowFormDto>,
) {
  return requestClient.post<number>("/WorkflowForm", data, config);
}

/**
 * 更新表单定义
 */
export function updateWorkflowForm(
  data: WorkflowFormDto,
  config?: RequestConfig<WorkflowFormDto>,
) {
  return requestClient.put<void>("/WorkflowForm", data, config);
}

/**
 * 删除表单定义
 */
export function deleteWorkflowForm(id: number | string) {
  return requestClient.delete<void>("/WorkflowForm", {
    params: { id },
  });
}

