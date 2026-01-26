using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 盘点计划分页查询参数
/// </summary>
public class AmInventoryPlanParam : PageParam
{
    /// <summary>
    /// 状态：0=全部；其它值对应 am_inventory_plan.Status
    /// </summary>
    public int PlanStatus { get; set; } = 0;
}

