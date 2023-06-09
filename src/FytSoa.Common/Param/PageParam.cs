namespace FytSoa.Common.Param;

/// <summary>
/// 分页参数
/// </summary>
public class PageParam:CommonParam
{
    /// <summary>
    /// 当前页
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页多少条
    /// </summary>
    public int Limit { get; set; } = 10;
        
}
    
/// <summary>
/// 分页参数
/// </summary>
public class PageParam<T>
{
    /// <summary>
    /// 当前页
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页多少条
    /// </summary>
    public int Limit { get; set; } = 15;
        
    /// <summary>
    /// 查询条件
    /// </summary>
    public T Filter { get; set; }
}