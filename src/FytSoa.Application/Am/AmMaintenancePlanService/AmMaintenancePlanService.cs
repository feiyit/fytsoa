using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 保养计划服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmMaintenancePlanService : IApplicationService
{
    private readonly SugarRepository<AmMaintenancePlan> _thisRepository;

    public AmMaintenancePlanService(SugarRepository<AmMaintenancePlan> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 保养计划分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmMaintenancePlanDto>> PagesAsync([FromBody] AmMaintenancePlanParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.Enabled == 1, x => x.IsEnabled)
            .WhereIF(param.Enabled == 2, x => !x.IsEnabled)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.PlanNo.Contains(param.Key) || x.Name.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmMaintenancePlanDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmMaintenancePlanDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmMaintenancePlanDto>();
    }

    public async Task<bool> AddAsync(AmMaintenancePlanDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmMaintenancePlan>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmMaintenancePlanDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmMaintenancePlan>();
        entity.TenantId = tenantId;
        entity.UpdateTime = DateTime.Now;
        return await _thisRepository.UpdateAsync(entity);
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
    }
}

