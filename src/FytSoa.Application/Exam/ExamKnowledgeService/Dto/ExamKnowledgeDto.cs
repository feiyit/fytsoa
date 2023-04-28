using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;
using FytSoa.Domain.Exam;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 知识库
/// </summary>
public class ExamKnowledgeDto : AppEntity
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
    /// 知识库分类编号
    /// </summary>
    [Required]
    public long CategoryId { get; set; }
    
    /// <summary>
    /// 分类信息
    /// </summary>
    public ExamKnowledgeCategoryDto Category { get; set; } 

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 封面
    /// </summary>
    public string Cover { get; set; }

    /// <summary>
    /// 文档
    /// </summary>
    public string Document { get; set; }

    /// <summary>
    /// 页数
    /// </summary>
    public int PageCount { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
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