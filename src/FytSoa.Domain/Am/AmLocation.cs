using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 地点/位置（层级结构）。
/// 对应表：am_location
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_location")]
public class AmLocation : Entity
{
    /// <summary>
    /// 父节点Id
    /// </summary>
    [Required]
    public long ParentId { get; set; } = 0;

    /// <summary>
    /// 父节点集合(JSON数组，如 [0,1,2])
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = "json")]
    public List<long> ParentIdList { get; set; } = new();

    /// <summary>
    /// 层级
    /// </summary>
    [Required]
    public int Layer { get; set; } = 1;

    /// <summary>
    /// 地点编码
    /// </summary>
    [StringLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 地点名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 类型：park/building/floor/room/custom
    /// </summary>
    [StringLength(32)]
    public string? Type { get; set; }

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
}
