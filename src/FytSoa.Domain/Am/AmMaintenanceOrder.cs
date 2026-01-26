using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 维修/保养工单。
/// 对应表：am_maintenance_order
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_maintenance_order")]
public class AmMaintenanceOrder : Entity
{
    /// <summary>
    /// 工单编号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 类型：1=报修,2=保养
    /// </summary>
    [Required]
    public byte Type { get; set; } = 1;

    /// <summary>
    /// 状态：0=草稿,1=待受理,2=已指派,3=处理中,4=已完成,5=已关闭,6=已取消
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 优先级：1=高,2=中,3=低
    /// </summary>
    [Required]
    public byte Priority { get; set; } = 2;

    /// <summary>
    /// 资产Id
    /// </summary>
    [Required]
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述/故障现象
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 报修人Id
    /// </summary>
    [Required]
    public long ReportUserId { get; set; } = 0;

    /// <summary>
    /// 报修时间
    /// </summary>
    [Required]
    public DateTime ReportTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 指派人/维修负责人Id
    /// </summary>
    [Required]
    public long AssignUserId { get; set; } = 0;

    /// <summary>
    /// 指派时间
    /// </summary>
    public DateTime? AssignTime { get; set; }

    /// <summary>
    /// 维修供应商Id（可选）
    /// </summary>
    [Required]
    public long VendorId { get; set; } = 0;

    /// <summary>
    /// 开始处理时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? FinishTime { get; set; }

    /// <summary>
    /// 费用（可选）
    /// </summary>
    [Required]
    public decimal Cost { get; set; } = 0m;

    /// <summary>
    /// 处理结果
    /// </summary>
    [StringLength(1000)]
    public string? Result { get; set; }

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
    /// 资产信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(AssetId))]
    public AmAsset? AssetObj { get; set; }

    #endregion
}
