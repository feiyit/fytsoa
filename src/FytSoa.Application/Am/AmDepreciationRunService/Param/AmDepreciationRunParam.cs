using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧计提分页查询参数
/// </summary>
public class AmDepreciationRunParam : PageParam
{
    /// <summary>
    /// 期间(YYYY-MM)，为空表示全部
    /// </summary>
    public string? Period { get; set; }

    /// <summary>
    /// 状态：0=全部；其它值对应 am_depreciation_run.Status
    /// </summary>
    public int RunStatus { get; set; } = 0;
}

