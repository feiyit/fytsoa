using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产盘点计划。
/// 对应表：am_inventory_plan
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_inventory_plan")]
public class AmInventoryPlan : Entity
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    [Required]
    [StringLength(64)]
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>
    /// 盘点名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 盘点范围(JSON)：地点/部门/分类/仓库等
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? ScopeJson { get; set; }

    /// <summary>
    /// 盘点执行人(JSON)：用户Id列表等
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? AssigneeIdsJson { get; set; }

    /// <summary>
    /// 状态：0=草稿,1=进行中,2=已完成,3=已取消
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

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
    /// 盘点明细列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AmInventoryItem.PlanId))]
    public List<AmInventoryItem> Items { get; set; } = new();

    #endregion
}
