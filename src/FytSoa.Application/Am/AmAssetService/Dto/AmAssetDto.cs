using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产台账 DTO
/// </summary>
public class AmAssetDto : AppEntity
{
    [Required]
    [StringLength(64)]
    public string AssetNo { get; set; } = string.Empty;

    [StringLength(64)]
    public string? TagCode { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(200)]
    public string? Model { get; set; }

    [StringLength(255)]
    public string? Spec { get; set; }

    public long CategoryId { get; set; } = 0;

    [StringLength(20)]
    public string? Unit { get; set; }

    public decimal Qty { get; set; } = 1m;

    [StringLength(128)]
    public string? SerialNo { get; set; }

    public long VendorId { get; set; } = 0;

    public DateTime? PurchaseDate { get; set; }

    public DateTime? InboundTime { get; set; }

    public DateTime? EnableDate { get; set; }

    public DateTime? WarrantyExpireDate { get; set; }

    public decimal OriginalValue { get; set; } = 0m;

    public decimal TaxValue { get; set; } = 0m;

    public decimal NetBookValue { get; set; } = 0m;

    public byte Status { get; set; } = 1;

    public long WarehouseId { get; set; } = 0;

    public long BinId { get; set; } = 0;

    public long LocationId { get; set; } = 0;

    public long OrgUnitId { get; set; } = 0;

    public long CustodianId { get; set; } = 0;

    public long UserId { get; set; } = 0;

    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 扩展字段(JSON)
    /// </summary>
    public string? ExtJson { get; set; }

    [StringLength(512)]
    public string? Summary { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }

    /// <summary>
    /// 分类信息（sys_code）
    /// </summary>
    public SysCodeSimpleDto? CategoryObj { get; set; }
    
    /// <summary>
    /// 地点信息
    /// </summary>
    public AmLocationDto LocationObj { get; set; }
}
