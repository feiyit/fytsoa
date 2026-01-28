using SqlSugar;

namespace FytSoa.Common.Scheduler.Stores;

[SugarTable("sys_scheduler_task")]
public class MySqlSchedulerTaskEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 100, IsNullable = false)]
    public string TaskName { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = false)]
    public string GroupName { get; set; } = "default";

    [SugarColumn(Length = 100, IsNullable = false)]
    public string Interval { get; set; } = "0 0 * * * ?";

    [SugarColumn(Length = 500, IsNullable = true)]
    public string ApiUrl { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = true)]
    public string Describe { get; set; } = string.Empty;

    public DateTime? LastRunTime { get; set; }

    public int Status { get; set; }

    public int TaskType { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string ApiRequestType { get; set; } = "GET";

    [SugarColumn(Length = 50, IsNullable = true)]
    public string ApiAuthKey { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string ApiAuthValue { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string ApiParameter { get; set; } = string.Empty;

    [SugarColumn(Length = 300, IsNullable = true)]
    public string DllClassName { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = true)]
    public string DllActionName { get; set; } = string.Empty;

    public DateTime? TimeFlag { get; set; }
    public DateTime? ChangeTime { get; set; }
}

[SugarTable("sys_scheduler_task_log")]
public class MySqlSchedulerTaskLogEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 100, IsNullable = false)]
    public string TaskName { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = false)]
    public string GroupName { get; set; } = string.Empty;

    public DateTime BeginDate { get; set; }
    public DateTime? EndDate { get; set; }

    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Msg { get; set; } = string.Empty;
}
