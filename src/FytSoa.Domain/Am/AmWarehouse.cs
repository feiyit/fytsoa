using System.ComponentModel.DataAnnotations;
using SqlSugar;
using FytSoa.Domain.Sys;

namespace FytSoa.Domain.Am;

/// <summary>
/// 仓库。
/// 对应表：am_warehouse
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_warehouse")]
public class AmWarehouse : Entity
{
    /// <summary>
    /// 仓库编码
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 仓库名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所在地点Id（am_location.Id）
    /// </summary>
    [Required]
    public long LocationId { get; set; } = 0;

    /// <summary>
    /// 仓库管理员Id（sys_admin.Id）
    /// </summary>
    [Required]
    public long ManagerId { get; set; } = 0;

    /// <summary>
    /// 联系方式
    /// </summary>
    [StringLength(50)]
    public string? Phone { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Summary { get; set; }

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
    /// 所在地点信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LocationId))]
    public AmLocation? LocationObj { get; set; }

    /// <summary>
    /// 仓库管理员信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ManagerId))]
    public SysAdmin? ManagerObj { get; set; }

    #endregion
}
