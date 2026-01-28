using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Stores;

public interface ISchedulerTaskStore
{
    Task<List<QuartzTask>> LoadAllAsync(CancellationToken ct = default);
    Task SaveAllAsync(List<QuartzTask> tasks, CancellationToken ct = default);
}

