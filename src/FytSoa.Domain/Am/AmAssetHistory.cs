using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产变更历史（留痕）。
/// 对应表：am_asset_history
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_asset_history")]
public class AmAssetHistory : Entity
{
    /// <summary>
    /// 资产Id
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 业务类型：INBOUND/OUTBOUND/RETURN/TRANSFER/CHANGE/DISPOSE/INVENTORY/MAINTENANCE等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string BizType { get; set; } = string.Empty;

    /// <summary>
    /// 业务单据Id
    /// </summary>
    [Required]
    public long BizId { get; set; } = 0;

    /// <summary>
    /// 操作：CREATE/UPDATE/STATUS/LOCATION/OWNER等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 变更前(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? BeforeJson { get; set; }

    /// <summary>
    /// 变更后(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? AfterJson { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 操作人Id（sys_admin.Id）
    /// </summary>
    [Required]
    public long OperatorId { get; set; } = 0;

    /// <summary>
    /// 操作时间
    /// </summary>
    [Required]
    public DateTime OperateTime { get; set; } = DateTime.Now;

    #region 导航属性

    /// <summary>
    /// 关联资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
