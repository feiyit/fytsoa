namespace FytSoa.Common.Scheduler.Options;

public enum SchedulerStoreType
{
    File = 0,
    Redis = 1,
    MySql = 2,
}

public class SchedulerOptions
{
    /// <summary>
    /// 存储类型（任务/日志）
    /// </summary>
    public SchedulerStoreType StoreType { get; set; } = SchedulerStoreType.File;

    /// <summary>
    /// 任务文件路径（StoreType=File 时使用）。默认：{AppRoot}/Quartz.Settings/task_job.json
    /// </summary>
    public string? TaskFilePath { get; set; }

    /// <summary>
    /// 日志文件路径（StoreType=File 时使用）。默认：{AppRoot}/Quartz.Settings/logs/task_logs.jsonl
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Redis Key 前缀（StoreType=Redis 时使用）
    /// </summary>
    public string RedisKeyPrefix { get; set; } = "fyt:scheduler";

    /// <summary>
    /// MySQL 表前缀（StoreType=MySql 时使用）
    /// </summary>
    public string MySqlTablePrefix { get; set; } = "sys_scheduler";

    /// <summary>
    /// 日志保留天数（<=0 表示不清理）
    /// </summary>
    public int LogRetentionDays { get; set; } = 180;

    /// <summary>
    /// 是否自动建表（仅 MySql）
    /// </summary>
    public bool MySqlAutoInitTables { get; set; } = false;
}

