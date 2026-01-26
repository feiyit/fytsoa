using System.Text.Json;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产盘点计划服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmInventoryPlanService : IApplicationService
{
    private readonly SugarRepository<AmInventoryPlan> _thisRepository;
    private readonly SugarRepository<AmAssetHistory> _historyRepository;

    public AmInventoryPlanService(
        SugarRepository<AmInventoryPlan> thisRepository,
        SugarRepository<AmAssetHistory> historyRepository)
    {
        _thisRepository = thisRepository;
        _historyRepository = historyRepository;
    }

    /// <summary>
    /// 盘点计划分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmInventoryPlanDto>> PagesAsync([FromBody] AmInventoryPlanParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.PlanStatus != 0, x => x.Status == (byte)param.PlanStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.PlanNo.Contains(param.Key) || x.Name.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        // 列表页不返回 Items，避免额外开销
        return page.Adapt<PageResult<AmInventoryPlanDto>>();
    }

    /// <summary>
    /// 盘点计划详情（含明细）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<AmInventoryPlanDto> GetAsync(long id)
    {
        var plan = await _thisRepository.GetByIdAsync(id);
        var dto = plan.Adapt<AmInventoryPlanDto>();

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmInventoryItem>>();
        var items = await itemRepo.AsQueryable()
            .Includes(x => x.AssetObj)
            .Where(x => x.TenantId == plan.TenantId && x.PlanId == id)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToListAsync();
        dto.Items = items.Adapt<List<AmInventoryItemDto>>();
        return dto;
    }

    /// <summary>
    /// 新增盘点计划（可带明细）
    /// </summary>
    [UnitOfWork]
    public async Task<bool> AddAsync(AmInventoryPlanDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var planEntity = model.Adapt<AmInventoryPlan>();
        if (planEntity.Id == 0) planEntity.Id = Unique.Id();
        planEntity.TenantId = tenantId;
        planEntity.CreateTime = DateTime.Now;
        planEntity.UpdateTime = null;

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmInventoryItem>>();
        var items = (model.Items ?? new List<AmInventoryItemDto>()).Select(x =>
        {
            x.TenantId = tenantId;
            x.PlanId = planEntity.Id;
            var e = x.Adapt<AmInventoryItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.PlanId = planEntity.Id;
            return e;
        }).ToList();
        await _thisRepository.InsertAsync(planEntity);
        if (items.Count > 0)
        {
            await itemRepo.InsertRangeAsync(items);
        }

        // 留痕：对计划涉及的每个资产写入一条盘点创建记录
        var histories = items
            .Where(x => x.AssetId != 0)
            .Select(x => AmAssetHistoryUtils.Build(
                tenantId,
                x.AssetId,
                "INVENTORY",
                planEntity.Id,
                "CREATE",
                null,
                new
                {
                    Plan = new
                    {
                        planEntity.Id,
                        planEntity.PlanNo,
                        planEntity.Name,
                        planEntity.Status,
                        planEntity.StartTime,
                        planEntity.EndTime
                    },
                    Item = new
                    {
                        x.Id,
                        x.AssetId,
                        x.ExpectedLocationId,
                        x.ExpectedCustodianId
                    }
                },
                remark: $"新增盘点计划：{planEntity.PlanNo}/{planEntity.Name}"
            )).ToList();

        if (histories.Count > 0)
        {
            await _historyRepository.InsertRangeAsync(histories);
        }

        return true;
    }

    /// <summary>
    /// 修改盘点计划（覆盖式更新明细：先删后插）
    /// </summary>
    public async Task<bool> ModifyAsync(AmInventoryPlanDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        // 变更前快照（用于写入留痕）
        var beforePlan = await _thisRepository.GetByIdAsync(model.Id);
        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmInventoryItem>>();
        var beforeItems = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.PlanId == model.Id)
            .ToListAsync();

        var planEntity = model.Adapt<AmInventoryPlan>();
        planEntity.TenantId = tenantId;
        planEntity.UpdateTime = DateTime.Now;

        var items = (model.Items ?? new List<AmInventoryItemDto>()).Select(x =>
        {
            x.TenantId = tenantId;
            x.PlanId = planEntity.Id;
            var e = x.Adapt<AmInventoryItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.PlanId = planEntity.Id;
            return e;
        }).ToList();

        var assetIds = beforeItems.Select(x => x.AssetId)
            .Concat(items.Select(x => x.AssetId))
            .Where(x => x != 0)
            .Distinct()
            .ToList();

        var histories = assetIds.Select(assetId =>
        {
            var beforeForAsset = beforeItems.Where(x => x.AssetId == assetId).Select(x => new
            {
                x.Id,
                x.AssetId,
                x.ExpectedLocationId,
                x.ActualLocationId,
                x.ExpectedCustodianId,
                x.ActualCustodianId,
                x.Result,
                x.ScanTime,
                x.ScanUserId,
                x.Remark
            }).ToList();

            var afterForAsset = items.Where(x => x.AssetId == assetId).Select(x => new
            {
                x.Id,
                x.AssetId,
                x.ExpectedLocationId,
                x.ActualLocationId,
                x.ExpectedCustodianId,
                x.ActualCustodianId,
                x.Result,
                x.ScanTime,
                x.ScanUserId,
                x.Remark
            }).ToList();

            return AmAssetHistoryUtils.Build(
                tenantId,
                assetId,
                "INVENTORY",
                planEntity.Id,
                "UPDATE",
                new
                {
                    Plan = beforePlan == null ? null : new
                    {
                        beforePlan.Id,
                        beforePlan.PlanNo,
                        beforePlan.Name,
                        beforePlan.Status,
                        beforePlan.StartTime,
                        beforePlan.EndTime
                    },
                    Items = beforeForAsset
                },
                new
                {
                    Plan = new
                    {
                        planEntity.Id,
                        planEntity.PlanNo,
                        planEntity.Name,
                        planEntity.Status,
                        planEntity.StartTime,
                        planEntity.EndTime
                    },
                    Items = afterForAsset
                },
                remark: $"修改盘点计划：{planEntity.PlanNo}/{planEntity.Name}"
            );
        }).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(planEntity);
            await itemRepo.DeleteAsync(x => x.TenantId == tenantId && x.PlanId == planEntity.Id);
            if (items.Count > 0)
            {
                await itemRepo.InsertRangeAsync(items);
            }
            if (histories.Count > 0)
            {
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 盘点扫码/录入结果（更新单条明细）
    /// </summary>
    public async Task<bool> ScanAsync(AmInventoryItemDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmInventoryItem>>();
        var before = await itemRepo.AsQueryable()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == model.Id);

        var afterPatch = new AmInventoryItem
        {
            ActualLocationId = model.ActualLocationId,
            ActualCustodianId = model.ActualCustodianId,
            Result = model.Result,
            ScanTime = model.ScanTime ?? DateTime.Now,
            ScanUserId = model.ScanUserId,
            Remark = model.Remark
        };

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await itemRepo.UpdateAsync(
                x => new AmInventoryItem
                {
                    ActualLocationId = afterPatch.ActualLocationId,
                    ActualCustodianId = afterPatch.ActualCustodianId,
                    Result = afterPatch.Result,
                    ScanTime = afterPatch.ScanTime,
                    ScanUserId = afterPatch.ScanUserId,
                    Remark = afterPatch.Remark
                },
                x => x.TenantId == tenantId && x.Id == model.Id);

            if (before.AssetId != 0)
            {
                await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                    tenantId,
                    before.AssetId,
                    "INVENTORY",
                    before.PlanId,
                    "SCAN",
                    before,
                    new
                    {
                        before.Id,
                        before.PlanId,
                        before.AssetId,
                        before.ExpectedLocationId,
                        ActualLocationId = afterPatch.ActualLocationId,
                        before.ExpectedCustodianId,
                        ActualCustodianId = afterPatch.ActualCustodianId,
                        Result = afterPatch.Result,
                        ScanTime = afterPatch.ScanTime,
                        ScanUserId = afterPatch.ScanUserId,
                        Remark = afterPatch.Remark
                    },
                    remark: $"盘点扫码/录入：PlanId={before.PlanId}"
                ));
            }
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 删除盘点计划（同时删除明细）
    /// </summary>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmInventoryItem>>();

        var items = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PlanId))
            .ToListAsync();

        var plans = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToListAsync();

        var histories = items
            .Where(x => x.AssetId != 0)
            .Select(x =>
            {
                var plan = plans.FirstOrDefault(p => p.Id == x.PlanId);
                var planNo = plan?.PlanNo ?? x.PlanId.ToString();
                return AmAssetHistoryUtils.Build(
                    tenantId,
                    x.AssetId,
                    "INVENTORY",
                    x.PlanId,
                    "DELETE",
                    new
                    {
                        Plan = plan == null ? null : new
                        {
                            plan.Id,
                            plan.PlanNo,
                            plan.Name,
                            plan.Status,
                            plan.StartTime,
                            plan.EndTime
                        },
                        Item = x
                    },
                    null,
                    remark: $"删除盘点计划：{planNo}"
                );
            }).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await itemRepo.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.PlanId));
            await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
            if (histories.Count > 0)
            {
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }
}
