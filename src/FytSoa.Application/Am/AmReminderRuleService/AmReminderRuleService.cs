using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产提醒规则服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmReminderRuleService : IApplicationService
{
    private readonly SugarRepository<AmReminderRule> _thisRepository;

    public AmReminderRuleService(SugarRepository<AmReminderRule> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 提醒规则分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmReminderRuleDto>> PagesAsync([FromBody] AmReminderRuleParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.RuleType), x => x.RuleType == param.RuleType)
            .WhereIF(param.Enabled == 1, x => x.IsEnabled)
            .WhereIF(param.Enabled == 2, x => !x.IsEnabled)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.RuleType.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmReminderRuleDto>>();
    }

    public async Task<List<AmReminderRuleDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.Status == "1", x => x.IsEnabled)
            .WhereIF(param.Status == "2", x => !x.IsEnabled)
            .OrderBy(x => x.Id, OrderByType.Desc)
            .ToListAsync();
        return list.Adapt<List<AmReminderRuleDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmReminderRuleDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmReminderRuleDto>();
    }

    public async Task<bool> AddAsync(AmReminderRuleDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmReminderRule>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmReminderRuleDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmReminderRule>();
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
