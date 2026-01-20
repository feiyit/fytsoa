namespace FytSoa.Common.Param;

/// <summary>
/// 上下移动参数
/// </summary>
public class SortParam
{
    /// <summary>
    /// 当前ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public long Parent { get; set; }

    /// <summary>
    /// 排序方式 0=向上  1=向下
    /// </summary>
    public int Type { get; set; }
}