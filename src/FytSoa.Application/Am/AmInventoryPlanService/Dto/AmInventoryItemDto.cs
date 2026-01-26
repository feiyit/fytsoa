using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产盘点明细 DTO
/// </summary>
public class AmInventoryItemDto : AppEntity
{
    [Required]
    public long PlanId { get; set; } = 0;

    [Required]
    public long AssetId { get; set; } = 0;

    public long ExpectedLocationId { get; set; } = 0;
    public long ActualLocationId { get; set; } = 0;

    public long ExpectedCustodianId { get; set; } = 0;
    public long ActualCustodianId { get; set; } = 0;

    /// <summary>
    /// 结果：0=正常,1=未盘到,2=盘盈(新增),3=盘亏,4=地点不符,5=责任人不符
    /// </summary>
    public byte Result { get; set; } = 0;

    public DateTime? ScanTime { get; set; }

    public long ScanUserId { get; set; } = 0;

    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 资产信息（详情回显用）
    /// </summary>
    public AmAssetDto? AssetObj { get; set; }
}
