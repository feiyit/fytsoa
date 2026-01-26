import requestClient from "../http";

export interface AmWorkspaceStatItemDto {
  key: string;
  name: string;
  value: number;
}

export interface AmWorkspaceSummaryDto {
  tenantId: number;
  statsTime: string;
  year: number;
  dueSoonDays: number;

  assetTotal: number;
  assetDelTotal: number;
  assetOriginalValueTotal: number;
  assetNetBookValueTotal: number;
  assetWarrantyOverdueTotal: number;
  assetWarrantyDueSoonTotal: number;
  assetStatusStats: AmWorkspaceStatItemDto[];
  assetCategoryTopStats: AmWorkspaceStatItemDto[];
  assetCreatedByMonth: AmWorkspaceStatItemDto[];

  vendorTotal: number;
  vendorEnabledTotal: number;
  locationTotal: number;
  locationEnabledTotal: number;
  warehouseTotal: number;
  warehouseEnabledTotal: number;
  warehouseBinTotal: number;

  inventoryPlanTotal: number;
  inventoryPlanRunningTotal: number;
  inventoryPlanStatusStats: AmWorkspaceStatItemDto[];

  maintenancePlanTotal: number;
  maintenancePlanEnabledTotal: number;
  maintenanceOrderTotal: number;
  maintenanceOrderOpenTotal: number;
  maintenanceOrderStatusStats: AmWorkspaceStatItemDto[];
  maintenanceOrderTypeStats: AmWorkspaceStatItemDto[];

  docTotal: number;

  assetDepreciationTotal: number;
  assetDepreciationEnabledTotal: number;
  assetDepreciationAccumAmountTotal: number;
  depreciationRunTotal: number;
  lastDepreciationRunPeriod?: string | null;
  depreciationRunTotalAmountAll: number;

  reminderRuleTotal: number;
  reminderRuleEnabledTotal: number;
  reminderTaskTotal: number;
  reminderTaskOpenTotal: number;
  reminderTaskOverdueTotal: number;
  reminderTaskDueSoonTotal: number;
  reminderTaskStatusStats: AmWorkspaceStatItemDto[];
}

// 资产管理 - 工作台汇总
export const fetchAmWorkspaceSummary = (params?: {
  tenantId?: number;
  year?: number;
  dueSoonDays?: number;
}) => requestClient.get<AmWorkspaceSummaryDto>("/amworkspace/summary", { params });
