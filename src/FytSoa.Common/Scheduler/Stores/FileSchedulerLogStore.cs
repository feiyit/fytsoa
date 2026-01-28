using System.Text.Json;
using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Utils;

namespace FytSoa.Common.Scheduler.Stores;

/// <summary>
/// 文件日志存储：JSON Lines（每行一个 QuartzTaskLog），便于追加与按需读取。
/// </summary>
public class FileSchedulerLogStore : ISchedulerLogStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public FileSchedulerLogStore(string? path = null)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _path = path!;
            return;
        }

        // 默认复用历史路径：Quartz.Settings/logs/logs.txt（内部用 JSONL 追加，不影响作为纯文本查看）
        var p1 = Path.Combine(AppUtils.AppRoot, "Quartz.Settings", "logs", "logs.txt");
        var p2 = Path.GetFullPath(Path.Combine(AppUtils.AppRoot, "..", "Quartz.Settings", "logs", "logs.txt"));
        _path = File.Exists(p1) || Directory.Exists(Path.GetDirectoryName(p1)!) ? p1 : p2;
    }

    public async Task AppendAsync(QuartzTaskLog log, CancellationToken ct = default)
    {
        EnsureDir();
        var line = JsonSerializer.Serialize(log, JsonOpts);
        await File.AppendAllTextAsync(_path, line + Environment.NewLine, ct);
    }

    public async Task<ResultData<QuartzTaskLog>> QueryAsync(string taskName, string groupName, int page, int pageSize,
        CancellationToken ct = default)
    {
        EnsureDir();
        if (!File.Exists(_path))
        {
            return new ResultData<QuartzTaskLog> { total = 0, data = new List<QuartzTaskLog>() };
        }

        // 简单实现：全部读取再过滤分页（半年量级在可接受范围内；后续可改为倒序扫描）
        var lines = await File.ReadAllLinesAsync(_path, ct);
        var list = new List<QuartzTaskLog>(capacity: lines.Length);
        foreach (var ln in lines)
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            try
            {
                var item = JsonSerializer.Deserialize<QuartzTaskLog>(ln, JsonOpts);
                if (item == null) continue;
                if (!string.Equals(item.TaskName, taskName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(item.GroupName, groupName, StringComparison.OrdinalIgnoreCase)) continue;
                list.Add(item);
            }
            catch
            {
                // ignore bad line
            }
        }

        // 按 BeginDate 倒序（兼容 UTC / Local）
        static DateTime AsLocal(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;
        list.Sort((a, b) => AsLocal(b.BeginDate).CompareTo(AsLocal(a.BeginDate)));

        var total = list.Count;
        var p = Math.Max(page, 1);
        var s = Math.Max(pageSize, 1);
        var skip = (p - 1) * s;
        var data = skip >= total ? new List<QuartzTaskLog>() : list.Skip(skip).Take(s).ToList();
        return new ResultData<QuartzTaskLog> { total = total, data = data };
    }

    public async Task<QuartzTaskLog?> GetLastAsync(string taskName, string groupName, CancellationToken ct = default)
    {
        var res = await QueryAsync(taskName, groupName, 1, 1, ct);
        return res.data.FirstOrDefault();
    }

    public async Task CleanupAsync(DateTime olderThanUtc, CancellationToken ct = default)
    {
        EnsureDir();
        if (!File.Exists(_path)) return;

        var lines = await File.ReadAllLinesAsync(_path, ct);
        var keep = new List<string>(capacity: lines.Length);
        foreach (var ln in lines)
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            try
            {
                var item = JsonSerializer.Deserialize<QuartzTaskLog>(ln, JsonOpts);
                if (item == null) continue;
                // 兼容历史日志（UTC 结尾 Z）与新日志（本地时间带 +08:00）
                var beginLocal = item.BeginDate.Kind == DateTimeKind.Utc
                    ? item.BeginDate.ToLocalTime()
                    : item.BeginDate;
                var thresholdLocal = olderThanUtc.Kind == DateTimeKind.Utc
                    ? olderThanUtc.ToLocalTime()
                    : olderThanUtc;
                if (beginLocal >= thresholdLocal) keep.Add(ln);
            }
            catch
            {
                // bad line: drop
            }
        }

        await File.WriteAllLinesAsync(_path, keep, ct);
    }

    private void EnsureDir()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
