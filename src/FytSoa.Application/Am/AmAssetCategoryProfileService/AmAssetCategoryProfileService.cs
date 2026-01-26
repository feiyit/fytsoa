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
/// 资产分类扩展信息服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmAssetCategoryProfileService : IApplicationService
{
    private readonly SugarRepository<AmAssetCategoryProfile> _thisRepository;

    public AmAssetCategoryProfileService(SugarRepository<AmAssetCategoryProfile> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    [HttpPost]
    public async Task<PageResult<AmAssetCategoryProfileDto>> PagesAsync([FromBody] AmAssetCategoryProfileParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.CategoryId != 0, x => x.CategoryId == param.CategoryId);

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.Includes(m=>m.CategoryObj).OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmAssetCategoryProfileDto>>();
    }

    public async Task<List<AmAssetCategoryProfileDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .Includes(m=>m.CategoryObj)
            .OrderBy(x => x.Id, OrderByType.Desc)
            .ToListAsync();
        return list.Adapt<List<AmAssetCategoryProfileDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmAssetCategoryProfileDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmAssetCategoryProfileDto>();
    }

    public async Task<bool> AddAsync(AmAssetCategoryProfileDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetCategoryProfile>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmAssetCategoryProfileDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetCategoryProfile>();
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

