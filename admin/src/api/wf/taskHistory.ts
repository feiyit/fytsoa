import requestClient from "../http";
import type {
  WorkflowTaskHistoryDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface TaskHistoryQuery extends TenantQuery {
  instanceId: number | string;
}

// 我经办的任务历史分页查询参数
export interface TaskHandledQuery extends TenantQuery {
  /** 当前用户 Id（经办人） */
  userId: number | string;
}

/**
 * 根据实例 Id 查询任务历史列表
 */
export function getWorkflowTaskHistory(params: TaskHistoryQuery) {
  return requestClient.get<WorkflowTaskHistoryDto[]>(
    "/WorkflowTaskHistory/ByInstanceId",
    { params },
  );
}

/**
 * 分页查询“我的审批记录”（经办过的任务历史）
 */
export function getMyHandledWorkflowTaskHistory(params: TaskHandledQuery) {
  return requestClient.get<WorkflowTaskHistoryDto[]>(
    "/WorkflowTaskHistory/MyHandledPages",
    { params },
  );
}

/**
 * 新增任务历史记录
 */
export function createWorkflowTaskHistory(
  data: WorkflowTaskHistoryDto,
  config?: RequestConfig<WorkflowTaskHistoryDto>,
) {
  return requestClient.post<number>("/WorkflowTaskHistory", data, config);
}
