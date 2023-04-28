using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 知识库分类
/// </summary>
public class ExamKnowledgeCategoryDto : AppEntity
{
    /// <summary>
    /// 年级编号
    /// </summary>
    [Required]
    public long GradeId { get; set; }
    
    /// <summary>
    /// 年级信息
    /// </summary>
    public SysCodeDto GrandCode { get; set; } 

    /// <summary>
    /// 分类名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// 分类父级
    /// </summary>
    [Required]
    public long ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

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