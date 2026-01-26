using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产提醒规则 DTO
/// </summary>
public class AmReminderRuleDto : AppEntity
{
    [Required]
    [StringLength(32)]
    public string RuleType { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public int DaysBefore { get; set; } = 0;

    public string? ConfigJson { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }
}

