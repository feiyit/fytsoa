using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 附件关联 DTO（关联 sys_file_info.Id）
/// </summary>
public class AmFileRefDto : AppEntity
{
    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    [Required]
    public long BizId { get; set; } = 0;

    [Required]
    public long FileId { get; set; } = 0;

    public int Sort { get; set; } = 0;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }
}

