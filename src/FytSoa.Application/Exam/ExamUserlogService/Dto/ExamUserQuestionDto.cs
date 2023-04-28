using FytSoa.Application.User;
using FytSoa.Domain.Exam;
using FytSoa.Domain.User;

namespace FytSoa.Application.Exam;

/// <summary>
/// 查询用户以及试卷信息
/// </summary>
public class ExamUserQuestionDto
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public MemberDto User { get; set; }
    
    /// <summary>
    /// 试题信息
    /// </summary>
    public ExamPaperDto Paper { get; set; }
    
    /// <summary>
    /// 用户考试信息
    /// </summary>
    public ExamUserlogDto UserLog { get; set; }
    
    /// <summary>
    /// 试卷信息
    /// </summary>
    public List<ExamQuestionDto> Question { get; set; }
}