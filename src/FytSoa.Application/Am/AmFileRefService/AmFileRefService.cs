using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 附件关联服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmFileRefService : IApplicationService
{
    private readonly SugarRepository<AmFileRef> _thisRepository;

    public AmFileRefService(SugarRepository<AmFileRef> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 附件关联分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmFileRefDto>> PagesAsync([FromBody] AmFileRefParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.BizType), x => x.BizType == param.BizType)
            .WhereIF(param.BizId != 0, x => x.BizId == param.BizId)
            .WhereIF(param.FileId != 0, x => x.FileId == param.FileId);

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => new { x.Sort, x.Id }, OrderByType.Asc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmFileRefDto>>();
    }

    /// <summary>
    /// 查询业务对象的附件列表
    /// </summary>
    public async Task<List<AmFileRefDto>> GetListAsync(string bizType, long bizId)
    {
        var tenantId = AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.BizType == bizType && x.BizId == bizId)
            .OrderBy(x => new { x.Sort, x.Id }, OrderByType.Asc)
            .ToListAsync();
        return list.Adapt<List<AmFileRefDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmFileRefDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmFileRefDto>();
    }

    public async Task<bool> AddAsync(AmFileRefDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmFileRef>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        return await _thisRepository.InsertAsync(entity);
    }

    /// <summary>
    /// 批量绑定：先删后插（以 bizType+bizId 为维度）
    /// </summary>
    [HttpPost]
    public async Task<bool> BindAsync([FromBody] AmFileRefBindParam param)
    {
        var tenantId = AppUtils.TenantId;
        var bizType = param.BizType;
        var bizId = param.BizId;
        var entities = (param.Files ?? new List<AmFileRefDto>()).Select((x, idx) =>
        {
            x.TenantId = tenantId;
            x.BizType = bizType;
            x.BizId = bizId;
            if (x.Sort == 0) x.Sort = idx + 1;
            var e = x.Adapt<AmFileRef>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.BizType = bizType;
            e.BizId = bizId;
            e.CreateTime = DateTime.Now;
            return e;
        }).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && x.BizType == bizType && x.BizId == bizId);
            if (entities.Count > 0)
            {
                await _thisRepository.InsertRangeAsync(entities);
            }
        });

        return tran.IsSuccess;
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
    }
}
