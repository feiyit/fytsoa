using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 工单分页查询参数
/// </summary>
public class AmMaintenanceOrderParam : PageParam
{
    /// <summary>
    /// 类型：0=全部；其它值对应 am_maintenance_order.Type
    /// </summary>
    public int OrderType { get; set; } = 0;

    /// <summary>
    /// 状态：0=全部；其它值对应 am_maintenance_order.Status
    /// </summary>
    public int OrderStatus { get; set; } = 0;

    /// <summary>
    /// 优先级：0=全部；其它值对应 am_maintenance_order.Priority
    /// </summary>
    public int Priority { get; set; } = 0;

    public long AssetId { get; set; } = 0;
    public long VendorId { get; set; } = 0;
    public long ReportUserId { get; set; } = 0;
    public long AssignUserId { get; set; } = 0;
}

