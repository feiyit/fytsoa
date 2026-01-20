#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 组织结构节点。
/// </summary>
[SugarTable("sys_org_unit")]
public class SysOrgUnit
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; } = 0;

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; } = 0;

    [Required]
    [StringLength(64)]
    [SugarColumn(ColumnName = "code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "parent_id")]
    public long ParentId { get; set; } = 0;

    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; } = true;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }

    [SugarColumn(ColumnName = "updated_at")]
    public DateTime UpdatedAt { get; set; }
}
