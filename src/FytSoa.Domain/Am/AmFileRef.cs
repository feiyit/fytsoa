using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 附件关联（复用 sys_file_info.Id）。
/// 对应表：am_file_ref
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_file_ref")]
public class AmFileRef : Entity
{
    /// <summary>
    /// 业务类型：ASSET/DOC/INVENTORY/MAINTENANCE等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    /// <summary>
    /// 业务Id
    /// </summary>
    [Required]
    public long BizId { get; set; } = 0;

    /// <summary>
    /// 文件Id（sys_file_info.Id）
    /// </summary>
    [Required]
    public long FileId { get; set; } = 0;

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 0;

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
}
