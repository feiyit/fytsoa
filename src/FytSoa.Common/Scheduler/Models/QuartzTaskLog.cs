namespace FytSoa.Common.Scheduler.Models;

/// <summary>
/// 执行日志模型（兼容原 FytSoa.Quartz.Model.QuarzTasklog 字段）。
/// </summary>
public class QuartzTaskLog
{
    public string TaskName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public DateTime BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Msg { get; set; } = string.Empty;

    public int Id { get; set; }
    public DateTime? TimeFlag { get; set; }
    public DateTime? ChangeTime { get; set; }
}

