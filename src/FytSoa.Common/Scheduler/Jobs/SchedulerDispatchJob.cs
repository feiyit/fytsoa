using System.Text.Json;
using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Scheduler.Options;
using FytSoa.Common.Scheduler.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Reflection;

namespace FytSoa.Common.Scheduler.Jobs;

/// <summary>
/// Quartz 统一分发 Job：根据 QuartzTask.TaskType 执行 HTTP 或业务处理器方法。
/// </summary>
public class SchedulerDispatchJob : IJob
{
    public const string JobDataKeyTaskJson = "task_json";

    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISchedulerLogStore _logStore;
    private readonly ISchedulerTaskStore _taskStore;
    private readonly SchedulerOptions _options;
    private readonly ILogger<SchedulerDispatchJob> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public SchedulerDispatchJob(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        ISchedulerLogStore logStore,
        ISchedulerTaskStore taskStore,
        IOptions<SchedulerOptions> options,
        ILogger<SchedulerDispatchJob> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _logStore = logStore;
        _taskStore = taskStore;
        _options = options.Value ?? new SchedulerOptions();
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var taskJson = context.MergedJobDataMap.GetString(JobDataKeyTaskJson) ?? string.Empty;
        QuartzTask? task = null;
        try
        {
            task = JsonSerializer.Deserialize<QuartzTask>(taskJson, JsonOpts);
        }
        catch
        {
            // ignore
        }

        if (task == null)
        {
            _logger.LogWarning("SchedulerDispatchJob: missing task payload for {JobKey}", context.JobDetail.Key);
            return;
        }

        // 使用本地时间写入日志，便于直接查看（如 17:46，而不是 UTC 08:46Z）
        var begin = DateTime.Now;
        var log = new QuartzTaskLog
        {
            TaskName = task.TaskName,
            GroupName = task.GroupName,
            BeginDate = begin,
        };

        try
        {
            if (task.TaskType == 2)
            {
                await ExecuteHttpAsync(task, context.CancellationToken);
            }
            else
            {
                await ExecuteHandlerAsync(task, context.CancellationToken);
            }

            log.Msg = "Success";
        }
        catch (Exception ex)
        {
            log.Msg = ex.ToString();
            _logger.LogError(ex, "Scheduler task failed: {TaskName}({GroupName})", task.TaskName, task.GroupName);
        }
        finally
        {
            log.EndDate = DateTime.Now;
            await _logStore.AppendAsync(log, context.CancellationToken);

            // 更新 LastRunTime（尽量保持与旧实现一致：本地存储字段以本地时间展示）
            await TouchLastRunTimeAsync(task, DateTime.Now, context.CancellationToken);

            // 日志清理（File/MySQL），避免无限增长
            if (_options.LogRetentionDays > 0)
            {
                var olderThan = DateTime.Now.AddDays(-_options.LogRetentionDays);
                await _logStore.CleanupAsync(olderThan, context.CancellationToken);
            }
        }
    }

    private async Task ExecuteHttpAsync(QuartzTask task, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(task.ApiUrl))
            throw new InvalidOperationException("ApiUrl is required for HTTP task.");

        var client = _httpClientFactory.CreateClient("fyt-scheduler");
        var method = (task.ApiRequestType ?? "GET").Trim().ToUpperInvariant();

        var reqUrl = task.ApiUrl.Trim();
        HttpContent? content = null;

