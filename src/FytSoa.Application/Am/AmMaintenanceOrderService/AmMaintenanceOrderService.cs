using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Am;

/// <summary>
/// 维修/保养工单服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmMaintenanceOrderService : IApplicationService
{
    private readonly SugarRepository<AmMaintenanceOrder> _thisRepository;
    private readonly SugarRepository<AmAssetHistory> _historyRepository;

    public AmMaintenanceOrderService(
        SugarRepository<AmMaintenanceOrder> thisRepository,
        SugarRepository<AmAssetHistory> historyRepository)
    {
        _thisRepository = thisRepository;
        _historyRepository = historyRepository;
    }

    /// <summary>
    /// 工单分页查询
    /// </summary>
    [HttpPost]
    public async Task<PageResult<AmMaintenanceOrderDto>> PagesAsync([FromBody] AmMaintenanceOrderParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(param.OrderType != 0, x => x.Type == (byte)param.OrderType)
            .WhereIF(param.OrderStatus != 0, x => x.Status == (byte)param.OrderStatus)
            .WhereIF(param.Priority != 0, x => x.Priority == (byte)param.Priority)
            .WhereIF(param.AssetId != 0, x => x.AssetId == param.AssetId)
            .WhereIF(param.VendorId != 0, x => x.VendorId == param.VendorId)
            .WhereIF(param.ReportUserId != 0, x => x.ReportUserId == param.ReportUserId)
            .WhereIF(param.AssignUserId != 0, x => x.AssignUserId == param.AssignUserId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.OrderNo.Contains(param.Key) ||
                     x.Title.Contains(param.Key));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        return page.Adapt<PageResult<AmMaintenanceOrderDto>>();
    }

    [HttpGet("{id}")]
    public async Task<AmMaintenanceOrderDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<AmMaintenanceOrderDto>();
    }

    public async Task<bool> AddAsync(AmMaintenanceOrderDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var entity = model.Adapt<AmMaintenanceOrder>();
        if (entity.Id == 0) entity.Id = Unique.Id();
        entity.TenantId = tenantId;
        entity.CreateTime = DateTime.Now;
        entity.UpdateTime = null;

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.InsertAsync(entity);

            if (entity.AssetId != 0)
            {
                await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                    tenantId,
                    entity.AssetId,
                    "MAINTENANCE",
                    entity.Id,
                    "CREATE",
                    null,
                    entity,
                    remark: $"新增工单：{entity.OrderNo}/{entity.Title}"
                ));
            }
        });

        return tran.IsSuccess;
    }

    public async Task<bool> ModifyAsync(AmMaintenanceOrderDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var before = await _thisRepository.AsQueryable()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == model.Id);

        var entity = model.Adapt<AmMaintenanceOrder>();
        entity.TenantId = tenantId;
        entity.UpdateTime = DateTime.Now;

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(entity);

            // assetId 允许修改（例如最初未绑定资产），记录时以变更后的资产为主
            var assetId = entity.AssetId != 0 ? entity.AssetId : before.AssetId;
            if (assetId != 0)
            {
                await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                    tenantId,
                    assetId,
                    "MAINTENANCE",
                    entity.Id,
                    "UPDATE",
                    before,
                    entity,
                    remark: $"修改工单：{entity.OrderNo}/{entity.Title}"
                ));
            }
        });

        return tran.IsSuccess;
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        var beforeList = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToListAsync();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            if (beforeList.Count > 0)
            {
                var histories = beforeList
                    .Where(x => x.AssetId != 0)
                    .Select(o => AmAssetHistoryUtils.Build(
                        tenantId,
                        o.AssetId,
                        "MAINTENANCE",
                        o.Id,
                        "DELETE",
                        o,
                        null,
                        remark: $"删除工单：{o.OrderNo}/{o.Title}"
                    )).ToList();

                if (histories.Count > 0)
                {
                    await _historyRepository.InsertRangeAsync(histories);
                }
            }

            await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
        });

        return tran.IsSuccess;
    }
}
