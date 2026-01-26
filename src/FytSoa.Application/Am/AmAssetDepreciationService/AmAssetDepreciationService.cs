using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产折旧台账/配置服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmAssetDepreciationService : IApplicationService
{
    private readonly SugarRepository<AmAssetDepreciation> _thisRepository;

    public AmAssetDepreciationService(SugarRepository<AmAssetDepreciation> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 折旧配置分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmAssetDepreciationDto>> PagesAsync([FromBody] AmAssetDepreciationParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.AssetId != 0, x => x.AssetId == param.AssetId)
            .WhereIF(param.Method != 0, x => x.Method == (byte)param.Method)
            .WhereIF(param.DepStatus != 0, x => x.Status == (byte)param.DepStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => SqlFunc.ToString(x.AssetId).Contains(param.Key) ||
                     (x.LastPeriod != null && x.LastPeriod.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmAssetDepreciationDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmAssetDepreciationDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmAssetDepreciationDto>();
    }

    public async Task<bool> AddAsync(AmAssetDepreciationDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetDepreciation>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;
        return await _thisRepository.InsertAsync(entity);
    }

    public async Task<bool> ModifyAsync(AmAssetDepreciationDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetDepreciation>();
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

