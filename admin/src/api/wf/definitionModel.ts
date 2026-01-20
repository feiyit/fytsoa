import requestClient from "../http";
import type {
  WorkflowDefinitionModelDto,
  TenantQuery,
  RequestConfig,
} from "./types";

export interface LatestModelParams extends TenantQuery {
  definitionId: number | string;
}

/**
 * 获取指定定义的最新模型
 */
export function getLatestWorkflowDefinitionModel(params: LatestModelParams) {
  return requestClient.get<WorkflowDefinitionModelDto | null>(
    "/WorkflowDefinitionModel/LatestByDefinitionId",
    { params },
  );
}

/**
 * 保存模型并标记为最新
 */
export function saveWorkflowDefinitionModel(
  data: WorkflowDefinitionModelDto,
  config?: RequestConfig<WorkflowDefinitionModelDto>,
) {
  return requestClient.post<number>(
    "/WorkflowDefinitionModel/Save",
    data,
    config,
  );
}

