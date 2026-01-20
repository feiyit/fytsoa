import requestClient from "../http";
import type {
  WorkflowDefinitionDto,
  RequestConfig,
  TenantQuery,
} from "./types";

export interface WorkflowDefinitionListParams extends TenantQuery {
  /** 关键字，可匹配 defKey / defName */
  keyword?: string;
  /** 状态：0=草稿,1=已发布,2=停用 */
  status?: number;
}

/**
 * 根据 Id 获取流程定义
 */
export function getWorkflowDefinition(id: number | string) {
  return requestClient.get<WorkflowDefinitionDto>("/WorkflowDefinition", {
    params: { id },
  });
}

/**
 * 流程定义列表（按关键字 / 状态简单查询）
 */
export function getWorkflowDefinitionList(params: WorkflowDefinitionListParams) {
  return requestClient.get<WorkflowDefinitionDto[]>(
    "/WorkflowDefinition/List",
    { params },
  );
}

/**
 * 创建流程定义（默认版本 1、草稿）
 */
export function createWorkflowDefinition(
  data: WorkflowDefinitionDto,
  config?: RequestConfig<WorkflowDefinitionDto>,
) {
  return requestClient.post<number>("/WorkflowDefinition", data, config);
}

/**
 * 更新流程定义（仅基础信息，不含模型）
 */
export function updateWorkflowDefinition(
  data: WorkflowDefinitionDto,
  config?: RequestConfig<WorkflowDefinitionDto>,
) {
  return requestClient.put<void>("/WorkflowDefinition", data, config);
}

/**
 * 删除流程定义（简单物理删除）
 */
export function deleteWorkflowDefinition(id: number | string) {
  return requestClient.delete<void>("/WorkflowDefinition", {
    params: { id },
  });
}

