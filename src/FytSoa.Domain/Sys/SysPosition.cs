#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 岗位/职级信息。
/// </summary>
[SugarTable("sys_position")]
public class SysPosition
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; }

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; }

    [Required]
    [StringLength(64)]
    [SugarColumn(ColumnName = "code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "parent_id")]
    public long? ParentId { get; set; }

    [SugarColumn(ColumnName = "is_managerial")]
    public bool IsManagerial { get; set; }

    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; } = true;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }

    [SugarColumn(ColumnName = "updated_at")]
    public DateTime UpdatedAt { get; set; }
}
