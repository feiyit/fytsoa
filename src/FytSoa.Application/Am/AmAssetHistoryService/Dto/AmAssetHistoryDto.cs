using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产变更历史 DTO
/// </summary>
public class AmAssetHistoryDto : AppEntity
{
    [Required]
    public long AssetId { get; set; } = 0;

    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    public long BizId { get; set; } = 0;

    [Required]
    [StringLength(32)]
    public string Operation { get; set; } = string.Empty;

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    [StringLength(512)]
    public string? Remark { get; set; }

    public long OperatorId { get; set; } = 0;

    public DateTime OperateTime { get; set; } = DateTime.Now;
}

