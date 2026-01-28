using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产单据明细 DTO
/// </summary>
public class AmDocItemDto : AppEntity
{
    public long DocId { get; set; } = 0;
    public int LineNo { get; set; } = 1;
    public long AssetId { get; set; } = 0;

    [StringLength(64)]
    public string? AssetNo { get; set; }

    [StringLength(64)]
    public string? TagCode { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Model { get; set; }

    public long CategoryId { get; set; } = 0;

    [StringLength(20)]
    public string? Unit { get; set; }

    public decimal Qty { get; set; } = 1m;

    public decimal Price { get; set; } = 0m;

    public decimal Amount { get; set; } = 0m;

    [StringLength(128)]
    public string? SerialNo { get; set; }

    public DateTime? WarrantyExpireDate { get; set; }

    public long WarehouseId { get; set; } = 0;
    public long BinId { get; set; } = 0;
    public long LocationId { get; set; } = 0;

    [StringLength(512)]
    public string? Remark { get; set; }
}
