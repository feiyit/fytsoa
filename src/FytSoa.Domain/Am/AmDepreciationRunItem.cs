using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 折旧计提明细（单资产）。
/// 对应表：am_depreciation_run_item
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_depreciation_run_item")]
public class AmDepreciationRunItem : Entity
{
    /// <summary>
    /// 折旧批次Id
    /// </summary>
    [Required]
    public long RunId { get; set; } = 0;

    /// <summary>
    /// 资产Id
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 本期折旧
    /// </summary>
    [Required]
    public decimal Amount { get; set; } = 0m;

    /// <summary>
    /// 累计折旧(计提后)
    /// </summary>
    [Required]
    public decimal AccumAmount { get; set; } = 0m;

    /// <summary>
    /// 账面净值(计提后)
    /// </summary>
    [Required]
    public decimal NetBookValue { get; set; } = 0m;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Remark { get; set; }

    #region 导航属性

    /// <summary>
    /// 折旧批次信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RunId))]
    public AmDepreciationRun? RunObj { get; set; }

    /// <summary>
    /// 资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
