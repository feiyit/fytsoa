namespace FytSoa.Application.Exam.Param;

/// <summary>
/// 重命名
/// </summary>
public class ExamMaterialRenameParam
{
    /// <summary>
    /// 资源编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 新的文件名
    /// </summary>
    public string Name { get; set; }
}