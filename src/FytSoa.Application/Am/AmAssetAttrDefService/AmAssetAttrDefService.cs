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
/// 资产分类扩展字段定义服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmAssetAttrDefService : IApplicationService
{
    private readonly SugarRepository<AmAssetAttrDef> _thisRepository;

    public AmAssetAttrDefService(SugarRepository<AmAssetAttrDef> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    [HttpPost]
    public async Task<PageResult<AmAssetAttrDefDto>> PagesAsync([FromBody] AmAssetAttrDefParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.CategoryId != 0, x => x.CategoryId == param.CategoryId)
            .WhereIF(param.IsEnabled.HasValue, x => x.IsEnabled == param.IsEnabled!.Value)
            .WhereIF(param.IsRequired.HasValue, x => x.IsRequired == param.IsRequired!.Value)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.FieldKey.Contains(param.Key) || x.FieldName.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.Includes(m=>m.CategoryObj)
            .OrderBy(x => x.Sort, OrderByType.Asc)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmAssetAttrDefDto>>();
    }

    public async Task<List<AmAssetAttrDefDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key), x => x.FieldName.Contains(param.Key))
            .OrderBy(x => x.Sort, OrderByType.Asc)
            .OrderBy(x => x.Id, OrderByType.Asc)
            .ToListAsync();
        return list.Adapt<List<AmAssetAttrDefDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmAssetAttrDefDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmAssetAttrDefDto>();
    }

    public async Task<bool> AddAsync(AmAssetAttrDefDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetAttrDef>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmAssetAttrDefDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetAttrDef>();
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

