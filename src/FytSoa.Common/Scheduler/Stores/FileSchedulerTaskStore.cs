using System.Text.Json;
using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Utils;

namespace FytSoa.Common.Scheduler.Stores;

public class FileSchedulerTaskStore : ISchedulerTaskStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public FileSchedulerTaskStore(string? path = null)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _path = path!;
            return;
        }

        // 兼容两种运行目录：
        // 1) 项目根目录：{AppRoot}/Quartz.Settings/...
        // 2) ApiService 目录：{AppRoot}/../Quartz.Settings/...
        var p1 = Path.Combine(AppUtils.AppRoot, "Quartz.Settings", "task_job.json");
        var p2 = Path.GetFullPath(Path.Combine(AppUtils.AppRoot, "..", "Quartz.Settings", "task_job.json"));
        _path = File.Exists(p1) || Directory.Exists(Path.GetDirectoryName(p1)!) ? p1 : p2;
    }

    public async Task<List<QuartzTask>> LoadAllAsync(CancellationToken ct = default)
    {
        EnsureDir();
        if (!File.Exists(_path))
        {
            await File.WriteAllTextAsync(_path, "[]", ct);
            return new List<QuartzTask>();
        }

        var json = await File.ReadAllTextAsync(_path, ct);
        if (string.IsNullOrWhiteSpace(json)) return new List<QuartzTask>();

        try
        {
            return JsonSerializer.Deserialize<List<QuartzTask>>(json, JsonOpts) ?? new List<QuartzTask>();
        }
        catch
        {
            // 避免因文件损坏导致系统启动失败：返回空列表
            return new List<QuartzTask>();
        }
    }

    public async Task SaveAllAsync(List<QuartzTask> tasks, CancellationToken ct = default)
    {
        EnsureDir();
        var json = JsonSerializer.Serialize(tasks ?? new List<QuartzTask>(), JsonOpts);
        await File.WriteAllTextAsync(_path, json, ct);
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
