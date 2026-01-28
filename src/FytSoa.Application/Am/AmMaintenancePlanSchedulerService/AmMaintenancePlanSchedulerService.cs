using FytSoa.Domain.Am;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FytSoa.Application.Am;

/// <summary>
/// 保养计划：后台任务/Quartz 调度专用逻辑（不走 HttpContext）。
/// </summary>
public class AmMaintenancePlanSchedulerService
{
    private static readonly HashSet<int> DueSoonOffsets = new() { 15, 5, 3 };

    private readonly SugarRepository<AmMaintenancePlan> _planRepo;
    private readonly SugarRepository<SysNotice> _noticeRepo;
    private readonly SugarRepository<SysAdmin> _adminRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AmMaintenancePlanSchedulerService> _logger;

    public AmMaintenancePlanSchedulerService(
        SugarRepository<AmMaintenancePlan> planRepo,
        SugarRepository<SysNotice> noticeRepo,
        SugarRepository<SysAdmin> adminRepo,
        IConfiguration configuration,
        ILogger<AmMaintenancePlanSchedulerService> logger)
    {
        _planRepo = planRepo;
        _noticeRepo = noticeRepo;
        _adminRepo = adminRepo;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 扫描保养计划：
    /// - 当前时间距离下次执行还有 15/5/3 天时，分别发送提醒通知（不推进 NextRunTime）
    /// - 已到期（NextRunTime &lt;= now）时发送到期提醒，并推进 NextRunTime（避免重复提醒）
    /// 说明：
    /// - 15/5/3 天提醒：通过在通知 Content 写入唯一 marker 做幂等，避免一天内多次调度重复发送。
    /// - NextRunTime 为空或 ManagerId=0 的计划会被跳过。
    /// </summary>
    public async Task NotifyDuePlansAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;
        // 扫描范围：过去所有 + 未来 15 天（含当天）的计划
        var dueSoonEnd = today.AddDays(15).AddDays(1).AddTicks(-1);
        Console.WriteLine($"日期dueSoonEnd={dueSoonEnd}");
        // 只扫描“已到期 + 未来 15 天内”的计划，减少全表扫描压力
        var plans = await _planRepo.AsQueryable()
            .Where(x => x.IsEnabled)
            .Where(x => x.ManagerId > 0)
            .Where(x => x.NextRunTime != null && x.NextRunTime <= dueSoonEnd)
            .ToListAsync();
        Console.WriteLine($"plans的长度={plans.Count}");
        if (plans.Count == 0) return;

        var senderId = await ResolveSenderIdAsync();
        if (senderId <= 0)
        {
            _logger.LogWarning("MaintenancePlan notify skipped: cannot resolve SysNotice sender id.");
            return;
        }
        Console.WriteLine($"发送人编号={senderId}");
        foreach (var plan in plans)
        {
            try
            {
                if (!plan.NextRunTime.HasValue) continue;
                var nextRun = plan.NextRunTime.Value;

                // 1) 距离下次执行还有 15/5/3 天提醒（按“当前日期 + N 天 == 下次执行日期”判断，不推进 NextRunTime）
                // 例如：now=2026-01-28，则 now+15=2026-02-12；若 NextRunTime 日期为 02-12 则发送提醒
                foreach (var offset in DueSoonOffsets)
                {
                    Console.WriteLine($"判断前时间={today.AddDays(offset)}");
                    Console.WriteLine($"判断后时间={nextRun.Date}");
                    if (today.AddDays(offset) == nextRun.Date)
                    {
                        await SendDueSoonNoticeIfNeededAsync(plan, senderId, offset, nextRun);
                    }
                }

                // 2) 已到期：发送到期提醒 + 推进 NextRunTime（避免重复提醒）
                if (now > nextRun)
                {
                    await SendDueNoticeAndAdvanceAsync(plan, senderId, now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MaintenancePlan notify failed. PlanId={PlanId}, PlanNo={PlanNo}", plan.Id, plan.PlanNo);
            }
        }
    }

    private async Task SendDueSoonNoticeIfNeededAsync(AmMaintenancePlan plan, long senderId, int diffDays, DateTime nextRun)
    {
        var marker = BuildMarker(plan, nextRun, diffDays);
        var exists = await _noticeRepo.AsQueryable()
            .Where(x => x.TenantId == plan.TenantId)
            .Where(x => x.Content != null && x.Content.Contains(marker))
            .AnyAsync();
        if (exists) return;

        var nextText = nextRun.ToString("yyyy-MM-dd HH:mm:ss");
        var notice = new SysNotice
        {
            TenantId = plan.TenantId,
            SendUserId = senderId,
            AcceptUserIds = [plan.ManagerId],
            Title = $"保养计划提醒（剩余{diffDays}天）：{plan.Name}",
            Content =
                $"{marker}\n" +
                $"计划编号：{plan.PlanNo}\n" +
                $"计划名称：{plan.Name}\n" +
                $"下次执行时间：{nextText}\n" +
                $"执行周期：{plan.CycleType} / {plan.CycleValue}\n" +
                $"距离下次执行还有 {diffDays} 天，请提前安排处理。",
            Status = 0,
            IsSend = false,
            CreateTime = DateTime.Now,
        };

        await _noticeRepo.InsertAsync(notice);
    }

    private async Task SendDueNoticeAndAdvanceAsync(AmMaintenancePlan plan, long senderId, DateTime now)
    {
        var dueText = plan.NextRunTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";

        var notice = new SysNotice
        {
            TenantId = plan.TenantId,
            SendUserId = senderId,
            AcceptUserIds = new List<long> { plan.ManagerId },
            Title = $"保养计划到期提醒：{plan.Name}",
            Content =
                $"计划编号：{plan.PlanNo}\n" +
                $"计划名称：{plan.Name}\n" +
                $"到期/执行时间：{dueText}\n" +
                $"执行周期：{plan.CycleType} / {plan.CycleValue}\n" +
                "请及时处理当前保养计划。",
            Status = 0,
            IsSend = false,
            CreateTime = DateTime.Now,
        };

        await _noticeRepo.InsertAsync(notice);

        if (plan.NextRunTime.HasValue)
        {
            var next = ComputeNextRunTime(plan.NextRunTime.Value, plan.CycleType, plan.CycleValue, now);
            await _planRepo.UpdateAsync(
                x => new AmMaintenancePlan { NextRunTime = next, UpdateTime = DateTime.Now },
                x => x.Id == plan.Id);
        }
    }

    private static string BuildMarker(AmMaintenancePlan plan, DateTime nextRun, int diffDays) =>
        $"[MP:{plan.Id}:{nextRun:yyyyMMddHHmmss}:D{diffDays}]";

    private async Task<long> ResolveSenderIdAsync()
    {
        // 可在 appsettings 配置一个固定的“系统发件人”，避免硬编码
        // e.g. "Scheduler": { "SystemNoticeSenderId": "1678330902595375105" }
        var cfg = _configuration["Scheduler:SystemNoticeSenderId"];
        if (long.TryParse(cfg, out var senderId) && senderId > 0)
        {
            var exists = await _adminRepo.AsQueryable()
                .Where(x => x.Id == senderId && x.Status && !x.IsDel)
                .AnyAsync();
            if (exists) return senderId;
        }

        // fallback：取一个可用管理员作为发件人
        var id = await _adminRepo.AsQueryable()
            .Where(x => x.Status && !x.IsDel)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();

        return id;
    }

    private static DateTime ComputeNextRunTime(DateTime currentNext, string? cycleType, int cycleValue, DateTime now)
    {
        var v = cycleValue <= 0 ? 1 : cycleValue;
        var type = (cycleType ?? string.Empty).Trim().ToUpperInvariant();

        // DAY/WEEK 可用数学推进，避免 while 循环过多
        if (type == "DAY" || type == "WEEK")
        {
            var stepDays = type == "WEEK" ? 7 * v : v;
            if (stepDays <= 0) stepDays = 1;
            if (currentNext > now) return currentNext;

            var deltaDays = (now - currentNext).TotalDays;
            var steps = (long)Math.Floor(deltaDays / stepDays) + 1;
            return currentNext.AddDays(steps * stepDays);
        }

        // MONTH/YEAR 使用 AddMonths/AddYears 推进（处理月底等边界）
        var next = currentNext;
        var guard = 0;
        while (next <= now && guard++ < 5000)
        {
            next = type switch
            {
                "YEAR" => next.AddYears(v),
                "MONTH" => next.AddMonths(v),
                _ => next.AddMonths(v),
            };
        }
        return next;
    }
}
