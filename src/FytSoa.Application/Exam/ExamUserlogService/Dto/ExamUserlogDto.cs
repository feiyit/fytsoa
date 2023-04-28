using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Exam;
using FytSoa.Domain.User;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 考试记录
/// </summary>
public class ExamUserlogDto : AppEntity
{
    /// <summary>
    /// 试卷编号
    /// </summary>
    [Required]
    public long PaperId { get; set; }

    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long UserId { get; set; }
    
    /// <summary>
    /// 用户信息
    /// </summary>
    public Member UserMember { get; set; }

    /// <summary>
    /// 题目内容
    /// </summary>
    [Required]
    public List<ExamQuestionUserArray> QuestionItem { get; set; } = new();

    /// <summary>
    /// 耗时分钟
    /// </summary>
    [Required]
    public decimal UseMinutes { get; set; } = 0;

    /// <summary>
    /// 批改完成状态
    /// </summary>
    [Required]
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