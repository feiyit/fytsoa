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
/// 供应商服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmVendorService : IApplicationService
{
    private readonly SugarRepository<AmVendor> _thisRepository;

    public AmVendorService(SugarRepository<AmVendor> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 供应商分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmVendorDto>> PagesAsync([FromBody] AmVendorParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.Name.Contains(param.Key) || (x.Code != null && x.Code.Contains(param.Key)))
            .WhereIF(param.Status == "1", x => x.Status)
            .WhereIF(param.Status == "2", x => !x.Status);

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmVendorDto>>();
    }

    /// <summary>
    /// 查询供应商列表
    /// </summary>
    public async Task<List<AmVendorDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.Name.Contains(param.Key))
            .WhereIF(param.Status == "1", x => x.Status)
            .WhereIF(param.Status == "2", x => !x.Status)
            .OrderBy(x => x.Id, OrderByType.Desc)
            .ToListAsync();
        return list.Adapt<List<AmVendorDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmVendorDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmVendorDto>();
    }

    public async Task<bool> AddAsync(AmVendorDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmVendor>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmVendorDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmVendor>();
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

