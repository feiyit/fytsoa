namespace FytSoa.Application.Sys;

public class SysMessageTotalDto
{
    /// <summary>
    /// 消息总数
    /// </summary>
    public int AllCount { get; set; } = 0;

    /// <summary>
    /// 未读总数
    /// </summary>
    public int UnReadCount { get; set; } = 0;

    /// <summary>
    /// 回收站总数
    /// </summary>
    public int RecycleCount { get; set; } = 0;
}