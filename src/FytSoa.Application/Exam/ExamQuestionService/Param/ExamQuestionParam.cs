using FytSoa.Common.Param;

namespace FytSoa.Application.Exam.Param;

public class ExamQuestionParam:PageParam
{
    /// <summary>
    /// 学科
    /// </summary>
    public long subject { get; set; } = 0;
    
    /// <summary>
    /// 年级，支持多个
    /// </summary>
    public string grand { get; set; }

    /// <summary>
    /// 根据多个ID查询
    /// </summary>
    public string IdArr { get; set; }
}