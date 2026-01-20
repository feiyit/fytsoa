import requestClient from "../http";
import type {
  WorkflowInstanceHistoryDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface InstanceHistoryQuery extends TenantQuery {
  instanceId: number | string;
}

/**
 * 根据租户和流程实例 Id，获取该实例的所有历史记录（按时间正序）
 */
export function getWorkflowInstanceHistory(params: InstanceHistoryQuery) {
  return requestClient.get<WorkflowInstanceHistoryDto[]>(
    "/WorkflowInstanceHistory/ByInstanceId",
    { params },
  );
}

/**
 * 新增一条流程实例历史记录
 */
export function createWorkflowInstanceHistory(
  data: WorkflowInstanceHistoryDto,
  config?: RequestConfig<WorkflowInstanceHistoryDto>,
) {
  return requestClient.post<number>("/WorkflowInstanceHistory", data, config);
}

