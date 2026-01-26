using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 仓库库位。
/// 对应表：am_warehouse_bin
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_warehouse_bin")]
public class AmWarehouseBin : Entity
{
    /// <summary>
    /// 仓库Id（am_warehouse.Id）
    /// </summary>
    [Required]
    public long WarehouseId { get; set; } = 0;

    /// <summary>
    /// 库位编码
    /// </summary>
    [StringLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

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
    /// 所属仓库信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(WarehouseId))]
    public AmWarehouse? WarehouseObj { get; set; }

    #endregion
}
