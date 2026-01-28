using System.Text.Json;
using FreeRedis;
using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Stores;

public class RedisSchedulerTaskStore : ISchedulerTaskStore
{
    private readonly RedisClient _redis;
    private readonly string _key;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public RedisSchedulerTaskStore(RedisClient redis, string keyPrefix)
    {
        _redis = redis;
        _key = $"{keyPrefix}:tasks";
    }

    public async Task<List<QuartzTask>> LoadAllAsync(CancellationToken ct = default)
    {
        var json = await _redis.GetAsync<string>(_key);
        if (string.IsNullOrWhiteSpace(json)) return new List<QuartzTask>();
        try
        {
            return JsonSerializer.Deserialize<List<QuartzTask>>(json, JsonOpts) ?? new List<QuartzTask>();
        }
        catch
        {
            return new List<QuartzTask>();
        }
    }

    public async Task SaveAllAsync(List<QuartzTask> tasks, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(tasks ?? new List<QuartzTask>(), JsonOpts);
        await _redis.SetAsync(_key, json);
    }
}

