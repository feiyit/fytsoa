using FytSoa.Common.Param;

namespace FytSoa.Application.Exam.Param;

public class ExamKnowledgeParam:PageParam
{
    /// <summary>
    /// 年级编号
    /// </summary>
    public long GradeId { get; set; } = 0;
    
    /// <summary>
    /// 分类编号
    /// </summary>
    public long CategoryId { get; set; } = 0;
}