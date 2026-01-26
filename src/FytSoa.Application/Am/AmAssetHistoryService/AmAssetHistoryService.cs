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
/// 资产变更历史（留痕）服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmAssetHistoryService : IApplicationService
{
    private readonly SugarRepository<AmAssetHistory> _thisRepository;

    public AmAssetHistoryService(SugarRepository<AmAssetHistory> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 历史分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmAssetHistoryDto>> PagesAsync([FromBody] AmAssetHistoryParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.AssetId != 0, x => x.AssetId == param.AssetId)
            .WhereIF(!string.IsNullOrEmpty(param.BizType), x => x.BizType == param.BizType)
            .WhereIF(param.BizId != 0, x => x.BizId == param.BizId)
            .WhereIF(!string.IsNullOrEmpty(param.Operation), x => x.Operation == param.Operation)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.BizType.Contains(param.Key) || x.Operation.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.OperateTime, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmAssetHistoryDto>>();
    }

    /// <summary>
    /// Export asset history to Excel (.xls).
    /// </summary>
    [HttpPost,NoJsonResult]
    public async Task<FileContentResult> ExportAsync([FromBody] AmAssetHistoryParam param)
    {
        const int maxRows = 10000;
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.AssetId != 0, x => x.AssetId == param.AssetId)
            .WhereIF(!string.IsNullOrEmpty(param.BizType), x => x.BizType == param.BizType)
            .WhereIF(param.BizId != 0, x => x.BizId == param.BizId)
            .WhereIF(!string.IsNullOrEmpty(param.Operation), x => x.Operation == param.Operation)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.BizType.Contains(param.Key) || x.Operation.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var list = await query
            .Includes(x => x.AssetObj)
            .OrderBy(x => x.OperateTime, OrderByType.Desc)
            .Take(maxRows)
            .ToListAsync();

        var headers = new[]
        {
            "操作时间","资产Id","资产编号","资产名称","业务类型","业务Id","操作","操作人Id","备注"
        };

        var rows = list.Select(x => (IReadOnlyList<object?>)new object?[]
        {
            x.OperateTime,
            x.AssetId,
            x.AssetObj?.AssetNo,
            x.AssetObj?.Name,
            x.BizType,
            x.BizId,
            x.Operation,
            x.OperatorId,
            x.Remark
        });

        var title = "资产留痕";
        var bytes = ExcelExportUtils.ToHtmlXls(title, headers, rows);
        var fileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xls";
        return new FileContentResult(bytes, "application/vnd.ms-excel")
        {
            FileDownloadName = fileName
        };
    }

    [HttpGet("{id}")]
    public async Task<AmAssetHistoryDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmAssetHistoryDto>();
    }

    /// <summary>
    /// 新增留痕（通常由业务流程调用）
    /// </summary>
    public async Task<bool> AddAsync(AmAssetHistoryDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmAssetHistory>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.OperateTime = model.OperateTime == default ? DateTime.Now : model.OperateTime;
        return await _thisRepository.InsertAsync(entity);
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        return await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
    }
}
