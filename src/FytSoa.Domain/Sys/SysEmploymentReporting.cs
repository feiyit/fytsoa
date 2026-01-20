#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 任用汇报关系定义。
/// </summary>
[SugarTable("sys_employment_reporting")]
public class SysEmploymentReporting
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; }

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; }

    [Required]
    [SugarColumn(ColumnName = "subordinate_emp_id")]
    public long SubordinateEmploymentId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(SubordinateEmploymentId))]
    public SysAdmin subordinateUser { get; set; }

    [Required]
    [SugarColumn(ColumnName = "manager_emp_id")]
    public long ManagerEmploymentId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(ManagerEmploymentId))]
    public SysAdmin managerUser { get; set; }

    [Required]
    [StringLength(20)]
    [SugarColumn(ColumnName = "relation")]
    public string Relation { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "valid_from")]
    public DateTime ValidFrom { get; set; }

    [SugarColumn(ColumnName = "valid_to")]
    public DateTime? ValidTo { get; set; }

    [StringLength(500)]
    [SugarColumn(ColumnName = "note")]
    public string? Note { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }

    [SugarColumn(ColumnName = "updated_at")]
    public DateTime UpdatedAt { get; set; }
}
