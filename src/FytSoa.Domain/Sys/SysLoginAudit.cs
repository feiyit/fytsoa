#nullable enable
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 登录审计日志。
/// </summary>
[SugarTable("sys_login_audit")]
public class SysLoginAudit
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [Required]
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; }

    [SugarColumn(ColumnName = "user_id")]
    public long? UserId { get; set; }

    [Required]
    [StringLength(100)]
    [SugarColumn(ColumnName = "user_name")]
    public string UserName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "is_success")]
    public bool IsSuccess { get; set; }

    [StringLength(200)]
    [SugarColumn(ColumnName = "reason")]
    public string? Reason { get; set; }

    [StringLength(64)]
    [SugarColumn(ColumnName = "ip_address")]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    [SugarColumn(ColumnName = "user_agent")]
    public string? UserAgent { get; set; }

    [SugarColumn(ColumnName = "occurred_at")]
    public DateTime OccurredAt { get; set; }
}
