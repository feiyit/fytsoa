using System.Text.Json;
using FytSoa.Common.Scheduler.Jobs;
using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Scheduler.Options;
using FytSoa.Common.Scheduler.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;

namespace FytSoa.Common.Scheduler.Services;

public class FytSchedulerService : IFytSchedulerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISchedulerTaskStore _taskStore;
    private readonly IOptions<SchedulerOptions> _options;
    private readonly ILogger<FytSchedulerService> _logger;

    private readonly SemaphoreSlim _mutex = new(1, 1);
    private IScheduler? _scheduler;
    private List<QuartzTask> _tasks = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public FytSchedulerService(
        IServiceProvider serviceProvider,
        ISchedulerTaskStore taskStore,
        IOptions<SchedulerOptions> options,
        ILogger<FytSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _taskStore = taskStore;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            if (_scheduler != null) return;

            var props = new NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = "FytScheduler",
                ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                ["quartz.threadPool.threadCount"] = "10",
                ["quartz.threadPool.threadPriority"] = "Normal",
                ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
            };
            var factory = new StdSchedulerFactory(props);

            _scheduler = await factory.GetScheduler(ct);
            _scheduler.JobFactory = new SchedulerJobFactory(_serviceProvider);
            await _scheduler.Start(ct);

            _tasks = await _taskStore.LoadAllAsync(ct);
            await SyncAllJobsAsync(_tasks, ct);

            _logger.LogInformation("FytScheduler started. Jobs={Count}", _tasks.Count);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            if (_scheduler == null) return;
            await _scheduler.Shutdown(waitForJobsToComplete: false, cancellationToken: ct);
            _scheduler = null;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<List<QuartzTask>> GetJobs()
    {
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            // LastRunTime 会在任务执行时落盘更新，因此这里每次读取以保证接口返回最新状态
            _tasks = await _taskStore.LoadAllAsync();
            return _tasks.Select(Clone).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> AddJob(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        if (string.IsNullOrWhiteSpace(model.TaskName) || string.IsNullOrWhiteSpace(model.GroupName))
            return ResultQuartzData.Fail("TaskName/GroupName 不能为空");
        if (string.IsNullOrWhiteSpace(model.Interval))
            return ResultQuartzData.Fail("Cron 不能为空");

        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            if (_tasks.Any(x => SameKey(x, model)))
                return ResultQuartzData.Fail("任务已存在");

            model.Id = model.Id != 0 ? model.Id : (_tasks.Count == 0 ? 1 : _tasks.Max(x => x.Id) + 1);
            model.Status = (int)JobState.暂停; // 与旧实现一致：创建后默认暂停
            model.ChangeTime = DateTime.Now;

            _tasks.Add(Clone(model));
            await PersistAsync();

            await UpsertJobAsync(model, pauseIfNeeded: true, CancellationToken.None);
            return ResultQuartzData.Ok("新增成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddJob failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> Update(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            var existing = _tasks.FirstOrDefault(x => SameKey(x, model));
            if (existing == null) return ResultQuartzData.Fail("任务不存在");

            // 保留旧状态（除非外部明确传入 status）
            var keepStatus = existing.Status;

            CopyTo(model, existing);
            existing.Status = model.Status != 0 ? model.Status : keepStatus;
            existing.ChangeTime = DateTime.Now;

            await PersistAsync();

            await UpsertJobAsync(existing, pauseIfNeeded: existing.Status != (int)JobState.开启, CancellationToken.None);
            return ResultQuartzData.Ok("更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> Remove(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            var idx = _tasks.FindIndex(x => SameKey(x, model));
            if (idx < 0) return ResultQuartzData.Fail("任务不存在");

            var jobKey = new JobKey(_tasks[idx].TaskName, _tasks[idx].GroupName);
            _tasks.RemoveAt(idx);
            await PersistAsync();

            if (_scheduler != null)
            {
                await _scheduler.DeleteJob(jobKey);
            }

            return ResultQuartzData.Ok("删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Remove failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> Pause(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            var t = _tasks.FirstOrDefault(x => SameKey(x, model));
            if (t == null) return ResultQuartzData.Fail("任务不存在");

            t.Status = (int)JobState.暂停;
            t.ChangeTime = DateTime.Now;
            await PersistAsync();

            if (_scheduler != null)
            {
                await _scheduler.PauseJob(new JobKey(t.TaskName, t.GroupName));
            }

            return ResultQuartzData.Ok("暂停成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pause failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> Start(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            var t = _tasks.FirstOrDefault(x => SameKey(x, model));
            if (t == null) return ResultQuartzData.Fail("任务不存在");

            t.Status = (int)JobState.开启;
            t.ChangeTime = DateTime.Now;
            await PersistAsync();

            await UpsertJobAsync(t, pauseIfNeeded: false, CancellationToken.None);
            if (_scheduler != null)
            {
                await _scheduler.ResumeJob(new JobKey(t.TaskName, t.GroupName));
            }

            return ResultQuartzData.Ok("开启成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Start failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<ResultQuartzData> Run(QuartzTask model)
    {
        if (model == null) return ResultQuartzData.Fail("参数不能为空");
        await EnsureStartedAsync();
        await _mutex.WaitAsync();
        try
        {
            var t = _tasks.FirstOrDefault(x => SameKey(x, model));
            if (t == null) return ResultQuartzData.Fail("任务不存在");

            await UpsertJobAsync(t, pauseIfNeeded: t.Status != (int)JobState.开启, CancellationToken.None);
            if (_scheduler != null)
            {
                await _scheduler.TriggerJob(new JobKey(t.TaskName, t.GroupName));
            }

            return ResultQuartzData.Ok("已触发执行");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run failed");
            return ResultQuartzData.Fail(ex.Message);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task EnsureStartedAsync()
    {
        if (_scheduler != null) return;
        await StartAsync();
    }

    private async Task PersistAsync()
    {
        // 统一排序，便于 diff
        _tasks = _tasks
            .OrderBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.TaskName, StringComparer.OrdinalIgnoreCase)
            .Select(Clone)
            .ToList();
        await _taskStore.SaveAllAsync(_tasks);
    }

    private async Task SyncAllJobsAsync(List<QuartzTask> tasks, CancellationToken ct)
    {
        if (_scheduler == null) return;

        foreach (var t in tasks)
        {
            try
            {
                var pause = t.Status != (int)JobState.开启;
                await UpsertJobAsync(t, pause, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schedule job failed: {TaskName}({GroupName})", t.TaskName, t.GroupName);
            }
        }
    }

    private async Task UpsertJobAsync(QuartzTask t, bool pauseIfNeeded, CancellationToken ct)
    {
        if (_scheduler == null) return;

        var jobKey = new JobKey(t.TaskName, t.GroupName);
        var triggerKey = new TriggerKey($"{t.TaskName}.trigger", t.GroupName);

        var taskJson = JsonSerializer.Serialize(t, JsonOpts);
        var jobData = new JobDataMap
        {
            [SchedulerDispatchJob.JobDataKeyTaskJson] = taskJson,
        };

        var job = JobBuilder.Create<SchedulerDispatchJob>()
            .WithIdentity(jobKey)
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .WithCronSchedule(t.Interval, x => x.WithMisfireHandlingInstructionDoNothing())
            .StartNow()
            .Build();

        var exists = await _scheduler.CheckExists(jobKey, ct);
        if (exists)
        {
            // Quartz 3.4.0 下最稳妥的方式：删掉再重建（任务量不大，代价可接受）
            await _scheduler.DeleteJob(jobKey, ct);
        }

        await _scheduler.ScheduleJob(job, trigger, ct);

        if (pauseIfNeeded)
        {
            await _scheduler.PauseJob(jobKey, ct);
        }
    }

    private static bool SameKey(QuartzTask a, QuartzTask b) =>
        string.Equals(a.TaskName, b.TaskName, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(a.GroupName, b.GroupName, StringComparison.OrdinalIgnoreCase);

    private static QuartzTask Clone(QuartzTask m) => new()
    {
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
        Id = m.Id,
        TimeFlag = m.TimeFlag,
        ChangeTime = m.ChangeTime,
    };

    private static void CopyTo(QuartzTask from, QuartzTask to)
    {
        to.Interval = from.Interval;
        to.ApiUrl = from.ApiUrl ?? string.Empty;
        to.Describe = from.Describe ?? string.Empty;
        to.TaskType = from.TaskType;
        to.ApiRequestType = from.ApiRequestType ?? string.Empty;
        to.ApiAuthKey = from.ApiAuthKey ?? string.Empty;
        to.ApiAuthValue = from.ApiAuthValue ?? string.Empty;
        to.ApiParameter = from.ApiParameter ?? string.Empty;
        to.DllClassName = from.DllClassName ?? string.Empty;
        to.DllActionName = from.DllActionName ?? string.Empty;
    }
}
