using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 提醒规则分页查询参数
/// </summary>
public class AmReminderRuleParam : PageParam
{
    public string? RuleType { get; set; }

    /// <summary>
    /// 启用状态：0=全部；1=启用；2=停用
    /// </summary>
    public int Enabled { get; set; } = 0;
}

