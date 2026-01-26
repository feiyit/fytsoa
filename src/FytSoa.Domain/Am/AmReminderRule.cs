using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产提醒规则。
/// 对应表：am_reminder_rule
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_reminder_rule")]
public class AmReminderRule : Entity
{
    /// <summary>
    /// 规则类型：BORROW_DUE/WARRANTY_EXPIRE/INVENTORY_DUE/MAINTENANCE_DUE/TRANSFER_SIGN等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 提前天数（到期前N天提醒）
    /// </summary>
    [Required]
    public int DaysBefore { get; set; } = 0;

    /// <summary>
    /// 规则配置(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? ConfigJson { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    [StringLength(50)]
    public string? CreateUser { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [StringLength(50)]
    public string? UpdateUser { get; set; }

    #region 导航属性

    /// <summary>
    /// 规则生成的提醒任务列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AmReminderTask.RuleId))]
    public List<AmReminderTask> Tasks { get; set; } = new();

    #endregion
}
