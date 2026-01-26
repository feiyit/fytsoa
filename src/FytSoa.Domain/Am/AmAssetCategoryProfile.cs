using System.ComponentModel.DataAnnotations;
using SqlSugar;
using FytSoa.Domain.Sys;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产分类扩展信息（折旧默认值等）。
/// 分类本身复用 sys_codetype（建议根节点 Code=AM_ASSET_CATEGORY）。
/// 对应表：am_asset_category_profile
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_asset_category_profile")]
public class AmAssetCategoryProfile : Entity
{
    /// <summary>
    /// 分类Id（sys_codetype.Id）
    /// </summary>
    [Required]
    public long CategoryId { get; set; } = 0;

    /// <summary>
    /// 默认折旧方法：0=不折旧,1=直线法,2=双倍余额,3=年数总和(可扩展)
    /// </summary>
    [Required]
    public byte DepMethod { get; set; } = 0;

    /// <summary>
    /// 默认折旧期（月）
    /// </summary>
    [Required]
    public int DepLifeMonths { get; set; } = 0;

    /// <summary>
    /// 默认残值率(%)
    /// </summary>
    [Required]
    public decimal SalvageRate { get; set; } = 0m;

    /// <summary>
    /// 分类扩展配置(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? ExtJson { get; set; }

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
    /// 分类信息（sys_codetype）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public SysCode? CategoryObj { get; set; }

    #endregion
}
