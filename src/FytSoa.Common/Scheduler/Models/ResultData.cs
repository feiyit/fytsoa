namespace FytSoa.Common.Scheduler.Models;

/// <summary>
/// 兼容原 FytSoa.Quartz.Service.ResultData`1
/// </summary>
public class ResultData<T>
{
    public int total { get; set; }
    public List<T> data { get; set; } = new();
}

