import requestClient from "../http";
import type {
  WorkflowInstanceDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface WorkflowInstanceByBusinessKeyParams extends TenantQuery {
  definitionKey: string;
  businessKey: string;
}

// 我发起的流程实例分页查询参数
export interface WorkflowInstanceMyStartParams extends TenantQuery {
  /** 当前用户 Id（发起人） */
  userId: number | string;
}

/**
 * 根据 Id 获取流程实例
 */
export function getWorkflowInstance(id: number | string) {
  return requestClient.get<WorkflowInstanceDto>("/WorkflowInstance", {
    params: { id },
  });
}

/**
 * 按业务主键查询流程实例列表
 */
export function getWorkflowInstancesByBusinessKey(
  params: WorkflowInstanceByBusinessKeyParams,
) {
  return requestClient.get<WorkflowInstanceDto[]>(
    "/WorkflowInstance/ByBusinessKey",
    { params },
  );
}

/**
 * 分页查询“我发起的流程实例”
 */
export function getMyStartedWorkflowInstances(
  params: WorkflowInstanceMyStartParams,
) {
  return requestClient.get<WorkflowInstanceDto[]>(
    "/WorkflowInstance/MyStartPages",
    { params },
  );
}

/**
 * 创建流程实例
 */
export function createWorkflowInstance(
  data: WorkflowInstanceDto,
  config?: RequestConfig<WorkflowInstanceDto>,
) {
  return requestClient.post<number>("/WorkflowInstance", data, config);
}

/**
 * 更新流程实例
 */
export function updateWorkflowInstance(
  data: WorkflowInstanceDto,
  config?: RequestConfig<WorkflowInstanceDto>,
) {
  return requestClient.put<void>("/WorkflowInstance", data, config);
}
