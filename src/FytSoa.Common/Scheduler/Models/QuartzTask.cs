using System.Text.Json.Serialization;

namespace FytSoa.Common.Scheduler.Models;

/// <summary>
/// 任务模型（兼容原 FytSoa.Quartz.Model.QuarzTask 的字段命名）。
/// </summary>
public class QuartzTask
{
    public string TaskName { get; set; } = string.Empty;
    public string GroupName { get; set; } = "default";
    /// <summary>
    /// Quartz Cron 表达式
    /// </summary>
    public string Interval { get; set; } = "0 0 * * * ?";
    public string ApiUrl { get; set; } = string.Empty;
    public string Describe { get; set; } = string.Empty;
    public DateTime? LastRunTime { get; set; }
    /// <summary>
    /// 任务状态（JobState）
    /// </summary>
    public int Status { get; set; } = (int)JobState.暂停;
    /// <summary>
    /// 任务类型：约定 2=HTTP, 1=业务处理器(DLL)
    /// </summary>
    public int TaskType { get; set; } = 2;
    public string ApiRequestType { get; set; } = "GET";
    public string ApiAuthKey { get; set; } = string.Empty;
    public string ApiAuthValue { get; set; } = string.Empty;
    public string ApiParameter { get; set; } = string.Empty;
    /// <summary>
    /// 业务处理器类型名：推荐填写 "Namespace.Type, AssemblyName"
    /// </summary>
    public string DllClassName { get; set; } = string.Empty;
    public string DllActionName { get; set; } = string.Empty;

    public int Id { get; set; }
    public DateTime? TimeFlag { get; set; }
    public DateTime? ChangeTime { get; set; }

    [JsonIgnore]
    public string Identity => $"{GroupName}:{TaskName}";
}

