using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 折旧计提批次（按月）。
/// 对应表：am_depreciation_run
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_depreciation_run")]
public class AmDepreciationRun : Entity
{
    /// <summary>
    /// 期间(YYYY-MM)
    /// </summary>
    [Required]
    [StringLength(7)]
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 状态：0=草稿,1=已过账/确认
    /// </summary>
    [Required]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 执行人Id
    /// </summary>
    [Required]
    public long RunUserId { get; set; } = 0;

    /// <summary>
    /// 执行时间
    /// </summary>
    [Required]
    public DateTime RunTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 本期折旧合计
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; } = 0m;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(1000)]
    public string? Remark { get; set; }

    #region 导航属性

    /// <summary>
    /// 折旧计提明细列表
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(AmDepreciationRunItem.RunId))]
    public List<AmDepreciationRunItem> Items { get; set; } = new();

    #endregion
}
