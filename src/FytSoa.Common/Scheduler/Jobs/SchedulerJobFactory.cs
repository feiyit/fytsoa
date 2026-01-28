using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace FytSoa.Common.Scheduler.Jobs;

/// <summary>
/// 让 Quartz Job 由 ASP.NET Core DI 创建，支持依赖注入。
/// </summary>
public class SchedulerJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SchedulerJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = _serviceProvider.CreateScope();
        try
        {
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                throw new InvalidOperationException($"Cannot resolve job type: {bundle.JobDetail.JobType.FullName}");
            }
            return new ScopedJob(job, scope);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        if (job is ScopedJob scoped)
        {
            scoped.Dispose();
            return;
        }
        (job as IDisposable)?.Dispose();
    }

    private sealed class ScopedJob : IJob, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IJob _inner;

        public ScopedJob(IJob inner, IServiceScope scope)
        {
            _inner = inner;
            _scope = scope;
        }

        public Task Execute(IJobExecutionContext context) => _inner.Execute(context);

        public void Dispose()
        {
            (_inner as IDisposable)?.Dispose();
            _scope.Dispose();
        }
    }
}
