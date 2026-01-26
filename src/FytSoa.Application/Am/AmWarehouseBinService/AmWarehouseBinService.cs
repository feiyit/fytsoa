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
/// 仓库库位服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmWarehouseBinService : IApplicationService
{
    private readonly SugarRepository<AmWarehouseBin> _thisRepository;

    public AmWarehouseBinService(SugarRepository<AmWarehouseBin> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    [HttpPost]
    public async Task<PageResult<AmWarehouseBinDto>> PagesAsync([FromBody] AmWarehouseBinParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.WarehouseId != 0, x => x.WarehouseId == param.WarehouseId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.Name.Contains(param.Key) || (x.Code != null && x.Code.Contains(param.Key)))
            .WhereIF(param.Status == "1", x => x.Status)
            .WhereIF(param.Status == "2", x => !x.Status);

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query
            .Includes(m=>m.WarehouseObj)
            .OrderBy(x => x.Sort, OrderByType.Asc)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmWarehouseBinDto>>();
    }

    public async Task<List<AmWarehouseBinDto>> GetListAsync(WhereParam param)
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
        return list.Adapt<List<AmWarehouseBinDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmWarehouseBinDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmWarehouseBinDto>();
    }

    public async Task<bool> AddAsync(AmWarehouseBinDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmWarehouseBin>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmWarehouseBinDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmWarehouseBin>();
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

