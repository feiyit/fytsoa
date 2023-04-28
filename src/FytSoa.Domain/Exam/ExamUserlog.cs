using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.User;
using SqlSugar;

namespace FytSoa.Domain.Exam;

/// <summary>
/// 考试记录
/// </summary>
[SugarTable("exam_userlog")]
public class ExamUserlog:Entity
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
    [Navigate(NavigateType.OneToOne, nameof(UserId))] 
    public Member UserMember { get; set; }

    /// <summary>
    /// 题目内容
    /// </summary>
    [Required]
    [SugarColumn(IsJson = true)]
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

/// <summary>
/// 试题中的题目列表
/// </summary>
public class ExamQuestionUserArray
{
    /// <summary>
    /// 题目编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 原题分数
    /// </summary>
    public decimal SourceScore { get; set; } = 0;

    /// <summary>
    /// 所得分数
    /// </summary>
    public decimal Score { get; set; } = 0;
    
    /// <summary>
    /// 用户做所答案
    /// </summary>
    public string Answer { get; set; }
    
    /// <summary>
    /// 对错
    /// </summary>
    public bool? Judge { get; set; } = null;
}