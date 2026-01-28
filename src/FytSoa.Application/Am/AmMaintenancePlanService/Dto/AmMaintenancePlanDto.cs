using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 保养计划 DTO
/// </summary>
public class AmMaintenancePlanDto : AppEntity
{
    [Required]
    [StringLength(64)]
    public string PlanNo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 周期类型：DAY/WEEK/MONTH/YEAR
    /// </summary>
    [Required]
    [StringLength(20)]
    public string CycleType { get; set; } = "MONTH";

    public int CycleValue { get; set; } = 1;

    /// <summary>
    /// 保养管理员Id（sys_admin.Id）
    /// </summary>
    public long ManagerId { get; set; } = 0;

    public DateTime? NextRunTime { get; set; }

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 适用范围(JSON)：资产列表/分类/地点等
    /// </summary>
    public string? ScopeJson { get; set; }

    [StringLength(1000)]
    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }
}
