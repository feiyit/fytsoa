using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产业务单据 DTO
/// </summary>
public class AmDocDto : AppEntity
{
    [Required]
    [StringLength(20)]
    public string DocType { get; set; } = string.Empty;

    [StringLength(20)]
    public string? SubType { get; set; }

    [Required]
    [StringLength(64)]
    public string DocNo { get; set; } = string.Empty;

    public byte Status { get; set; } = 0;

    public long WfInstanceId { get; set; } = 0;
    public long RefDocId { get; set; } = 0;

    public long ApplyUserId { get; set; } = 0;
    public DateTime? ApplyTime { get; set; }

    public long ApproveUserId { get; set; } = 0;
    public DateTime? ApproveTime { get; set; }

    public long VendorId { get; set; } = 0;

    public long FromWarehouseId { get; set; } = 0;
    public long ToWarehouseId { get; set; } = 0;
    public long FromLocationId { get; set; } = 0;
    public long ToLocationId { get; set; } = 0;
    public long FromOrgUnitId { get; set; } = 0;
    public long ToOrgUnitId { get; set; } = 0;
    public long FromCustodianId { get; set; } = 0;
    public long ToCustodianId { get; set; } = 0;
    public long FromUserId { get; set; } = 0;
    public long ToUserId { get; set; } = 0;

    public decimal TotalAmount { get; set; } = 0m;

    public DateTime? BizTime { get; set; }
    public DateTime? DueTime { get; set; }

    public bool IsDel { get; set; } = false;

    [StringLength(1000)]
    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }

    /// <summary>
    /// 单据明细
    /// </summary>
    public List<AmDocItemDto> Items { get; set; } = new();
}

