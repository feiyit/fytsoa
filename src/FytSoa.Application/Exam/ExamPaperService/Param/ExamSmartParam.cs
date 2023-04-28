namespace FytSoa.Application.Exam.Param;

/// <summary>
/// 智能组卷参数
/// </summary>
public class ExamSmartParam
{
    /// <summary>
    /// 年级编号
    /// </summary>
    public long SmartGrand { get; set; }
    
    /// <summary>
    /// 学科编号
    /// </summary>
    public long SmartSubject { get; set; }
    
    /// <summary>
    /// 单选题数量
    /// </summary>
    public int Single { get; set; } = 0;
    
    /// <summary>
    /// 多选题数量
    /// </summary>
    public int Multiple { get; set; } = 0;
    
    /// <summary>
    /// 判断题数量
    /// </summary>
    public int Judge { get; set; } = 0;
    
    /// <summary>
    /// 填空题数量
    /// </summary>
    public int GapFilling { get; set; } = 0;
    
    /// <summary>
    /// 解答题数量
    /// </summary>
    public int Explain { get; set; } = 0;

    /// <summary>
    /// 难度
    /// </summary>
    public int Difficulty { get; set; } = 0;
}