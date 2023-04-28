using FytSoa.Common.Param;

namespace FytSoa.Application.Exam.Param;

public class ExamCourseSearchParam:PageParam
{
    /// <summary>
    /// 年级
    /// </summary>
    public long GradeId { get; set; } = 0;
    
    /// <summary>
    /// 学科
    /// </summary>
    public long SubjectId { get; set; } = 0;
    
    /// <summary>
    /// 难度
    /// </summary>
    public long Difficulty { get; set; } = 0;

    /// <summary>
    /// 属性
    /// </summary>
    public int Attr { get; set; } = 0;
    
    
    /// <summary>
    /// 审核状态
    /// </summary>
    public string Audit { get; set; }
    
    /// <summary>
    /// 讲师
    /// </summary>
    public long TeacherId { get; set; } = 0;
}