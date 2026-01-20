import requestClient from "../http";
import type {
  WorkflowTaskDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface WorkflowTaskTodoListParams extends TenantQuery {
  userId: number | string;
}

export interface HandleWorkflowTaskInput extends TenantQuery {
  taskId: number | string;
  action: string; // Agree / Reject
  comment?: string;
  operatorId: number | string;
  operatorName?: string;
  /** 驳回模式：Start=发起人，Previous=上一节点，Specific=指定节点 */
  rejectMode?: "Start" | "Previous" | "Specific";
  /** RejectMode=Specific 时，指定的目标节点 Id（当前简化为节点名称） */
  rejectToNodeId?: string;
  /** 审批时若修改了业务表单，可将最新表单值传入 formData */
  formData?: Record<string, any>;
}

/**
 * 根据 Id 获取任务
 */
export function getWorkflowTask(id: number | string) {
  return requestClient.get<WorkflowTaskDto>("/WorkflowTask", {
    params: { id },
  });
}

/**
 * 创建任务
 */
export function createWorkflowTask(
  data: WorkflowTaskDto,
  config?: RequestConfig<WorkflowTaskDto>,
) {
  return requestClient.post<number>("/WorkflowTask", data, config);
}

/**
 * 更新任务
 */
export function updateWorkflowTask(
  data: WorkflowTaskDto,
  config?: RequestConfig<WorkflowTaskDto>,
) {
  return requestClient.put<void>("/WorkflowTask", data, config);
}

/**
 * 查询当前用户待办任务（简单示例）
 */
export function getWorkflowTodoTasks(params: WorkflowTaskTodoListParams) {
  return requestClient.get<WorkflowTaskDto[]>("/WorkflowTask/pages", {
    params,
  });
}

/**
 * 处理任务（同意 / 驳回）
 */
export function handleWorkflowTask(data: HandleWorkflowTaskInput) {
  return requestClient.post<void>("/WorkflowTask/Handle", data);
}
