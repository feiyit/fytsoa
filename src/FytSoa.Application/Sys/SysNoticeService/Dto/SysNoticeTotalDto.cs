namespace FytSoa.Application.Sys;

/// <summary>
/// 统计信息
/// </summary>
public class SysNoticeTotalDto
{
    /// <summary>
    /// 未读
    /// </summary>
    public int Unread { get; set; } = 0;
    
    /// <summary>
    /// 草稿
    /// </summary>
    public int Draft { get; set; } = 0;
    
    /// <summary>
    /// 删除
    /// </summary>
    public int Delete { get; set; } = 0;
    
    /// <summary>
    /// 存档
    /// </summary>
    public int Archive { get; set; } = 0;
}