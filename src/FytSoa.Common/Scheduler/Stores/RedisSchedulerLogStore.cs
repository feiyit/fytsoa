using System.Text.Json;
using FreeRedis;
using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Stores;

public class RedisSchedulerLogStore : ISchedulerLogStore
{
    private readonly RedisClient _redis;
    private readonly string _prefix;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public RedisSchedulerLogStore(RedisClient redis, string keyPrefix)
    {
        _redis = redis;
        _prefix = $"{keyPrefix}:logs";
    }

    private string Key(string taskName, string groupName) => $"{_prefix}:{groupName}:{taskName}";

    public async Task AppendAsync(QuartzTaskLog log, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(log, JsonOpts);
        // 左侧入队，便于按时间倒序分页
        await _redis.LPushAsync(Key(log.TaskName, log.GroupName), json);
    }

    public async Task<ResultData<QuartzTaskLog>> QueryAsync(string taskName, string groupName, int page, int pageSize,
        CancellationToken ct = default)
    {
        var key = Key(taskName, groupName);
        var total = (int)await _redis.LLenAsync(key);
        var p = Math.Max(page, 1);
        var s = Math.Max(pageSize, 1);
        var start = (p - 1) * s;
        var stop = start + s - 1;
        var items = await _redis.LRangeAsync(key, start, stop);

        var data = new List<QuartzTaskLog>();
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it)) continue;
            try
            {
                var m = JsonSerializer.Deserialize<QuartzTaskLog>(it, JsonOpts);
                if (m != null) data.Add(m);
            }
            catch
            {
                // ignore
            }
        }

        return new ResultData<QuartzTaskLog> { total = total, data = data };
    }

    public async Task<QuartzTaskLog?> GetLastAsync(string taskName, string groupName, CancellationToken ct = default)
    {
        var res = await _redis.LIndexAsync(Key(taskName, groupName), 0);
        if (string.IsNullOrWhiteSpace(res)) return null;
        try
        {
            return JsonSerializer.Deserialize<QuartzTaskLog>(res, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    public Task CleanupAsync(DateTime olderThanUtc, CancellationToken ct = default)
    {
        // Redis 存储：建议用 TTL / 定时清理，这里留空（可扩展为扫描并删除过期日志）
        return Task.CompletedTask;
    }
}

