using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 提醒任务分页查询参数
/// </summary>
public class AmReminderTaskParam : PageParam
{
    public long RuleId { get; set; } = 0;
    public string? BizType { get; set; }
    public long BizId { get; set; } = 0;
    public long ReceiverUserId { get; set; } = 0;

    /// <summary>
    /// 状态：0=全部；其它值对应 am_reminder_task.Status
    /// </summary>
    public int TaskStatus { get; set; } = 0;
}

