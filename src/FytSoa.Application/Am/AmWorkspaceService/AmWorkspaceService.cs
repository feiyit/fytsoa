using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产管理 - 数据统计中心（工作台）服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmWorkspaceService : IApplicationService
{
    private readonly SugarRepository<AmAsset> _assetRepository;

    public AmWorkspaceService(SugarRepository<AmAsset> assetRepository)
    {
        _assetRepository = assetRepository;
    }

    /// <summary>
    /// 获取资产管理工作台汇总信息
    /// 路由示例：GET /amworkspace/summary?year=2026&amp;dueSoonDays=7
    /// </summary>
    public async Task<AmWorkspaceSummaryDto> GetSummaryAsync(long tenantId = 0, int year = 0, int dueSoonDays = 7)
    {
        tenantId = tenantId != 0 ? tenantId : AppUtils.TenantId;
        year = year != 0 ? year : DateTime.Now.Year;
        if (dueSoonDays <= 0 || dueSoonDays > 365) dueSoonDays = 7;

        var now = DateTime.Now;
        var dueSoonEnd = now.AddDays(dueSoonDays);

        // 通过 ChangeRepository 复用同一套 DbContext
        var vendorRepo = _assetRepository.ChangeRepository<SugarRepository<AmVendor>>();
        var locationRepo = _assetRepository.ChangeRepository<SugarRepository<AmLocation>>();
        var warehouseRepo = _assetRepository.ChangeRepository<SugarRepository<AmWarehouse>>();
        var binRepo = _assetRepository.ChangeRepository<SugarRepository<AmWarehouseBin>>();
        var inventoryPlanRepo = _assetRepository.ChangeRepository<SugarRepository<AmInventoryPlan>>();
        var maintenancePlanRepo = _assetRepository.ChangeRepository<SugarRepository<AmMaintenancePlan>>();
        var maintenanceOrderRepo = _assetRepository.ChangeRepository<SugarRepository<AmMaintenanceOrder>>();
        var docRepo = _assetRepository.ChangeRepository<SugarRepository<AmDoc>>();
        var assetDepRepo = _assetRepository.ChangeRepository<SugarRepository<AmAssetDepreciation>>();
        var depRunRepo = _assetRepository.ChangeRepository<SugarRepository<AmDepreciationRun>>();
        var reminderRuleRepo = _assetRepository.ChangeRepository<SugarRepository<AmReminderRule>>();
        var reminderTaskRepo = _assetRepository.ChangeRepository<SugarRepository<AmReminderTask>>();

        var res = new AmWorkspaceSummaryDto
        {
            TenantId = tenantId,
            Year = year,
            DueSoonDays = dueSoonDays,
            StatsTime = now
        };

        // ========== 资产 ==========
        var assetQuery = _assetRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && !x.IsDel);

        res.AssetTotal = await assetQuery.CountAsync();
        res.AssetDelTotal = await _assetRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.IsDel)
            .CountAsync();
        res.AssetOriginalValueTotal = await assetQuery.SumAsync(x => x.OriginalValue);
        res.AssetNetBookValueTotal = await assetQuery.SumAsync(x => x.NetBookValue);
        res.AssetWarrantyOverdueTotal = await assetQuery
            .Where(x => x.WarrantyExpireDate != null && x.WarrantyExpireDate < now)
            .CountAsync();
        res.AssetWarrantyDueSoonTotal = await assetQuery
            .Where(x => x.WarrantyExpireDate != null && x.WarrantyExpireDate >= now && x.WarrantyExpireDate <= dueSoonEnd)
            .CountAsync();

        res.AssetStatusStats = await BuildAssetStatusStatsAsync(assetQuery);
        res.AssetCreatedByMonth = await BuildCreatedByMonthAsync(assetQuery, year);
        res.AssetCategoryTopStats = await BuildTopCategoryStatsAsync(tenantId);

        // ========== 主数据 ==========
        res.VendorTotal = await vendorRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.VendorEnabledTotal = await vendorRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status)
            .CountAsync();

        res.LocationTotal = await locationRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.LocationEnabledTotal = await locationRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status)
            .CountAsync();

        res.WarehouseTotal = await warehouseRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.WarehouseEnabledTotal = await warehouseRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status)
            .CountAsync();

        res.WarehouseBinTotal = await binRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();

        // ========== 盘点 ==========
        res.InventoryPlanTotal = await inventoryPlanRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.InventoryPlanRunningTotal = await inventoryPlanRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status == 1)
            .CountAsync();
        res.InventoryPlanStatusStats = await BuildInventoryPlanStatusStatsAsync(tenantId, inventoryPlanRepo);

        // ========== 维修/保养 ==========
        res.MaintenancePlanTotal = await maintenancePlanRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.MaintenancePlanEnabledTotal = await maintenancePlanRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.IsEnabled)
            .CountAsync();

        res.MaintenanceOrderTotal = await maintenanceOrderRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.MaintenanceOrderOpenTotal = await maintenanceOrderRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && (x.Status == 1 || x.Status == 2 || x.Status == 3))
            .CountAsync();
        res.MaintenanceOrderStatusStats = await BuildMaintenanceOrderStatusStatsAsync(tenantId, maintenanceOrderRepo);
        res.MaintenanceOrderTypeStats = await BuildMaintenanceOrderTypeStatsAsync(tenantId, maintenanceOrderRepo);

        // ========== 单据 ==========
        res.DocTotal = await docRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();

        // ========== 折旧 ==========
        res.AssetDepreciationTotal = await assetDepRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.AssetDepreciationEnabledTotal = await assetDepRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status == 1)
            .CountAsync();
        res.AssetDepreciationAccumAmountTotal = await assetDepRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .SumAsync(x => x.AccumAmount);

        res.DepreciationRunTotal = await depRunRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.DepreciationRunTotalAmountAll = await depRunRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .SumAsync(x => x.TotalAmount);
        if (res.DepreciationRunTotal > 0)
        {
            var lastRun = await depRunRepo.AsQueryable()
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.RunTime, OrderByType.Desc)
                .FirstAsync();
            res.LastDepreciationRunPeriod = lastRun?.Period;
        }

        // ========== 提醒 ==========
        res.ReminderRuleTotal = await reminderRuleRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.ReminderRuleEnabledTotal = await reminderRuleRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.IsEnabled)
            .CountAsync();

        res.ReminderTaskTotal = await reminderTaskRepo.AsQueryable().Where(x => x.TenantId == tenantId).CountAsync();
        res.ReminderTaskOpenTotal = await reminderTaskRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status != 3)
            .CountAsync();
        res.ReminderTaskOverdueTotal = await reminderTaskRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status != 3 && x.DueTime != null && x.DueTime < now)
            .CountAsync();
        res.ReminderTaskDueSoonTotal = await reminderTaskRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.Status != 3 && x.DueTime != null && x.DueTime >= now && x.DueTime <= dueSoonEnd)
            .CountAsync();
        res.ReminderTaskStatusStats = await BuildReminderTaskStatusStatsAsync(tenantId, reminderTaskRepo);

        return res;
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildCreatedByMonthAsync(
        ISugarQueryable<AmAsset> baseQuery,
        int year)
    {
        var list = await baseQuery
            .Where(m => m.CreateTime.Year == year)
            .GroupBy(m => m.CreateTime.Month)
            .Select(m => new
            {
                count = SqlFunc.AggregateCount(m.Id),
                month = m.CreateTime.Month
            })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(12);
        for (var i = 1; i < 13; i++)
        {
            var model = list.Find(m => m.month == i);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = i.ToString(),
                Name = $"{i}月",
                Value = model?.count ?? 0
            });
        }

        return res;
    }

    private async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildTopCategoryStatsAsync(long tenantId)
    {
        // 资产分类来自 sys_code（AmAsset.CategoryId -> SysCode.Id）
        var list = await _assetRepository.Context.Queryable<AmAsset, SysCode>((a, c) =>
                new JoinQueryInfos(JoinType.Left, a.CategoryId == c.Id))
            .Where((a, c) => a.TenantId == tenantId && !a.IsDel)
            .GroupBy((a, c) => new { a.CategoryId, c.Name })
            .Select((a, c) => new
            {
                categoryId = a.CategoryId,
                name = c.Name,
                count = SqlFunc.AggregateCount(a.Id)
            })
            .MergeTable()
            .OrderBy(a => a.count, OrderByType.Desc)
            .Take(10)
            .ToListAsync();

        return list.Select(x => new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
        {
            Key = x.categoryId.ToString(),
            Name = string.IsNullOrEmpty(x.name) ? $"分类{x.categoryId}" : x.name,
            Value = x.count
        }).ToList();
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildAssetStatusStatsAsync(
        ISugarQueryable<AmAsset> baseQuery)
    {
        var map = new Dictionary<byte, string>
        {
            [1] = "在库",
            [2] = "在用",
            [3] = "借出",
            [4] = "维修中",
            [5] = "闲置",
            [6] = "在途",
            [7] = "处置中",
            [8] = "已处置",
        };

        var list = await baseQuery
            .GroupBy(m => m.Status)
            .Select(m => new
            {
                status = m.Status,
                count = SqlFunc.AggregateCount(m.Id)
            })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(map.Count);
        foreach (var kv in map.OrderBy(x => x.Key))
        {
            var model = list.Find(x => x.status == kv.Key);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = kv.Key.ToString(),
                Name = kv.Value,
                Value = model?.count ?? 0
            });
        }

        return res;
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildInventoryPlanStatusStatsAsync(
        long tenantId,
        SugarRepository<AmInventoryPlan> repo)
    {
        var map = new Dictionary<byte, string>
        {
            [0] = "草稿",
            [1] = "进行中",
            [2] = "已完成",
            [3] = "已取消",
        };

        var list = await repo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.Status)
            .Select(x => new { status = x.Status, count = SqlFunc.AggregateCount(x.Id) })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(map.Count);
        foreach (var kv in map.OrderBy(x => x.Key))
        {
            var model = list.Find(x => x.status == kv.Key);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = kv.Key.ToString(),
                Name = kv.Value,
                Value = model?.count ?? 0
            });
        }

        return res;
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildMaintenanceOrderStatusStatsAsync(
        long tenantId,
        SugarRepository<AmMaintenanceOrder> repo)
    {
        var map = new Dictionary<byte, string>
        {
            [0] = "草稿",
            [1] = "待受理",
            [2] = "已指派",
            [3] = "处理中",
            [4] = "已完成",
            [5] = "已关闭",
            [6] = "已取消",
        };

        var list = await repo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.Status)
            .Select(x => new { status = x.Status, count = SqlFunc.AggregateCount(x.Id) })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(map.Count);
        foreach (var kv in map.OrderBy(x => x.Key))
        {
            var model = list.Find(x => x.status == kv.Key);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = kv.Key.ToString(),
                Name = kv.Value,
                Value = model?.count ?? 0
            });
        }

        return res;
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildMaintenanceOrderTypeStatsAsync(
        long tenantId,
        SugarRepository<AmMaintenanceOrder> repo)
    {
        var map = new Dictionary<byte, string>
        {
            [1] = "报修",
            [2] = "保养",
        };

        var list = await repo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.Type)
            .Select(x => new { type = x.Type, count = SqlFunc.AggregateCount(x.Id) })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(map.Count);
        foreach (var kv in map.OrderBy(x => x.Key))
        {
            var model = list.Find(x => x.type == kv.Key);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = kv.Key.ToString(),
                Name = kv.Value,
                Value = model?.count ?? 0
            });
        }

        return res;
    }

    private static async Task<List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>> BuildReminderTaskStatusStatsAsync(
        long tenantId,
        SugarRepository<AmReminderTask> repo)
    {
        var map = new Dictionary<byte, string>
        {
            [0] = "待发送",
            [1] = "已发送",
            [2] = "已读",
            [3] = "已关闭",
        };

        var list = await repo.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.Status)
            .Select(x => new { status = x.Status, count = SqlFunc.AggregateCount(x.Id) })
            .ToListAsync();

        var res = new List<AmWorkspaceSummaryDto.AmWorkspaceStatItemDto>(map.Count);
        foreach (var kv in map.OrderBy(x => x.Key))
        {
            var model = list.Find(x => x.status == kv.Key);
            res.Add(new AmWorkspaceSummaryDto.AmWorkspaceStatItemDto
            {
                Key = kv.Key.ToString(),
                Name = kv.Value,
                Value = model?.count ?? 0
            });
        }

        return res;
    }
}
