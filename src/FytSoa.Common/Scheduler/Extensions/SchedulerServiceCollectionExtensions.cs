using FreeRedis;
using FytSoa.Common.Scheduler.Jobs;
using FytSoa.Common.Scheduler.Options;
using FytSoa.Common.Scheduler.Services;
using FytSoa.Common.Scheduler.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FytSoa.Common.Scheduler.Extensions;

public static class SchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddFytScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.Configure<SchedulerOptions>(configuration.GetSection("Scheduler"));

        // RedisClient 复用（只有在 StoreType=Redis 时才会被解析）
        services.AddSingleton(sp =>
        {
            var conn = configuration.GetValue<string>("Cache:Redis");
            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException("Cache:Redis is empty.");
            }
            return new RedisClient(conn);
        });

        services.AddSingleton<ISchedulerTaskStore>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SchedulerOptions>>().Value ??
                       new SchedulerOptions();
            return opts.StoreType switch
            {
                SchedulerStoreType.Redis => new RedisSchedulerTaskStore(
                    sp.GetRequiredService<RedisClient>(),
                    opts.RedisKeyPrefix),
                SchedulerStoreType.MySql => new MySqlSchedulerTaskStore(opts.MySqlAutoInitTables),
                _ => new FileSchedulerTaskStore(opts.TaskFilePath),
            };
        });

        services.AddSingleton<ISchedulerLogStore>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SchedulerOptions>>().Value ??
                       new SchedulerOptions();
            return opts.StoreType switch
            {
                SchedulerStoreType.Redis => new RedisSchedulerLogStore(
                    sp.GetRequiredService<RedisClient>(),
                    opts.RedisKeyPrefix),
                SchedulerStoreType.MySql => new MySqlSchedulerLogStore(opts.MySqlAutoInitTables),
                _ => new FileSchedulerLogStore(opts.LogFilePath),
            };
        });

        // Quartz Job
        services.AddTransient<SchedulerDispatchJob>();

        // Scheduler services
        services.AddSingleton<IFytSchedulerService, FytSchedulerService>();
        services.AddSingleton<IFytSchedulerLogService, FytSchedulerLogService>();
        services.AddHostedService<FytSchedulerHostedService>();

        return services;
    }
}
