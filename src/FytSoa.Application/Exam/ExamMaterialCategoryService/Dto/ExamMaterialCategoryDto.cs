using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 素材分类
/// </summary>
public class ExamMaterialCategoryDto : AppEntity
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 分类英文名称
    /// </summary>
    [Required]
    public string EnName { get; set; }

    /// <summary>
    /// 父级编号
    /// </summary>
    [Required]
    public long ParentId { get; set; }

    /// <summary>
    /// 父节点集合组
    /// </summary>
    public List<string> ParentIdList { get; set; } = new();

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
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