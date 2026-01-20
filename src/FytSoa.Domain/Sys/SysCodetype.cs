using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 字典分类信息
/// </summary>
[SugarTable("sys_codetype")]
public class SysCodetype:Entity
{
    /// <summary>
    /// 父节点
    /// </summary>
    [Required]
    public long ParentId { get; set; }

    /// <summary>
    /// 父级
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<string> ParentIdList { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [Required]
    public int Layer { get; set; } = 1;

    /// <summary>
    /// 分类名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 分类标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 1=系统 2=商城
    /// </summary>
    [Required]
    public int Types { get; set; } = 1;

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 是否系统内置集成
    /// </summary>
    [Required]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}