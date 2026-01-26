using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 折旧计提服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmDepreciationRunService : IApplicationService
{
    private readonly SugarRepository<AmDepreciationRun> _thisRepository;

    public AmDepreciationRunService(SugarRepository<AmDepreciationRun> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 折旧计提批次分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmDepreciationRunDto>> PagesAsync([FromBody] AmDepreciationRunParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Period), x => x.Period == param.Period)
            .WhereIF(param.RunStatus != 0, x => x.Status == (byte)param.RunStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.Period.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.RunTime, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        // 列表页不返回 Items
        return page.Adapt<PageResult<AmDepreciationRunDto>>();
    }

    /// <summary>
    /// 计提详情（含明细）
    /// </summary>
    [HttpGet("{id}")]
    public async Task<AmDepreciationRunDto> GetAsync(long id)
    {
        var run = await _thisRepository.GetByIdAsync(id);
        var dto = run.Adapt<AmDepreciationRunDto>();

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDepreciationRunItem>>();
        var items = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == run.TenantId && x.RunId == id)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToListAsync();
        dto.Items = items.Adapt<List<AmDepreciationRunItemDto>>();
        return dto;
    }

    /// <summary>
    /// 新增计提批次（可带明细）
    /// </summary>
    public async Task<bool> AddAsync(AmDepreciationRunDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var runEntity = model.Adapt<AmDepreciationRun>();
        if (runEntity.Id == 0) runEntity.Id = Unique.Id();
        runEntity.TenantId = tenantId;
        runEntity.RunTime = model.RunTime == default ? DateTime.Now : model.RunTime;

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDepreciationRunItem>>();
        var items = (model.Items ?? new List<AmDepreciationRunItemDto>()).Select(x =>
        {
            x.TenantId = tenantId;
            x.RunId = runEntity.Id;
            var e = x.Adapt<AmDepreciationRunItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.RunId = runEntity.Id;
            return e;
        }).ToList();

        runEntity.TotalAmount = items.Sum(x => x.Amount);

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.InsertAsync(runEntity);
            if (items.Count > 0)
            {
                await itemRepo.InsertRangeAsync(items);
            }
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 修改计提批次（覆盖式更新明细：先删后插）
    /// </summary>
    public async Task<bool> ModifyAsync(AmDepreciationRunDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var runEntity = model.Adapt<AmDepreciationRun>();
        runEntity.TenantId = tenantId;

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDepreciationRunItem>>();
        var items = (model.Items ?? new List<AmDepreciationRunItemDto>()).Select(x =>
        {
            x.TenantId = tenantId;
            x.RunId = runEntity.Id;
            var e = x.Adapt<AmDepreciationRunItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.RunId = runEntity.Id;
            return e;
        }).ToList();

        runEntity.TotalAmount = items.Sum(x => x.Amount);

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(runEntity);
            await itemRepo.DeleteAsync(x => x.TenantId == tenantId && x.RunId == runEntity.Id);
            if (items.Count > 0)
            {
                await itemRepo.InsertRangeAsync(items);
            }
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 确认/过账计提批次（仅修改状态）
    /// </summary>
    public async Task<bool> ConfirmAsync(long id)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.UpdateAsync(
            x => new AmDepreciationRun { Status = 1 },
            x => x.TenantId == tenantId && x.Id == id);
    }

    /// <summary>
    /// 删除计提批次（同时删除明细）
    /// </summary>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDepreciationRunItem>>();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await itemRepo.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.RunId));
            await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
        });

        return tran.IsSuccess;
    }
}

