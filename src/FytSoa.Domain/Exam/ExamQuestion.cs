using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Domain.Exam;

/// <summary>
/// 考试题
/// </summary>
[SugarTable("exam_question")]
public class ExamQuestion:Entity
{
    /// <summary>
    /// 年级编号
    /// </summary>
    [Required]
    public long GrandId { get; set; } = 0;
    
    [Navigate(NavigateType.OneToOne, nameof(GrandId))]
    public SysCode GrandCode { get; set; } 


    /// <summary>
    /// 学科编号
    /// </summary>
    [Required]
    public long SubjectId { get; set; } = 0;
    
    [Navigate(NavigateType.OneToOne, nameof(SubjectId))] 
    public SysCode SubjectCode { get; set; } 

    /// <summary>
    /// 类型1=单选2=多选3=判断4=填空5=解答
    /// </summary>
    public int Type { get; set; } = 1;

    /// <summary>
    /// 题目
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 题干
    /// </summary>
    [Required]
    [SugarColumn(IsJson = true)]
    public List<ExamQuestionItem> SubjectItem { get; set; } = new();

    /// <summary>
    /// 答案
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Answer { get; set; }

    /// <summary>
    /// 解析
    /// </summary>
    public string Parsing { get; set; }

    /// <summary>
    /// 分数
    /// </summary>
    [Required]
    public decimal Score { get; set; }

    /// <summary>
    /// 难度
    /// </summary>
    [Required]
    public int Difficulty { get; set; } = 1;

    /// <summary>
    /// 关联知识点
    /// </summary>
    public string KnowledgePoint { get; set; }

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
/// 题目项
/// </summary>
public class ExamQuestionItem
{
    /// <summary>
    /// 标识
    /// </summary>
    public string Tag { get; set; }
    
    /// <summary>
    /// 项内容
    /// </summary>
    public string Value { get; set; }
}