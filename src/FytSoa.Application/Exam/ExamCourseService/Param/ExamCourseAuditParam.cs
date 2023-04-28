namespace FytSoa.Application.Exam.Param;

/// <summary>
/// 审核
/// </summary>
public class ExamCourseAuditParam
{
    public List<long> Ids { get; set; }

    public bool Audit { get; set; }
}