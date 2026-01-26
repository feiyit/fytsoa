using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产单据明细。
/// 对应表：am_doc_item
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_doc_item")]
public class AmDocItem : Entity
{
    /// <summary>
    /// 单据Id
    /// </summary>
    [Required]
    public long DocId { get; set; } = 0;

    /// <summary>
    /// 行号
    /// </summary>
    [Required]
    public int LineNo { get; set; } = 1;

    /// <summary>
    /// 资产Id（入库新建资产可为0）
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 资产编号（冗余，便于查询）
    /// </summary>
    [StringLength(64)]
    public string? AssetNo { get; set; }

    /// <summary>
    /// 标签码（冗余）
    /// </summary>
    [StringLength(64)]
    public string? TagCode { get; set; }

    /// <summary>
    /// 资产名称（冗余）
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 型号（冗余）
    /// </summary>
    [StringLength(200)]
    public string? Model { get; set; }

    /// <summary>
    /// 分类Id（冗余）
    /// </summary>
    [Required]
    public long CategoryId { get; set; } = 0;

    /// <summary>
    /// 单位
    /// </summary>
    [StringLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [Required]
    public decimal Qty { get; set; } = 1m;

    /// <summary>
    /// 单价（可选）
    /// </summary>
    [Required]
    public decimal Price { get; set; } = 0m;

    /// <summary>
    /// 金额（可选）
    /// </summary>
    [Required]
    public decimal Amount { get; set; } = 0m;

    /// <summary>
    /// 序列号（冗余/可选）
    /// </summary>
    [StringLength(128)]
    public string? SerialNo { get; set; }

    /// <summary>
    /// 仓库Id（行级覆盖）
    /// </summary>
    [Required]
    public long WarehouseId { get; set; } = 0;

    /// <summary>
    /// 库位Id（行级覆盖）
    /// </summary>
    [Required]
    public long BinId { get; set; } = 0;

    /// <summary>
    /// 地点Id（行级覆盖）
    /// </summary>
    [Required]
    public long LocationId { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Remark { get; set; }

    #region 导航属性

    /// <summary>
    /// 单据信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DocId))]
    public AmDoc? DocObj { get; set; }

    /// <summary>
    /// 资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
