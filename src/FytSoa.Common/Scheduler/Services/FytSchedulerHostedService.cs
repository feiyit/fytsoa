using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FytSoa.Common.Scheduler.Services;

public class FytSchedulerHostedService : IHostedService
{
    private readonly IFytSchedulerService _scheduler;
    private readonly ILogger<FytSchedulerHostedService> _logger;

    public FytSchedulerHostedService(IFytSchedulerService scheduler, ILogger<FytSchedulerHostedService> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _scheduler.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FytSchedulerHostedService start failed");
            // 不抛出，避免影响主应用启动；调度模块失败可通过日志排查
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => _scheduler.StopAsync(cancellationToken);
}

