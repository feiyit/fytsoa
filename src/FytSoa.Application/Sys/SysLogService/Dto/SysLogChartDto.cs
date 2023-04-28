namespace FytSoa.Application.Sys;

/// <summary>
/// 15日 图表
/// </summary>
public class SysLogChartDto
{
    /// <summary>
    /// 日期
    /// </summary>
    public List<string> Time { get; set; } = new();

    /// <summary>
    /// 数量
    /// </summary>
    public List<List<int>> Count { get; set; } = new();
}