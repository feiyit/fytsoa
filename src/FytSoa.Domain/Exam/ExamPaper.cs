using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Domain.Exam;

/// <summary>
/// 试卷
/// </summary>
[SugarTable("exam_paper")]
public class ExamPaper:Entity
{
    /// <summary>
    /// 年级编号
    /// </summary>
    [Required]
    public long GrandId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(GrandId))]
    public SysCode GrandCode { get; set; } 

    /// <summary>
    /// 学科编号
    /// </summary>
    [Required]
    public long SubjectId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(SubjectId))]
    public SysCode SubjectCode { get; set; } 
    
    /// <summary>
    /// 考试次数
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public int UserNumber { get; set; } = 0;

    /// <summary>
    /// 试卷类型编号
    /// </summary>
    [Required]
    public long TypeId { get; set; }
    
    [Navigate(NavigateType.OneToOne, nameof(TypeId))]
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
    [SugarColumn(IsJson = true)]
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
    [SugarColumn(IsJson = true)]
    public List<QuestionArray> QuestionItem { get; set; } = new ();

    /// <summary>
    /// 发布状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }

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
public class QuestionArray
{
    /// <summary>
    /// 题目编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分数
    /// </summary>
    public decimal Score { get; set; } = 0;
}