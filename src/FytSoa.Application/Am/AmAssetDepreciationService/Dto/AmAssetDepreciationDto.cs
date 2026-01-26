using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产折旧台账/配置 DTO
/// </summary>
public class AmAssetDepreciationDto : AppEntity
{
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 折旧方法：0=不折旧,1=直线法,2=双倍余额,3=年数总和
    /// </summary>
    public byte Method { get; set; } = 0;

    public int LifeMonths { get; set; } = 0;

    public decimal SalvageRate { get; set; } = 0m;

    public DateTime? StartDate { get; set; }

    public decimal AccumAmount { get; set; } = 0m;

    [StringLength(7)]
    public string? LastPeriod { get; set; }

    /// <summary>
    /// 状态：0=未启用,1=折旧中,2=已停用
    /// </summary>
    public byte Status { get; set; } = 0;

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }
}

