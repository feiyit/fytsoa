namespace FytSoa.Application.Am;

/// <summary>
/// 资产管理 - 数据统计中心（工作台）汇总数据
/// </summary>
public class AmWorkspaceSummaryDto
{
    public long TenantId { get; set; }

    /// <summary>
    /// 数据时间（服务端生成）
    /// </summary>
    public DateTime StatsTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 汇总年度（用于趋势图等）
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 即将到期的提醒天数（用于提醒统计）
    /// </summary>
    public int DueSoonDays { get; set; } = 7;

    #region 资产

    public int AssetTotal { get; set; }
    public int AssetDelTotal { get; set; }
    public decimal AssetOriginalValueTotal { get; set; }
    public decimal AssetNetBookValueTotal { get; set; }

    /// <summary>
    /// 质保已过期数量（WarrantyExpireDate &lt; 当前时间）
    /// </summary>
    public int AssetWarrantyOverdueTotal { get; set; }

    /// <summary>
    /// 质保即将到期数量（当前时间 ~ dueSoonDays 内）
    /// </summary>
    public int AssetWarrantyDueSoonTotal { get; set; }

    public List<AmWorkspaceStatItemDto> AssetStatusStats { get; set; } = [];
    public List<AmWorkspaceStatItemDto> AssetCategoryTopStats { get; set; } = [];
    public List<AmWorkspaceStatItemDto> AssetCreatedByMonth { get; set; } = [];

    #endregion

    #region 主数据

    public int VendorTotal { get; set; }
    public int VendorEnabledTotal { get; set; }

    public int LocationTotal { get; set; }
    public int LocationEnabledTotal { get; set; }

    public int WarehouseTotal { get; set; }
    public int WarehouseEnabledTotal { get; set; }

    public int WarehouseBinTotal { get; set; }

    #endregion

    #region 盘点 / 维修 / 折旧 / 单据

    public int InventoryPlanTotal { get; set; }
    public int InventoryPlanRunningTotal { get; set; }
    public List<AmWorkspaceStatItemDto> InventoryPlanStatusStats { get; set; } = [];

    public int MaintenancePlanTotal { get; set; }
    public int MaintenancePlanEnabledTotal { get; set; }

    public int MaintenanceOrderTotal { get; set; }
    public int MaintenanceOrderOpenTotal { get; set; }
    public List<AmWorkspaceStatItemDto> MaintenanceOrderStatusStats { get; set; } = [];
    public List<AmWorkspaceStatItemDto> MaintenanceOrderTypeStats { get; set; } = [];

    public int DocTotal { get; set; }

    public int AssetDepreciationTotal { get; set; }
    public int AssetDepreciationEnabledTotal { get; set; }
    public decimal AssetDepreciationAccumAmountTotal { get; set; }

    public int DepreciationRunTotal { get; set; }
    public string? LastDepreciationRunPeriod { get; set; }
    public decimal DepreciationRunTotalAmountAll { get; set; }

    #endregion

    #region 提醒

    public int ReminderRuleTotal { get; set; }
    public int ReminderRuleEnabledTotal { get; set; }

    public int ReminderTaskTotal { get; set; }
    public int ReminderTaskOpenTotal { get; set; }
    public int ReminderTaskOverdueTotal { get; set; }
    public int ReminderTaskDueSoonTotal { get; set; }
    public List<AmWorkspaceStatItemDto> ReminderTaskStatusStats { get; set; } = [];

    #endregion

    public class AmWorkspaceStatItemDto
    {
        /// <summary>
        /// 用于区分项的 key（如状态码）
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 展示名称（如：在库、已完成等）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 统计值
        /// </summary>
        public int Value { get; set; }
    }
}
