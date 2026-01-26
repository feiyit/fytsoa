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
/// 地点服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmLocationService : IApplicationService
{
    private readonly SugarRepository<AmLocation> _thisRepository;

    public AmLocationService(SugarRepository<AmLocation> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    [HttpPost]
    public async Task<PageResult<AmLocationDto>> PagesAsync([FromBody] AmLocationParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.ParentId != 0, x => x.ParentId == param.ParentId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.Name.Contains(param.Key) || (x.Code != null && x.Code.Contains(param.Key)))
            .WhereIF(param.Status == "1", x => x.Status)
            .WhereIF(param.Status == "2", x => !x.Status);

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Sort, OrderByType.Asc)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmLocationDto>>();
    }

    public async Task<List<AmLocationDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.Name.Contains(param.Key))
            .WhereIF(param.Status == "1", x => x.Status)
            .WhereIF(param.Status == "2", x => !x.Status)
            .OrderBy(x => x.Sort, OrderByType.Asc)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToListAsync();
        return list.Adapt<List<AmLocationDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmLocationDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmLocationDto>();
    }

    public async Task<bool> AddAsync(AmLocationDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmLocation>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;

        // ParentIdList：如未传，按 ParentId 补一个最小可用链路
        if (entity.ParentIdList == null || entity.ParentIdList.Count == 0)
        {
            entity.ParentIdList = entity.ParentId == 0
                ? new List<long> { 0, entity.Id }
                : new List<long> { entity.ParentId, entity.Id };
        }
        else
        {
            if (!entity.ParentIdList.Contains(entity.Id))
            {
                entity.ParentIdList.Add(entity.Id);
            }
        }

        entity.Layer = entity.ParentIdList.Count;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmLocationDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmLocation>();
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
