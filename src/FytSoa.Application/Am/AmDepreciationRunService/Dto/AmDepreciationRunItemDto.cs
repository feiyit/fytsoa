using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧计提明细 DTO
/// </summary>
public class AmDepreciationRunItemDto : AppEntity
{
    [Required]
    public long RunId { get; set; } = 0;

    [Required]
    public long AssetId { get; set; } = 0;

    public decimal Amount { get; set; } = 0m;

    public decimal AccumAmount { get; set; } = 0m;

    public decimal NetBookValue { get; set; } = 0m;

    [StringLength(512)]
    public string? Remark { get; set; }
}

