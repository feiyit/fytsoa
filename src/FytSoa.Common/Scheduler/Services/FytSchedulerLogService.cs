using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Scheduler.Options;
using FytSoa.Common.Scheduler.Stores;
using Microsoft.Extensions.Options;

namespace FytSoa.Common.Scheduler.Services;

public class FytSchedulerLogService : IFytSchedulerLogService
{
    private readonly ISchedulerLogStore _store;
    private readonly SchedulerOptions _options;

    public FytSchedulerLogService(ISchedulerLogStore store, IOptions<SchedulerOptions> options)
    {
        _store = store;
        _options = options.Value ?? new SchedulerOptions();
    }

    public Task<ResultData<QuartzTaskLog>> GetLogs(string taskName, string groupName, int page, int pageSize) =>
        _store.QueryAsync(taskName, groupName, page, pageSize);

    public Task<QuartzTaskLog?> GetLastLog(string taskName, string groupName) =>
        _store.GetLastAsync(taskName, groupName);

    public Task CleanupAsync()
    {
        if (_options.LogRetentionDays <= 0) return Task.CompletedTask;
        var olderThan = DateTime.Now.AddDays(-_options.LogRetentionDays);
        return _store.CleanupAsync(olderThan);
    }
}
