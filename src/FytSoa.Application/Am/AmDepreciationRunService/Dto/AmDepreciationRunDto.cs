using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧计提批次 DTO
/// </summary>
public class AmDepreciationRunDto : AppEntity
{
    [Required]
    [StringLength(7)]
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 状态：0=草稿,1=已过账/确认
    /// </summary>
    public byte Status { get; set; } = 0;

    public long RunUserId { get; set; } = 0;

    public DateTime RunTime { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; } = 0m;

    [StringLength(1000)]
    public string? Remark { get; set; }

    /// <summary>
    /// 明细（详情页返回）
    /// </summary>
    public List<AmDepreciationRunItemDto> Items { get; set; } = new();
}

