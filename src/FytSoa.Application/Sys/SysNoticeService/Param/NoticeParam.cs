using FytSoa.Common.Param;

namespace FytSoa.Application.Sys.Param;

public class NoticeParam:PageParam
{
    /// <summary>
    /// 未读=1 已读=2  全部=0
    /// </summary>
    public int ReadStatus { get; set; } = 0;
}

/// <summary>
/// 对状态操作的参数，例如 草稿、删除、存档
/// </summary>
public class NoticeStatusParam
{
    /// <summary>
    /// 通知ID集合
    /// </summary>
    public List<long> Ids { get; set; }

    /// <summary>
    /// 1=草稿2=存档3=删除
    /// </summary>
    public int Status { get; set; }
}

