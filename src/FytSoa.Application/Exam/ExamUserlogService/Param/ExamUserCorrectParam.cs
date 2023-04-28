using FytSoa.Domain.Exam;

namespace FytSoa.Application.Exam.Param;

/// <summary>
/// 批改参数
/// </summary>
public class ExamUserCorrectParam
{
    /// <summary>
    /// 唯一编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 批改结果
    /// </summary>
    public List<ExamQuestionUserArray> QuestionItem { get; set; } = new();
}