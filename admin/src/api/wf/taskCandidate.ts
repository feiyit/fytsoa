import requestClient from "../http";
import type {
  WorkflowTaskCandidateDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface TaskCandidateQuery extends TenantQuery {
  taskId: number | string;
}

/**
 * 根据任务查询候选人列表
 */
export function getWorkflowTaskCandidates(params: TaskCandidateQuery) {
  return requestClient.get<WorkflowTaskCandidateDto[]>(
    "/WorkflowTaskCandidate/ByTaskId",
    { params },
  );
}

/**
 * 新增任务候选人
 */
export function createWorkflowTaskCandidate(
  data: WorkflowTaskCandidateDto,
  config?: RequestConfig<WorkflowTaskCandidateDto>,
) {
  return requestClient.post<number>(
    "/WorkflowTaskCandidate",
    data,
    config,
  );
}

/**
 * 删除某任务下的所有候选人
 */
export function deleteWorkflowTaskCandidates(params: TaskCandidateQuery) {
  return requestClient.delete<void>("/WorkflowTaskCandidate/ByTask", {
    params,
  });
}

