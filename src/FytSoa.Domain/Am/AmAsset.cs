using System.ComponentModel.DataAnnotations;
using SqlSugar;
using FytSoa.Domain.Sys;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产台账。
/// 对应表：am_asset
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_asset")]
public class AmAsset : Entity
{
    /// <summary>
    /// 资产编号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string AssetNo { get; set; } = string.Empty;

    /// <summary>
    /// 标签码/二维码内容（用于扫码/输入）
    /// </summary>
    [StringLength(64)]
    public string? TagCode { get; set; }

    /// <summary>
    /// 资产名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 品牌
    /// </summary>
    [StringLength(100)]
    public string? Brand { get; set; }

    /// <summary>
    /// 型号
    /// </summary>
    [StringLength(200)]
    public string? Model { get; set; }

    /// <summary>
    /// 规格
    /// </summary>
    [StringLength(255)]
    public string? Spec { get; set; }

    /// <summary>
    /// 分类Id（sys_code.Id）
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
    /// 序列号/IMEI等
    /// </summary>
    [StringLength(128)]
    public string? SerialNo { get; set; }

    /// <summary>
    /// 供应商Id（am_vendor.Id）
    /// </summary>
    [Required]
    public long VendorId { get; set; } = 0;

    /// <summary>
    /// 购置日期（date）
    /// </summary>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime? InboundTime { get; set; }

    /// <summary>
    /// 启用日期（date）
    /// </summary>
    public DateTime? EnableDate { get; set; }

    /// <summary>
    /// 保修到期日（date）
    /// </summary>
    public DateTime? WarrantyExpireDate { get; set; }

    /// <summary>
    /// 原值
    /// </summary>
    [Required]
    public decimal OriginalValue { get; set; } = 0m;

    /// <summary>
    /// 税额（可选）
    /// </summary>
    [Required]
    public decimal TaxValue { get; set; } = 0m;

    /// <summary>
    /// 净值/账面价值（可选，或由折旧计算）
    /// </summary>
    [Required]
    public decimal NetBookValue { get; set; } = 0m;

    /// <summary>
    /// 状态：1=在库,2=在用,3=借出,4=维修中,5=闲置,6=在途,7=处置中,8=已处置
    /// </summary>
    [Required]
    public byte Status { get; set; } = 1;

    /// <summary>
    /// 所在仓库Id（在库/在途场景）
    /// </summary>
    [Required]
    public long WarehouseId { get; set; } = 0;

    /// <summary>
    /// 库位Id
    /// </summary>
    [Required]
    public long BinId { get; set; } = 0;

    /// <summary>
    /// 地点Id（am_location.Id）
    /// </summary>
    [Required]
    public long LocationId { get; set; } = 0;

    /// <summary>
    /// 所属部门Id（sys_org_unit.Id）
    /// </summary>
    [Required]
    public long OrgUnitId { get; set; } = 0;

    /// <summary>
    /// 责任人Id（sys_admin.Id 或 员工Id，按现有体系）
    /// </summary>
    [Required]
    public long CustodianId { get; set; } = 0;

    /// <summary>
    /// 使用人Id（sys_admin.Id 或 员工Id，按现有体系）
    /// </summary>
    [Required]
    public long UserId { get; set; } = 0;

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 扩展字段(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? ExtJson { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Summary { get; set; }

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
    /// 分类信息（sys_code）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public SysCode? CategoryObj { get; set; }

    /// <summary>
    /// 供应商信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(VendorId))]
    public AmVendor? VendorObj { get; set; }

    /// <summary>
    /// 仓库信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(WarehouseId))]
    public AmWarehouse? WarehouseObj { get; set; }

    /// <summary>
    /// 库位信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(BinId))]
    public AmWarehouseBin? BinObj { get; set; }

    /// <summary>
    /// 地点信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LocationId))]
    public AmLocation? LocationObj { get; set; }

    /// <summary>
    /// 部门信息（sys_org_unit）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrgUnitId))]
    public SysOrgUnit? OrgUnitObj { get; set; }

    /// <summary>
    /// 责任人信息（sys_admin）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CustodianId))]
    public SysAdmin? CustodianObj { get; set; }

    /// <summary>
    /// 使用人信息（sys_admin）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public SysAdmin? UserObj { get; set; }

    /// <summary>
    /// 资产变更历史（留痕）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AmAssetHistory.AssetId))]
    public List<AmAssetHistory> Histories { get; set; } = new();

    #endregion
}
