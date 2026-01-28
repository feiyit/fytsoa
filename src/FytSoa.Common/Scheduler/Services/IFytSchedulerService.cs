using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Services;

public interface IFytSchedulerService
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);

    Task<List<QuartzTask>> GetJobs();
    Task<ResultQuartzData> AddJob(QuartzTask model);
    Task<ResultQuartzData> Update(QuartzTask model);
    Task<ResultQuartzData> Remove(QuartzTask model);
    Task<ResultQuartzData> Pause(QuartzTask model);
    Task<ResultQuartzData> Start(QuartzTask model);
    Task<ResultQuartzData> Run(QuartzTask model);
}

