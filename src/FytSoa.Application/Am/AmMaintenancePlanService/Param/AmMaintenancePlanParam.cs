using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 保养计划分页查询参数
/// </summary>
public class AmMaintenancePlanParam : PageParam
{
    /// <summary>
    /// 启用状态：0=全部；1=启用；2=停用
    /// </summary>
    public int Enabled { get; set; } = 0;
}

