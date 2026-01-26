using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产提醒任务服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmReminderTaskService : IApplicationService
{
    private readonly SugarRepository<AmReminderTask> _thisRepository;

    public AmReminderTaskService(SugarRepository<AmReminderTask> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 提醒任务分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmReminderTaskDto>> PagesAsync([FromBody] AmReminderTaskParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.RuleId != 0, x => x.RuleId == param.RuleId)
            .WhereIF(!string.IsNullOrEmpty(param.BizType), x => x.BizType == param.BizType)
            .WhereIF(param.BizId != 0, x => x.BizId == param.BizId)
            .WhereIF(param.ReceiverUserId != 0, x => x.ReceiverUserId == param.ReceiverUserId)
            .WhereIF(param.TaskStatus != 0, x => x.Status == (byte)param.TaskStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.Title.Contains(param.Key) || (x.Content != null && x.Content.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmReminderTaskDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmReminderTaskDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmReminderTaskDto>();
    }

    public async Task<bool> AddAsync(AmReminderTaskDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmReminderTask>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmReminderTaskDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmReminderTask>();
        entity.TenantId = tenantId;
        return await _thisRepository.UpdateAsync(entity);
    }

    /// <summary>
    /// 标记已读
    /// </summary>
    public async Task<bool> ReadAsync(long id)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.UpdateAsync(
            x => new AmReminderTask { Status = 2, ReadTime = DateTime.Now },
            x => x.TenantId == tenantId && x.Id == id);
    }

    /// <summary>
    /// 关闭任务
    /// </summary>
    public async Task<bool> CloseAsync(long id)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.UpdateAsync(
            x => new AmReminderTask { Status = 3 },
            x => x.TenantId == tenantId && x.Id == id);
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
    }
}