        // ApiParameter 允许传 querystring 或 json；这里按简单规则处理
        var param = task.ApiParameter ?? string.Empty;
        if (method == "GET")
        {
            if (!string.IsNullOrWhiteSpace(param))
            {
                var extra = param.Trim();
                if (extra.StartsWith("{") && extra.EndsWith("}"))
                {
                    // json object => querystring
                    try
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(extra, JsonOpts) ?? new();
                        var qs = string.Join("&", dict.Select(kv =>
                            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(Convert.ToString(kv.Value) ?? string.Empty)}"));
                        if (!string.IsNullOrWhiteSpace(qs))
                        {
                            reqUrl += reqUrl.Contains("?") ? "&" + qs : "?" + qs;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
                else
                {
                    // raw querystring
                    var qs = extra.StartsWith("?") ? extra[1..] : extra;
                    if (!string.IsNullOrWhiteSpace(qs))
                    {
                        reqUrl += reqUrl.Contains("?") ? "&" + qs : "?" + qs;
                    }
                }
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(param))
            {
                var body = param.Trim();
                var isJson = body.StartsWith("{") || body.StartsWith("[");
                content = new StringContent(body, System.Text.Encoding.UTF8, isJson ? "application/json" : "text/plain");
            }
        }

        using var request = new HttpRequestMessage(new HttpMethod(method), reqUrl)
        {
            Content = content,
        };

        if (!string.IsNullOrWhiteSpace(task.ApiAuthKey))
        {
            request.Headers.TryAddWithoutValidation(task.ApiAuthKey.Trim(), task.ApiAuthValue ?? string.Empty);
        }

        using var resp = await client.SendAsync(request, ct);
        var respText = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {Truncate(respText, 2000)}");
        }
    }

    private async Task ExecuteHandlerAsync(QuartzTask task, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(task.DllClassName) || string.IsNullOrWhiteSpace(task.DllActionName))
            throw new InvalidOperationException("DllClassName and DllActionName are required for handler task.");

        var type = ResolveType(task.DllClassName);
        if (type == null) throw new InvalidOperationException($"Type not found: {task.DllClassName}");

        var method = type.GetMethod(task.DllActionName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (method == null)
        {
            throw new InvalidOperationException($"Method not found: {type.FullName}.{task.DllActionName}");
        }
        if (method.GetParameters().Length != 0)
        {
            throw new InvalidOperationException("Handler method must be parameterless.");
        }

        object? instance = null;
        if (!method.IsStatic)
        {
            // 优先从 DI 容器解析（支持带依赖构造函数），否则 fallback Activator.CreateInstance
            instance = _serviceProvider.GetService(type) ?? Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException($"Cannot create instance for type: {type.FullName}");
            }
        }

        var result = method.Invoke(instance, null);
        if (result is Task t)
        {
            await t;
        }
    }

    private static Type? ResolveType(string dllClassName)
    {
        if (string.IsNullOrWhiteSpace(dllClassName)) return null;

        // 1) 先尝试完整限定名（含程序集）
        var t = Type.GetType(dllClassName, throwOnError: false);
        if (t != null) return t;

        // 2) 兼容旧配置：可能写成 "Namespace.Type, SomeAssemblyName" 但实际程序集名不同
        var typeName = dllClassName.Split(',')[0].Trim();
        if (string.IsNullOrWhiteSpace(typeName)) return null;

        // 3) 尝试显式加载逗号后面的程序集名（如果有）
        var parts = dllClassName.Split(',');
        if (parts.Length >= 2)
        {
            var asmName = parts[1].Trim();
            if (!string.IsNullOrWhiteSpace(asmName))
            {
                try
                {
                    var asm = Assembly.Load(asmName);
                    t = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;
                }
                catch
                {
                    // ignore
                }
            }
        }

        // 4) 最后：遍历当前已加载程序集
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                t = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (t != null) return t;
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }

    private async Task TouchLastRunTimeAsync(QuartzTask task, DateTime localRunTime, CancellationToken ct)
    {
        // 为兼容 UI：LastRunTime 用于展示，不做 UTC 强制
        var list = await _taskStore.LoadAllAsync(ct);
        var target = list.FirstOrDefault(x =>
            string.Equals(x.TaskName, task.TaskName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.GroupName, task.GroupName, StringComparison.OrdinalIgnoreCase));
        if (target == null) return;
        target.LastRunTime = localRunTime;
        target.ChangeTime = DateTime.Now;
        await _taskStore.SaveAllAsync(list, ct);
    }

    private static string Truncate(string? s, int maxLen)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= maxLen ? s : s.Substring(0, maxLen);
    }
}
