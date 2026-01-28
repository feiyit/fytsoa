using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧配置：净值计算调度服务（支持手动/任务调度）。
/// </summary>
public class AmAssetDepreciationSchedulerService
{
    private readonly SugarRepository<AmAssetDepreciation> _depRepo;
    private readonly SugarRepository<AmAsset> _assetRepo;
    private readonly SugarRepository<AmAssetHistory> _historyRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AmAssetDepreciationSchedulerService> _logger;

    public AmAssetDepreciationSchedulerService(
        SugarRepository<AmAssetDepreciation> depRepo,
        SugarRepository<AmAsset> assetRepo,
        SugarRepository<AmAssetHistory> historyRepo,
        IConfiguration configuration,
        ILogger<AmAssetDepreciationSchedulerService> logger)
    {
        _depRepo = depRepo;
        _assetRepo = assetRepo;
        _historyRepo = historyRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsManualMode =>
        string.Equals(GetMode(), "Manual", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 根据折旧配置计算资产净值（仅 Status=折旧中）。
    /// force=true 时忽略 Manual 配置。
    /// </summary>
    public async Task<int> RecalcAssetNetBookValueAsync(bool force = false)
    {
        //Console.WriteLine($"IsManualMode:{IsManualMode}");
        //Console.WriteLine($"force:{force}");
        if (IsManualMode && !force)
        {
            _logger.LogInformation("AssetDepreciation calc skipped: mode=Manual.");
            return 0;
        }

        var now = DateTime.Now.Date;
        var configs = await _depRepo.AsQueryable()
            .Where(x => x.Status == 1)
            .Where(x => x.Method != 0)
            .Where(x => x.LifeMonths > 0)
            .Where(x => x.StartDate != null)
            .ToListAsync();
        //Console.WriteLine($"configs总数:{configs.Count}");
        if (configs.Count == 0) return 0;

        var assetIds = configs.Select(x => x.AssetId).Distinct().ToList();
        var assets = await _assetRepo.AsQueryable()
            .Where(a => assetIds.Contains(a.Id))
            .ToListAsync();
        if (assets.Count == 0) return 0;
        //Console.WriteLine($"资产总数:{assets.Count}");
        var assetMap = assets.ToDictionary(a => a.Id, a => a);
        var updateList = new List<AmAsset>();
        var histories = new List<AmAssetHistory>();
        var operatorId = ResolveOperatorId();

        foreach (var cfg in configs)
        {
            if (!assetMap.TryGetValue(cfg.AssetId, out var asset)) continue;
            if (cfg.StartDate == null) continue;

            var netValue = ComputeNetBookValue(asset.OriginalValue, cfg, now);
            if (netValue == null) continue;

            var rounded = Math.Round(netValue.Value, 2, MidpointRounding.AwayFromZero);
            if (asset.NetBookValue == rounded) continue;

            var before = new { NetBookValue = asset.NetBookValue };
            var after = new { NetBookValue = rounded };
            asset.NetBookValue = rounded;
            updateList.Add(asset);

            histories.Add(AmAssetHistoryUtils.Build(
                cfg.TenantId,
                asset.Id,
                "DEPCFG",
                cfg.Id,
                "NET_VALUE",
                before,
                after,
                remark: $"折旧配置净值计算：{asset.AssetNo}/{asset.Name}",
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

    private string GetMode() =>
        _configuration["Depreciation:AssetDepreciationMode"] ?? "Manual";

    private long ResolveOperatorId()
    {
        var cfg = _configuration["Depreciation:NetValueOperatorId"];
        return long.TryParse(cfg, out var id) && id > 0 ? id : 0;
    }

    private static decimal? ComputeNetBookValue(
        decimal originalValue,
        AmAssetDepreciation cfg,
        DateTime now)
    {
        var start = cfg.StartDate?.Date;
        if (start == null) return null;
        if (cfg.LifeMonths <= 0) return null;
        if (originalValue < 0) return 0m;

        var months = CalcElapsedMonths(start.Value, now);
        if (months <= 0) return originalValue;
        if (months > cfg.LifeMonths) months = cfg.LifeMonths;

        var salvageRate = cfg.SalvageRate;
        if (salvageRate < 0) salvageRate = 0;
        if (salvageRate > 100) salvageRate = 100;

        var salvageValue = originalValue * salvageRate / 100m;
        if (salvageValue < 0) salvageValue = 0;
        if (salvageValue > originalValue) salvageValue = originalValue;

        var depreciableBase = originalValue - salvageValue;
        if (depreciableBase <= 0) return salvageValue;

        decimal accum = 0m;
        switch (cfg.Method)
        {
            case 1: // 直线法
                accum = depreciableBase * months / cfg.LifeMonths;
                break;
            case 2: // 双倍余额递减法（按月）
                accum = CalcDoubleDecliningAccum(originalValue, salvageValue, cfg.LifeMonths, months);
                break;
            case 3: // 年数总和法（按月）
                accum = CalcSumOfYearsAccum(depreciableBase, cfg.LifeMonths, months);
                break;
            default:
                return originalValue;
        }

        var netValue = originalValue - accum;
        if (netValue < salvageValue) netValue = salvageValue;
        if (netValue < 0) netValue = 0;
        return netValue;
    }

    private static int CalcElapsedMonths(DateTime start, DateTime now)
    {
        if (now < start) return 0;
        var months = (now.Year - start.Year) * 12 + (now.Month - start.Month);
        if (now.Day >= start.Day) months += 1;
        return months;
    }

    private static decimal CalcDoubleDecliningAccum(
        decimal originalValue,
        decimal salvageValue,
        int lifeMonths,
        int months)
    {
        if (lifeMonths <= 0 || months <= 0) return 0m;

        var rate = 2m / lifeMonths;
        var bookValue = originalValue;
        decimal accum = 0m;

        for (var i = 0; i < months; i++)
        {
            var dep = bookValue * rate;
            if (bookValue - dep < salvageValue)
            {
                dep = bookValue - salvageValue;
            }

            if (dep <= 0) break;
            bookValue -= dep;
            accum += dep;

            if (bookValue <= salvageValue) break;
        }

        return accum;
    }

    private static decimal CalcSumOfYearsAccum(decimal depreciableBase, int lifeMonths, int months)
    {
        if (lifeMonths <= 0 || months <= 0) return 0m;
        if (months > lifeMonths) months = lifeMonths;

        var total = lifeMonths * (lifeMonths + 1) / 2m;
        var sumRemaining = months * (2m * lifeMonths - months + 1) / 2m;
        return depreciableBase * (sumRemaining / total);
    }
}
