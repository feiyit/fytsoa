using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产折旧台账/配置（单资产维度）。
/// 对应表：am_asset_depreciation
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_asset_depreciation")]
public class AmAssetDepreciation : Entity
{
    /// <summary>
    /// 资产Id
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 折旧方法：0=不折旧,1=直线法,2=双倍余额,3=年数总和(可扩展)
    /// </summary>
    [Required]
    public byte Method { get; set; } = 0;

    /// <summary>
    /// 折旧期(月)
    /// </summary>
    [Required]
    public int LifeMonths { get; set; } = 0;

    /// <summary>
    /// 残值率(%)
    /// </summary>
    [Required]
    public decimal SalvageRate { get; set; } = 0m;

    /// <summary>
    /// 折旧起算日期（date）
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 累计折旧
    /// </summary>
    [Required]
    public decimal AccumAmount { get; set; } = 0m;

    /// <summary>
    /// 最后折旧期间(YYYY-MM)
    /// </summary>
    [StringLength(7)]
    public string? LastPeriod { get; set; }

    /// <summary>
    /// 状态：0=未启用,1=折旧中,2=已停用
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

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

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [StringLength(50)]
    public string? UpdateUser { get; set; }

    #region 导航属性

    /// <summary>
    /// 资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
