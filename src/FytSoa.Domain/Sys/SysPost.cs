using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 岗位表
/// </summary>
[SugarTable("sys_post")]
public class SysPost:Entity
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Name { get; set; }

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 岗位状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 删除状态
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 备注信息
    /// </summary>
    public string Summary { get; set; }

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