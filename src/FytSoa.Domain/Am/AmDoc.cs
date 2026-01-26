using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产业务单据（通用）。
/// 对应表：am_doc
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_doc")]
public class AmDoc : Entity
{
    /// <summary>
    /// 单据类型：INBOUND/OUTBOUND/RETURN/TRANSFER/CHANGE/DISPOSE/INV_ADJUST
    /// </summary>
    [Required]
    [StringLength(20)]
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// 子类型：例如 OUTBOUND(ISSUE/BORROW), RETURN(RETURN/BACK), DISPOSE(SCRAP/SELL/LOSS)等
    /// </summary>
    [StringLength(20)]
    public string? SubType { get; set; }

    /// <summary>
    /// 单据编号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string DocNo { get; set; } = string.Empty;

    /// <summary>
    /// 状态：0=草稿,1=待审批,2=已通过,3=已驳回,4=执行中,5=已完成,6=已取消
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 关联工作流实例Id（如启用）
    /// </summary>
    [Required]
    public long WfInstanceId { get; set; } = 0;

    /// <summary>
    /// 关联单据Id（如归还关联借用单、冲销关联原单等）
    /// </summary>
    [Required]
    public long RefDocId { get; set; } = 0;

    /// <summary>
    /// 发起人Id
    /// </summary>
    [Required]
    public long ApplyUserId { get; set; } = 0;

    /// <summary>
    /// 发起时间
    /// </summary>
    public DateTime? ApplyTime { get; set; }

    /// <summary>
    /// 审批人Id
    /// </summary>
    [Required]
    public long ApproveUserId { get; set; } = 0;

    /// <summary>
    /// 审批时间
    /// </summary>
    public DateTime? ApproveTime { get; set; }

    /// <summary>
    /// 供应商Id（入库/维修等可用）
    /// </summary>
    [Required]
    public long VendorId { get; set; } = 0;

    /// <summary>
    /// 来源仓库Id
    /// </summary>
    [Required]
    public long FromWarehouseId { get; set; } = 0;

    /// <summary>
    /// 目标仓库Id
    /// </summary>
    [Required]
    public long ToWarehouseId { get; set; } = 0;

    /// <summary>
    /// 来源地点Id
    /// </summary>
    [Required]
    public long FromLocationId { get; set; } = 0;

    /// <summary>
    /// 目标地点Id
    /// </summary>
    [Required]
    public long ToLocationId { get; set; } = 0;

    /// <summary>
    /// 来源部门Id
    /// </summary>
    [Required]
    public long FromOrgUnitId { get; set; } = 0;

    /// <summary>
    /// 目标部门Id
    /// </summary>
    [Required]
    public long ToOrgUnitId { get; set; } = 0;

    /// <summary>
    /// 来源责任人Id
    /// </summary>
    [Required]
    public long FromCustodianId { get; set; } = 0;

    /// <summary>
    /// 目标责任人Id
    /// </summary>
    [Required]
    public long ToCustodianId { get; set; } = 0;

    /// <summary>
    /// 来源使用人Id
    /// </summary>
    [Required]
    public long FromUserId { get; set; } = 0;

    /// <summary>
    /// 目标使用人Id
    /// </summary>
    [Required]
    public long ToUserId { get; set; } = 0;

    /// <summary>
    /// 金额汇总
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; } = 0m;

    /// <summary>
    /// 业务时间（入库/领用/调拨等发生时间）
    /// </summary>
    public DateTime? BizTime { get; set; }

    /// <summary>
    /// 到期时间（借用归还截止/调拨签收截止等）
    /// </summary>
    public DateTime? DueTime { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(1000)]
    public string? Remark { get; set; }

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
    /// 单据明细列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AmDocItem.DocId))]
    public List<AmDocItem> Items { get; set; } = new();

    #endregion
}
