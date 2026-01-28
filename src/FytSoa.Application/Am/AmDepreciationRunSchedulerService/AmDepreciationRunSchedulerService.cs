using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧计提：净值回写调度服务（支持手动/任务调度）。
/// </summary>
public class AmDepreciationRunSchedulerService
{
    private readonly SugarRepository<AmDepreciationRun> _runRepo;
    private readonly SugarRepository<AmAsset> _assetRepo;
    private readonly SugarRepository<AmAssetHistory> _historyRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AmDepreciationRunSchedulerService> _logger;

    public AmDepreciationRunSchedulerService(
        SugarRepository<AmDepreciationRun> runRepo,
        SugarRepository<AmAsset> assetRepo,
        SugarRepository<AmAssetHistory> historyRepo,
        IConfiguration configuration,
        ILogger<AmDepreciationRunSchedulerService> logger)
    {
        _runRepo = runRepo;
        _assetRepo = assetRepo;
        _historyRepo = historyRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsManualMode =>
        string.Equals(GetMode(), "Manual", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 根据指定已确认批次的明细回写净值。
    /// force=true 时忽略 Manual 配置。
    /// </summary>
    public async Task<int> ApplyConfirmedRunAsync(long runId, bool force = false)
    {
        if (IsManualMode && !force)
        {
            _logger.LogInformation("DepreciationRun net value sync skipped: mode=Manual.");
            return 0;
        }

        var run = await _runRepo.GetByIdAsync(runId);
        if (run == null || run.Status != 1) return 0;

        var itemRepo = _runRepo.ChangeRepository<SugarRepository<AmDepreciationRunItem>>();
        var items = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == run.TenantId && x.RunId == runId)
            .ToListAsync();

        return await ApplyItemsAsync(items, runId);
    }

    /// <summary>
    /// 根据所有“已确认”批次的最新明细回写净值（按资产取最新）。
    /// force=true 时忽略 Manual 配置。
    /// </summary>
    public async Task<int> ApplyLatestConfirmedRunsAsync(bool force = false)
    {
        if (IsManualMode && !force)
        {
            _logger.LogInformation("DepreciationRun net value sync skipped: mode=Manual.");
            return 0;
        }

        var rows = await _runRepo.Context.Queryable<AmDepreciationRunItem, AmDepreciationRun>((i, r) =>
                new JoinQueryInfos(JoinType.Inner, i.RunId == r.Id))
            .Where((i, r) => i.TenantId == r.TenantId && r.Status == 1)
            .Select((i, r) => new DepRunNetRow
            {
                TenantId = i.TenantId,
                AssetId = i.AssetId,
                NetBookValue = i.NetBookValue,
                RunId = r.Id,
                RunTime = r.RunTime,
                Period = r.Period
            })
            .ToListAsync();

        if (rows.Count == 0) return 0;

        var latest = rows
            .GroupBy(x => new { x.TenantId, x.AssetId })
            .Select(g => g
                .OrderByDescending(x => x.RunTime)
                .ThenByDescending(x => x.Period)
                .First())
            .ToList();

        return await ApplyNetValuesAsync(latest);
    }

    private string GetMode() =>
        _configuration["Depreciation:DepreciationRunMode"] ?? "Manual";

    private async Task<int> ApplyItemsAsync(List<AmDepreciationRunItem> items, long runId)
    {
        if (items.Count == 0) return 0;

        var rows = items
            .GroupBy(x => new { x.TenantId, x.AssetId })
            .Select(g => g.First())
            .Select(x => new DepRunNetRow
            {
                TenantId = x.TenantId,
                AssetId = x.AssetId,
                NetBookValue = x.NetBookValue,
                RunId = runId
            })
            .ToList();

        return await ApplyNetValuesAsync(rows);
    }

    private async Task<int> ApplyNetValuesAsync(List<DepRunNetRow> rows)
    {
        if (rows.Count == 0) return 0;

        var assetIds = rows.Select(x => x.AssetId).Distinct().ToList();
        var assets = await _assetRepo.AsQueryable()
            .Where(a => assetIds.Contains(a.Id))
            .ToListAsync();
        if (assets.Count == 0) return 0;

        var assetMap = assets.ToDictionary(a => a.Id, a => a);
        var updateList = new List<AmAsset>();
        var histories = new List<AmAssetHistory>();
        var operatorId = ResolveOperatorId();

        foreach (var row in rows)
        {
            if (!assetMap.TryGetValue(row.AssetId, out var asset)) continue;
            var rounded = Math.Round(row.NetBookValue, 2, MidpointRounding.AwayFromZero);
            if (asset.NetBookValue == rounded) continue;

            var before = new { NetBookValue = asset.NetBookValue };
            var after = new { NetBookValue = rounded };
            asset.NetBookValue = rounded;
            updateList.Add(asset);

            histories.Add(AmAssetHistoryUtils.Build(
                row.TenantId,
                asset.Id,
                "DEPRUN",
                row.RunId,
                "NET_VALUE",
                before,
                after,
                remark: string.IsNullOrEmpty(row.Period)
                    ? $"折旧计提净值回写：{asset.AssetNo}/{asset.Name}"
                    : $"折旧计提净值回写({row.Period})：{asset.AssetNo}/{asset.Name}",
                operatorId: operatorId
            ));
        }

        if (updateList.Count > 0)
        {
            var tran = await _assetRepo.Context.Ado.UseTranAsync(async () =>
            {
                await _assetRepo.UpdateRangeAsync(updateList);
                if (histories.Count > 0)
                {
                    await _historyRepo.InsertRangeAsync(histories);
                }
            });
            if (!tran.IsSuccess) return 0;
        }

        return updateList.Count;
    }

    private long ResolveOperatorId()
    {
        var cfg = _configuration["Depreciation:NetValueOperatorId"];
        return long.TryParse(cfg, out var id) && id > 0 ? id : 0;
    }

    private sealed class DepRunNetRow
    {
        public long TenantId { get; set; }
        public long AssetId { get; set; }
        public decimal NetBookValue { get; set; }
        public long RunId { get; set; }
        public DateTime RunTime { get; set; }
        public string? Period { get; set; }
    }
}
