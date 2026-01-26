using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 维修/保养工单 DTO
/// </summary>
public class AmMaintenanceOrderDto : AppEntity
{
    [Required]
    [StringLength(64)]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 类型：1=报修,2=保养
    /// </summary>
    public byte Type { get; set; } = 1;

    /// <summary>
    /// 状态：0=草稿,1=待受理,2=已指派,3=处理中,4=已完成,5=已关闭,6=已取消
    /// </summary>
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 优先级：1=高,2=中,3=低
    /// </summary>
    public byte Priority { get; set; } = 2;

    public long AssetId { get; set; } = 0;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long ReportUserId { get; set; } = 0;
    public DateTime ReportTime { get; set; } = DateTime.Now;

    public long AssignUserId { get; set; } = 0;
    public DateTime? AssignTime { get; set; }

    public long VendorId { get; set; } = 0;

    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }

    public decimal Cost { get; set; } = 0m;

    [StringLength(1000)]
    public string? Result { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }
}

