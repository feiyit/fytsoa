using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Utils;
using SqlSugar;

namespace FytSoa.Common.Scheduler.Stores;

public class MySqlSchedulerTaskStore : ISchedulerTaskStore
{
    private readonly SqlSugarClient _db;

    public MySqlSchedulerTaskStore(bool autoInitTables)
    {
        _db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            ConnectionString = AppUtils.MySqlConnectionString,
        });

        if (autoInitTables)
        {
            _db.CodeFirst.InitTables<MySqlSchedulerTaskEntity>();
        }
    }

    public async Task<List<QuartzTask>> LoadAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.Queryable<MySqlSchedulerTaskEntity>()
            .OrderBy(x => x.GroupName)
            .OrderBy(x => x.TaskName)
            .ToListAsync();

        return rows.Select(ToModel).ToList();
    }

    public async Task SaveAllAsync(List<QuartzTask> tasks, CancellationToken ct = default)
    {
        // 简单实现：全量覆盖（任务量通常不大）
        await _db.Deleteable<MySqlSchedulerTaskEntity>().ExecuteCommandAsync();
        if (tasks == null || tasks.Count == 0) return;
        var list = tasks.Select(ToEntity).ToList();
        await _db.Insertable(list).ExecuteCommandAsync();
    }

    private static QuartzTask ToModel(MySqlSchedulerTaskEntity e) => new()
    {
        TaskName = e.TaskName,
        GroupName = e.GroupName,
        Interval = e.Interval,
        ApiUrl = e.ApiUrl,
        Describe = e.Describe,
        LastRunTime = e.LastRunTime,
        Status = e.Status,
        TaskType = e.TaskType,
        ApiRequestType = e.ApiRequestType,
        ApiAuthKey = e.ApiAuthKey,
        ApiAuthValue = e.ApiAuthValue,
        ApiParameter = e.ApiParameter,
        DllClassName = e.DllClassName,
        DllActionName = e.DllActionName,
        Id = e.Id,
        TimeFlag = e.TimeFlag,
        ChangeTime = e.ChangeTime,
    };

    private static MySqlSchedulerTaskEntity ToEntity(QuartzTask m) => new()
    {
        Id = m.Id,
        TaskName = m.TaskName,
        GroupName = m.GroupName,
        Interval = m.Interval,
        ApiUrl = m.ApiUrl,
        Describe = m.Describe,
        LastRunTime = m.LastRunTime,
        Status = m.Status,
        TaskType = m.TaskType,
        ApiRequestType = m.ApiRequestType,
        ApiAuthKey = m.ApiAuthKey,
        ApiAuthValue = m.ApiAuthValue,
        ApiParameter = m.ApiParameter,
        DllClassName = m.DllClassName,
        DllActionName = m.DllActionName,
        TimeFlag = m.TimeFlag,
        ChangeTime = m.ChangeTime,
    };
}

public class MySqlSchedulerLogStore : ISchedulerLogStore
{
    private readonly SqlSugarClient _db;

    public MySqlSchedulerLogStore(bool autoInitTables)
    {
        _db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            ConnectionString = AppUtils.MySqlConnectionString,
        });

        if (autoInitTables)
        {
            _db.CodeFirst.InitTables<MySqlSchedulerTaskLogEntity>();
        }
    }

    public async Task AppendAsync(QuartzTaskLog log, CancellationToken ct = default)
    {
        var e = new MySqlSchedulerTaskLogEntity
        {
            TaskName = log.TaskName,
            GroupName = log.GroupName,
            BeginDate = log.BeginDate,
            EndDate = log.EndDate,
            Msg = log.Msg,
        };
        await _db.Insertable(e).ExecuteCommandAsync();
    }

    public async Task<ResultData<QuartzTaskLog>> QueryAsync(string taskName, string groupName, int page, int pageSize,
        CancellationToken ct = default)
    {
        var p = Math.Max(page, 1);
        var s = Math.Max(pageSize, 1);

        RefAsync<int> total = 0;
        var rows = await _db.Queryable<MySqlSchedulerTaskLogEntity>()
            .Where(x => x.TaskName == taskName && x.GroupName == groupName)
            .OrderBy(x => x.BeginDate, OrderByType.Desc)
            .ToPageListAsync(p, s, total);

        return new ResultData<QuartzTaskLog>
        {
            total = total,
            data = rows.Select(r => new QuartzTaskLog
            {
                TaskName = r.TaskName,
                GroupName = r.GroupName,
                BeginDate = r.BeginDate,
                EndDate = r.EndDate,
                Msg = r.Msg,
                Id = r.Id,
            }).ToList(),
        };
    }

    public async Task<QuartzTaskLog?> GetLastAsync(string taskName, string groupName, CancellationToken ct = default)
    {
        var row = await _db.Queryable<MySqlSchedulerTaskLogEntity>()
            .Where(x => x.TaskName == taskName && x.GroupName == groupName)
            .OrderBy(x => x.BeginDate, OrderByType.Desc)
            .FirstAsync();

        if (row == null) return null;
        return new QuartzTaskLog
        {
            TaskName = row.TaskName,
            GroupName = row.GroupName,
            BeginDate = row.BeginDate,
            EndDate = row.EndDate,
            Msg = row.Msg,
            Id = row.Id,
        };
    }

    public async Task CleanupAsync(DateTime olderThanUtc, CancellationToken ct = default)
    {
        await _db.Deleteable<MySqlSchedulerTaskLogEntity>()
            .Where(x => x.BeginDate < olderThanUtc)
            .ExecuteCommandAsync();
    }
}
