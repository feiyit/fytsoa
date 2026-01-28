using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Am;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Text.Json;

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

    private const byte AssetStatusMaintenance = 4; // am_asset.Status = 4 => 维修中
    private const string BizTypeMaintenance = "MAINTENANCE";
    private const string OpAssetStatusLock = "ASSET_STATUS_LOCK";
    private const string OpAssetStatusRestore = "ASSET_STATUS_RESTORE";

    private static bool IsOrderInProgress(byte status) => status is 1 or 2 or 3; // 待受理/已指派/处理中

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

        var assetRepo = _thisRepository.ChangeRepository<SugarRepository<AmAsset>>();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.InsertAsync(entity);

            if (entity.AssetId != 0)
            {
                await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
                    tenantId,
                    entity.AssetId,
                    BizTypeMaintenance,
                    entity.Id,
                    "CREATE",
                    null,
                    entity,
                    remark: $"新增工单：{entity.OrderNo}/{entity.Title}"
                ));

                // 工单创建即进入处理中链路时：锁定资产状态为“维修中”
                if (IsOrderInProgress(entity.Status))
                {
                    await LockAssetToMaintenanceAsync(tenantId, entity, assetRepo);
                }
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

        var assetRepo = _thisRepository.ChangeRepository<SugarRepository<AmAsset>>();

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
                    BizTypeMaintenance,
                    entity.Id,
                    "UPDATE",
                    before,
                    entity,
                    remark: $"修改工单：{entity.OrderNo}/{entity.Title}"
                ));
            }

            await SyncAssetStatusByOrderChangeAsync(tenantId, before, entity, assetRepo);
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

        var assetRepo = _thisRepository.ChangeRepository<SugarRepository<AmAsset>>();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            if (beforeList.Count > 0)
            {
                var histories = beforeList
                    .Where(x => x.AssetId != 0)
                    .Select(o => AmAssetHistoryUtils.Build(
                        tenantId,
                        o.AssetId,
                        BizTypeMaintenance,
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

                // 删除工单时如果资产仍被该工单锁定为维修中，尝试恢复
                foreach (var o in beforeList.Where(x => x.AssetId != 0))
                {
                    await RestoreAssetStatusIfLockedAsync(tenantId, o, assetRepo, reason: "删除工单");
                }
            }

            await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && ids.Contains(x.Id));
        });

        return tran.IsSuccess;
    }

    private async Task SyncAssetStatusByOrderChangeAsync(
        long tenantId,
        AmMaintenanceOrder before,
        AmMaintenanceOrder after,
        SugarRepository<AmAsset> assetRepo)
    {
        var beforeAssetId = before.AssetId;
        var afterAssetId = after.AssetId;

        var beforeInProgress = IsOrderInProgress(before.Status);
        var afterInProgress = IsOrderInProgress(after.Status);

        // 资产发生变更：先处理旧资产恢复，再处理新资产锁定
        if (beforeAssetId != 0 && beforeAssetId != afterAssetId)
        {
            await RestoreAssetStatusIfLockedAsync(tenantId, before, assetRepo, reason: "工单资产变更");
        }

        // 状态从非处理中 -> 处理中：锁定资产状态
        if (!beforeInProgress && afterInProgress && afterAssetId != 0)
        {
            await LockAssetToMaintenanceAsync(tenantId, after, assetRepo);
            return;
        }

        // 只要从“处理中链路”离开（包括完成/关闭/取消/回到草稿等），就尝试恢复资产状态（如果曾被锁定）
        if (beforeInProgress && !afterInProgress && afterAssetId != 0)
        {
            await RestoreAssetStatusIfLockedAsync(tenantId, after, assetRepo, reason: "工单结束");
        }
    }

    private async Task LockAssetToMaintenanceAsync(
        long tenantId,
        AmMaintenanceOrder order,
        SugarRepository<AmAsset> assetRepo)
    {
        if (order.AssetId == 0) return;

        // 避免重复锁定：同一工单同一资产如果已经记录过锁定快照，就不再重复写入
        var hasLockHistory = await _historyRepository.AsQueryable()
            .Where(h =>
                h.TenantId == tenantId &&
                h.AssetId == order.AssetId &&
                h.BizType == BizTypeMaintenance &&
                h.BizId == order.Id &&
                h.Operation == OpAssetStatusLock)
            .AnyAsync();
        if (hasLockHistory) return;

        var asset = await assetRepo.AsQueryable()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == order.AssetId);

        var beforeStatus = asset.Status;
        if (beforeStatus == AssetStatusMaintenance) return;

        await assetRepo.UpdateAsync(
            x => new AmAsset { Status = AssetStatusMaintenance, UpdateTime = DateTime.Now },
            x => x.TenantId == tenantId && x.Id == order.AssetId);

        await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
            tenantId,
            order.AssetId,
            BizTypeMaintenance,
            order.Id,
            OpAssetStatusLock,
            new { asset.Id, asset.AssetNo, Status = beforeStatus },
            new { asset.Id, asset.AssetNo, Status = AssetStatusMaintenance },
            remark: $"工单锁定资产状态为维修中：{order.OrderNo}/{order.Title}"
        ));
    }

    private async Task RestoreAssetStatusIfLockedAsync(
        long tenantId,
        AmMaintenanceOrder order,
        SugarRepository<AmAsset> assetRepo,
        string reason)
    {
        if (order.AssetId == 0) return;

        // 查找该工单对该资产的“锁定快照”
        var lockHistory = await _historyRepository.AsQueryable()
            .Where(h =>
                h.TenantId == tenantId &&
                h.AssetId == order.AssetId &&
                h.BizType == BizTypeMaintenance &&
                h.BizId == order.Id &&
                h.Operation == OpAssetStatusLock)
            .OrderBy(h => h.OperateTime, OrderByType.Asc)
            .FirstAsync();

        var restoreTo = TryReadStatusFromJson(lockHistory?.BeforeJson);
        if (restoreTo == null) return;

        var asset = await assetRepo.AsQueryable()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == order.AssetId);

        // 只在当前仍为“维修中”时恢复，避免覆盖其它业务已经改变的状态
        if (asset.Status != AssetStatusMaintenance) return;

        await assetRepo.UpdateAsync(
            x => new AmAsset { Status = restoreTo.Value, UpdateTime = DateTime.Now },
            x => x.TenantId == tenantId && x.Id == order.AssetId);

        await _historyRepository.InsertAsync(AmAssetHistoryUtils.Build(
            tenantId,
            order.AssetId,
            BizTypeMaintenance,
            order.Id,
            OpAssetStatusRestore,
            new { asset.Id, asset.AssetNo, Status = AssetStatusMaintenance },
            new { asset.Id, asset.AssetNo, Status = restoreTo.Value },
            remark: $"{reason}，恢复资产状态：{order.OrderNo}/{order.Title}"
        ));
    }

    private static byte? TryReadStatusFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

            if (TryGetPropertyCaseInsensitive(doc.RootElement, "status", out var statusEl))
            {
                // number 或 string 都兼容
                if (statusEl.ValueKind == JsonValueKind.Number && statusEl.TryGetInt32(out var i))
                    return (byte)i;
                if (statusEl.ValueKind == JsonValueKind.String && int.TryParse(statusEl.GetString(), out var si))
                    return (byte)si;
            }
        }
        catch
        {
            // ignore parse error
        }

        return null;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement obj, string name, out JsonElement value)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
