using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 保养计划（预防性维护）。
/// 对应表：am_maintenance_plan
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_maintenance_plan")]
public class AmMaintenancePlan : Entity
{
    /// <summary>
    /// 计划编号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>
    /// 计划名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 周期类型：DAY/WEEK/MONTH/YEAR
    /// </summary>
    [Required]
    [StringLength(20)]
    public string CycleType { get; set; } = "MONTH";

    /// <summary>
    /// 周期值
    /// </summary>
    [Required]
    public int CycleValue { get; set; } = 1;

    /// <summary>
    /// 保养管理员Id（sys_admin.Id）
    /// </summary>
    public long ManagerId { get; set; } = 0;

    /// <summary>
    /// 下次执行时间（生成保养工单）
    /// </summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 启用状态
    /// </summary>
    [Required]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 适用范围(JSON)：资产列表/分类/地点等
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? ScopeJson { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(1000)]
    public string? Remark { get; set; }

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
}
