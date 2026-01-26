using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Application.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产台账服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmAssetService : IApplicationService
{
    private readonly SugarRepository<AmAsset> _thisRepository;
    private readonly SugarRepository<AmAssetHistory> _historyRepository;

    public AmAssetService(
        SugarRepository<AmAsset> thisRepository,
        SugarRepository<AmAssetHistory> historyRepository)
    {
        _thisRepository = thisRepository;
        _historyRepository = historyRepository;
    }

    /// <summary>
    /// 资产分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmAssetDto>> PagesAsync([FromBody] AmAssetParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!param.IncludeDeleted, x => !x.IsDel)
            .WhereIF(param.CategoryId != 0, x => x.CategoryId == param.CategoryId)
            .WhereIF(param.OrgUnitId != 0, x => x.OrgUnitId == param.OrgUnitId)
            .WhereIF(param.LocationId != 0, x => x.LocationId == param.LocationId)
            .WhereIF(param.WarehouseId != 0, x => x.WarehouseId == param.WarehouseId)
            .WhereIF(param.VendorId != 0, x => x.VendorId == param.VendorId)
            .WhereIF(param.AssetStatus != 0, x => x.Status == (byte)param.AssetStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.AssetNo.Contains(param.Key) ||
                     (x.TagCode != null && x.TagCode.Contains(param.Key)) ||
                     x.Name.Contains(param.Key) ||
                     (x.Brand != null && x.Brand.Contains(param.Key)) ||
                     (x.Model != null && x.Model.Contains(param.Key)) ||
                     (x.SerialNo != null && x.SerialNo.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query
            .Includes(x => x.CategoryObj)
            .Includes(x => x.LocationObj)
            .OrderBy(x => x.Id, OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmAssetDto>>();
    }

    /// <summary>
    /// Export assets to Excel (.xls).
    /// </summary>
    [HttpPost,NoJsonResult]
    public async Task<FileContentResult> ExportAsync([FromBody] AmAssetParam param)
    {
        const int maxRows = 10000;
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;

        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!param.IncludeDeleted, x => !x.IsDel)
            .WhereIF(param.CategoryId != 0, x => x.CategoryId == param.CategoryId)
            .WhereIF(param.OrgUnitId != 0, x => x.OrgUnitId == param.OrgUnitId)
            .WhereIF(param.LocationId != 0, x => x.LocationId == param.LocationId)
            .WhereIF(param.WarehouseId != 0, x => x.WarehouseId == param.WarehouseId)
            .WhereIF(param.VendorId != 0, x => x.VendorId == param.VendorId)
            .WhereIF(param.AssetStatus != 0, x => x.Status == (byte)param.AssetStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.AssetNo.Contains(param.Key) ||
                     (x.TagCode != null && x.TagCode.Contains(param.Key)) ||
                     x.Name.Contains(param.Key) ||
                     (x.Brand != null && x.Brand.Contains(param.Key)) ||
                     (x.Model != null && x.Model.Contains(param.Key)) ||
                     (x.SerialNo != null && x.SerialNo.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var list = await query
            .Includes(x => x.CategoryObj)
            .OrderBy(x => x.Id, OrderByType.Desc)
            .Take(maxRows)
            .ToListAsync();

        var headers = new[]
        {
            "资产Id","资产编号","资产名称","分类","品牌","型号","标签码","状态","创建时间"
        };

        var rows = list.Select(x => (IReadOnlyList<object?>)new object?[]
        {
            x.Id,
            x.AssetNo,
            x.Name,
            x.CategoryObj?.Name,
            x.Brand,
            x.Model,
            x.TagCode,
            x.Status,
            x.CreateTime
        });

        var title = "资产台账";
        var bytes = ExcelExportUtils.ToHtmlXls(title, headers, rows);
        var fileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xls";
        return new FileContentResult(bytes, "application/vnd.ms-excel")
        {
            FileDownloadName = fileName
        };
    }

    /// <summary>
    /// 查询资产列表（用于下拉/简单查询）
    /// </summary>
    public async Task<List<AmAssetDto>> GetListAsync(WhereParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var list = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && !x.IsDel)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.AssetNo.Contains(param.Key) || x.Name.Contains(param.Key))
            .OrderBy(x => x.Id, OrderByType.Desc)
            .ToListAsync();
        return list.Adapt<List<AmAssetDto>>();
    }

    /// <summary>
    /// 根据主键查询资产
    /// </summary>
    [HttpGet("{id}")]
    public async Task<AmAssetDto> GetAsync(long id)
    {
        var model = await _thisRepository.AsQueryable()
            .Includes(x => x.CategoryObj)
            .Includes(x => x.LocationObj)
            .FirstAsync(x => x.Id == id);
        return model.Adapt<AmAssetDto>();
    }

    /// <summary>
    /// 新增资产
    /// </summary>
    public async Task<bool> AddAsync(AmAssetDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAsset>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.InsertAsync(entity);
            await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                tenantId,
                entity.Id,
                "ASSET",
                entity.Id,
                "CREATE",
                null,
                entity,
                remark: $"新增资产：{entity.AssetNo}/{entity.Name}"
            ));
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 修改资产
    /// </summary>
    public async Task<bool> ModifyAsync(AmAssetDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var before = await _thisRepository.AsQueryable()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == model.Id);

        var entity = model.Adapt<AmAsset>();
        entity.TenantId = tenantId;
        entity.UpdateTime = DateTime.Now;

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(entity);
            await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                tenantId,
                entity.Id,
                "ASSET",
                entity.Id,
                "UPDATE",
                before,
                entity,
                remark: $"修改资产：{entity.AssetNo}/{entity.Name}"
            ));
        });

        return tran.IsSuccess;
    }

    /// <summary>
    /// 删除资产（逻辑删除：IsDel=true）
    /// </summary>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        var beforeList = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToListAsync();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(
                x => new AmAsset { IsDel = true, UpdateTime = DateTime.Now },
                x => x.TenantId == tenantId && ids.Contains(x.Id));

            if (beforeList.Count > 0)
            {
                var histories = beforeList.Select(a => AmAssetHistoryUtils.Build(
                    tenantId,
                    a.Id,
                    "ASSET",
                    a.Id,
                    "DELETE",
                    a,
                    new { IsDel = true },
                    remark: $"删除资产（逻辑删除）：{a.AssetNo}/{a.Name}"
                )).ToList();
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }
}
