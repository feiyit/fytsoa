namespace FytSoa.Common.Scheduler.Models;

/// <summary>
/// 兼容原 FytSoa.Quartz.Service.ResultQuartzData
/// </summary>
public class ResultQuartzData
{
    public bool status { get; set; }
    public string message { get; set; } = string.Empty;

    public static ResultQuartzData Ok(string? msg = null) => new()
    {
        status = true,
        message = msg ?? "Success",
    };

    public static ResultQuartzData Fail(string? msg = null) => new()
    {
        status = false,
        message = msg ?? "Failed",
    };
}

