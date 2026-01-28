using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Stores;

public interface ISchedulerLogStore
{
    Task AppendAsync(QuartzTaskLog log, CancellationToken ct = default);
    Task<ResultData<QuartzTaskLog>> QueryAsync(string taskName, string groupName, int page, int pageSize, CancellationToken ct = default);
    Task<QuartzTaskLog?> GetLastAsync(string taskName, string groupName, CancellationToken ct = default);
    Task CleanupAsync(DateTime olderThanUtc, CancellationToken ct = default);
}

