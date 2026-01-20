import requestClient from "../http";

export interface SaveBusinessInput {
  tenantId: number | string;
  definitionId: number | string;
  definitionKey: string;
  businessKey?: string;
  formData: Record<string, any>;
  createdBy: number | string;
}

// 后端通用业务数据 Dto（对应 wf_business_data）
export interface WorkflowBusinessDataDto {
  id?: string;
  tenantId?: string;
  definitionId?: string;
  definitionKey?: string;
  businessKey?: string;
  formDataJson?: string | null;
  createdBy?: string | number;
  createdAt?: string;
}

export interface GetBusinessParams {
  tenantId: number | string;
  definitionKey: string;
  businessKey: string;
}

/**
 * 保存业务数据，返回最终 businessKey
 */
export function saveWorkflowBusiness(data: SaveBusinessInput) {
  return requestClient.post<string>("/WorkflowBusiness/save", data);
}

/**
 * 根据 TenantId + DefinitionKey + BusinessKey 获取业务数据
 * 注意：需要后端提供对应的 /WorkflowBusiness 接口
 */
export function getWorkflowBusiness(params: GetBusinessParams) {
  return requestClient.get<WorkflowBusinessDataDto>("/WorkflowBusiness", {
    params,
  });
}

