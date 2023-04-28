using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Exam;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 试卷
/// </summary>
public class ExamPaperDto : AppEntity
{
    /// <summary>
    /// 考试次数
    /// </summary>
    public int UserNumber { get; set; } = 0;

    /// <summary>
    /// 年级编号
    /// </summary>
    [Required]
    public long GrandId { get; set; }
    
    public SysCode GrandCode { get; set; } 

    /// <summary>
    /// 学科编号
    /// </summary>
    [Required]
    public long SubjectId { get; set; }
    
    public SysCode SubjectCode { get; set; } 

    /// <summary>
    /// 试卷类型编号
    /// </summary>
    [Required]
    public long TypeId { get; set; }
    
    public SysCode TypeCode { get; set; } 

    /// <summary>
    /// 考试开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 考试结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 试卷名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 题目打乱
    /// </summary>
    public List<int> AntiCheating { get; set; } = new ();

    /// <summary>
    /// 考试时长-分钟
    /// </summary>
    [Required]
    public decimal MinutesLength { get; set; } = 0;

    /// <summary>
    /// 考试题内容
    /// </summary>
    [Required]
    public List<QuestionArray> QuestionItem { get; set; } = new ();

    /// <summary>
    /// 发布状态
    /// </summary>
    public bool Status { get; set; } = false;

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