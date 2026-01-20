#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 用户任用信息。
/// </summary>
[SugarTable("sys_employment")]
public class SysEmployment
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; }

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; }

    [Required]
    [SugarColumn(ColumnName = "user_id")]
    public long UserId { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public SysAdmin User { get; set; }

    [Required]
    [SugarColumn(ColumnName = "org_id")]
    public long OrgId { get; set; }
    
    /// <summary>
    /// 机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrgId))]
    public SysOrgUnit Org { get; set; }

    [Required]
    [SugarColumn(ColumnName = "position_id")]
    public long PositionId { get; set; }
    
    /// <summary>
    /// 岗位
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PositionId))]
    public SysPosition Position { get; set; }

    [SugarColumn(ColumnName = "is_primary")]
    public bool IsPrimary { get; set; } = true;

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
