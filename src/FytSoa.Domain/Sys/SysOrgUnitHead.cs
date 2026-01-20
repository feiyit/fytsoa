#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 组织负责人任用信息。
/// </summary>
[SugarTable("sys_org_unit_head")]
public class SysOrgUnitHead
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; } = 0;

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; }

    [Required]
    [SugarColumn(ColumnName = "org_id")]
    public long OrgId { get; set; }

    [Required]
    [SugarColumn(ColumnName = "employment_id")]
    public long EmploymentId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(EmploymentId))]
    public SysAdmin Employment { get; set; }

    [Required]
    [StringLength(20)]
    [SugarColumn(ColumnName = "head_type")]
    public string HeadType { get; set; } = "PRIMARY";

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
