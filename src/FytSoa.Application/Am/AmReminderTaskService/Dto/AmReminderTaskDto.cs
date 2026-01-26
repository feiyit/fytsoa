using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产提醒任务 DTO
/// </summary>
public class AmReminderTaskDto : AppEntity
{
    public long RuleId { get; set; } = 0;

    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    public long BizId { get; set; } = 0;

    public long ReceiverUserId { get; set; } = 0;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Content { get; set; }

    public DateTime? DueTime { get; set; }

    /// <summary>
    /// 状态：0=待发送,1=已发送,2=已读,3=已关闭
    /// </summary>
    public byte Status { get; set; } = 0;

    public DateTime? SentTime { get; set; }

    public DateTime? ReadTime { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
}

