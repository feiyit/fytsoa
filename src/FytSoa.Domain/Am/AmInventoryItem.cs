using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产盘点明细/结果。
/// 对应表：am_inventory_item
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_inventory_item")]
public class AmInventoryItem : Entity
{
    /// <summary>
    /// 盘点计划Id
    /// </summary>
    [Required]
    public long PlanId { get; set; } = 0;

    /// <summary>
    /// 资产Id
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 系统地点Id
    /// </summary>
    [Required]
    public long ExpectedLocationId { get; set; } = 0;

    /// <summary>
    /// 盘点地点Id
    /// </summary>
    [Required]
    public long ActualLocationId { get; set; } = 0;

    /// <summary>
    /// 系统责任人Id
    /// </summary>
    [Required]
    public long ExpectedCustodianId { get; set; } = 0;

    /// <summary>
    /// 盘点责任人Id
    /// </summary>
    [Required]
    public long ActualCustodianId { get; set; } = 0;

    /// <summary>
    /// 结果：0=正常,1=未盘到,2=盘盈(新增),3=盘亏,4=地点不符,5=责任人不符
    /// </summary>
    [Required]
    public byte Result { get; set; } = 0;

    /// <summary>
    /// 盘点时间
    /// </summary>
    public DateTime? ScanTime { get; set; }

    /// <summary>
    /// 盘点人Id
    /// </summary>
    [Required]
    public long ScanUserId { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Remark { get; set; }

    #region 导航属性

    /// <summary>
    /// 盘点计划信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PlanId))]
    public AmInventoryPlan? PlanObj { get; set; }

    /// <summary>
    /// 资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
