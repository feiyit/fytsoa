using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产提醒任务（由规则/定时任务生成）。
/// 对应表：am_reminder_task
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_reminder_task")]
public class AmReminderTask : Entity
{
    /// <summary>
    /// 规则Id
    /// </summary>
    [Required]
    public long RuleId { get; set; } = 0;

    /// <summary>
    /// 关联业务类型：ASSET/DOC/INVENTORY/MAINTENANCE 等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    /// <summary>
    /// 业务Id
    /// </summary>
    [Required]
    public long BizId { get; set; } = 0;

    /// <summary>
    /// 接收人Id（sys_admin.Id）
    /// </summary>
    [Required]
    public long ReceiverUserId { get; set; } = 0;

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [StringLength(2000)]
    public string? Content { get; set; }

    /// <summary>
    /// 到期/触发时间
    /// </summary>
    public DateTime? DueTime { get; set; }

    /// <summary>
    /// 状态：0=待发送,1=已发送,2=已读,3=已关闭
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SentTime { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    #region 导航属性

    /// <summary>
    /// 规则信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RuleId))]
    public AmReminderRule? RuleObj { get; set; }

    #endregion
}
