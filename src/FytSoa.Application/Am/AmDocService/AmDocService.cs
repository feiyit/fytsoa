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
/// 资产业务单据服务
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class AmDocService : IApplicationService
{
    private readonly SugarRepository<AmDoc> _thisRepository;
    private readonly SugarRepository<AmAssetHistory> _historyRepository;

    public AmDocService(
        SugarRepository<AmDoc> thisRepository,
        SugarRepository<AmAssetHistory> historyRepository)
    {
        _thisRepository = thisRepository;
        _historyRepository = historyRepository;
    }

    [HttpPost]
    public async Task<PageResult<AmDocDto>> PagesAsync([FromBody] AmDocParam param)
    {
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!param.IncludeDeleted, x => !x.IsDel)
            .WhereIF(!string.IsNullOrEmpty(param.DocType), x => x.DocType == param.DocType)
            .WhereIF(!string.IsNullOrEmpty(param.SubType), x => x.SubType == param.SubType)
            .WhereIF(param.DocStatus != 0, x => x.Status == (byte)param.DocStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.DocNo.Contains(param.Key) ||
                     x.DocType.Contains(param.Key) ||
                     (x.SubType != null && x.SubType.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var page = await query.OrderBy(x => x.Id, OrderByType.Desc).ToPageAsync(param.Page, param.Limit);
        // 列表页不返回 Items，避免额外开销
        return page.Adapt<PageResult<AmDocDto>>();
    }

    /// <summary>
    /// Export docs to Excel (.xls).
    /// </summary>
    [HttpPost,NoJsonResult]
    public async Task<FileContentResult> ExportAsync([FromBody] AmDocParam param)
    {
        const int maxRows = 10000;
        var tenantId = param.TenantId != 0 ? param.TenantId : AppUtils.TenantId;
        var query = _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId)
            .WhereIF(!param.IncludeDeleted, x => !x.IsDel)
            .WhereIF(!string.IsNullOrEmpty(param.DocType), x => x.DocType == param.DocType)
            .WhereIF(!string.IsNullOrEmpty(param.SubType), x => x.SubType == param.SubType)
            .WhereIF(param.DocStatus != 0, x => x.Status == (byte)param.DocStatus)
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                x => x.DocNo.Contains(param.Key) ||
                     x.DocType.Contains(param.Key) ||
                     (x.SubType != null && x.SubType.Contains(param.Key)));

        if (!string.IsNullOrEmpty(param.Query))
        {
            var cond = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query.Where(cond);
        }

        var list = await query
            .OrderBy(x => x.Id, OrderByType.Desc)
            .Take(maxRows)
            .ToListAsync();

        var headers = new[]
        {
            "单据Id","单据号","类型","子类型","状态","金额","业务时间","到期时间","创建时间","备注"
        };

        var rows = list.Select(x => (IReadOnlyList<object?>)new object?[]
        {
            x.Id,
            x.DocNo,
            x.DocType,
            x.SubType,
            x.Status,
            x.TotalAmount,
            x.BizTime,
            x.DueTime,
            x.CreateTime,
            x.Remark
        });

        var title = "单据管理";
        var bytes = ExcelExportUtils.ToHtmlXls(title, headers, rows);
        var fileName = $"{title}_{DateTime.Now:yyyyMMddHHmmss}.xls";
        return new FileContentResult(bytes, "application/vnd.ms-excel")
        {
            FileDownloadName = fileName
        };
    }

    [HttpGet("{id}")]
    public async Task<AmDocDto> GetAsync(long id)
    {
        var doc = await _thisRepository.GetByIdAsync(id);
        var dto = doc.Adapt<AmDocDto>();

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDocItem>>();
        var items = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == doc.TenantId && x.DocId == id)
            .OrderBy(x => x.LineNo, OrderByType.Asc)
            .ToListAsync();
        dto.Items = items.Adapt<List<AmDocItemDto>>();
        return dto;
    }

    public async Task<bool> AddAsync(AmDocDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        var docEntity = model.Adapt<AmDoc>();
        if (docEntity.Id == 0) docEntity.Id = Unique.Id();
        docEntity.TenantId = tenantId;
        docEntity.CreateTime = DateTime.Now;
        docEntity.UpdateTime = null;

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDocItem>>();
        var items = (model.Items ?? new List<AmDocItemDto>()).Select((x, idx) =>
        {
            x.TenantId = tenantId;
            x.DocId = docEntity.Id;
            x.LineNo = x.LineNo <= 0 ? idx + 1 : x.LineNo;
            var e = x.Adapt<AmDocItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.DocId = docEntity.Id;
            return e;
        }).ToList();

        docEntity.TotalAmount = items.Sum(x => x.Amount);

        var histories = items
            .Where(x => x.AssetId != 0)
            .Select(x => AmAssetHistoryUtils.Build(
                tenantId,
                x.AssetId,
                docEntity.DocType,
                docEntity.Id,
                "CREATE",
                null,
                new
                {
                    Doc = new
                    {
                        docEntity.Id,
                        docEntity.DocNo,
                        docEntity.DocType,
                        docEntity.SubType,
                        docEntity.Status,
                        docEntity.BizTime,
                        docEntity.DueTime
                    },
                    Item = new
                    {
                        x.Id,
                        x.LineNo,
                        x.AssetId,
                        x.AssetNo,
                        x.Name,
                        x.Qty,
                        x.WarehouseId,
                        x.BinId,
                        x.LocationId,
                        x.Remark
                    }
                },
                remark: $"新增单据：{docEntity.DocNo}（{docEntity.DocType}）"
            )).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.InsertAsync(docEntity);
            if (items.Count > 0)
            {
                await itemRepo.InsertRangeAsync(items);
            }
            if (histories.Count > 0)
            {
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }

    public async Task<bool> ModifyAsync(AmDocDto model)
    {
        var tenantId = model.TenantId != 0 ? model.TenantId : AppUtils.TenantId;
        model.TenantId = tenantId;

        // 变更前快照（用于写入留痕）
        var beforeDoc = await _thisRepository.GetByIdAsync(model.Id);
        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDocItem>>();
        var beforeItems = await itemRepo.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.DocId == model.Id)
            .OrderBy(x => x.LineNo, OrderByType.Asc)
            .ToListAsync();

        var docEntity = model.Adapt<AmDoc>();
        docEntity.TenantId = tenantId;
        docEntity.UpdateTime = DateTime.Now;

        var items = (model.Items ?? new List<AmDocItemDto>()).Select((x, idx) =>
        {
            x.TenantId = tenantId;
            x.DocId = docEntity.Id;
            x.LineNo = x.LineNo <= 0 ? idx + 1 : x.LineNo;
            var e = x.Adapt<AmDocItem>();
            if (e.Id == 0) e.Id = Unique.Id();
            e.TenantId = tenantId;
            e.DocId = docEntity.Id;
            return e;
        }).ToList();

        docEntity.TotalAmount = items.Sum(x => x.Amount);

        var assetIds = beforeItems.Select(x => x.AssetId)
            .Concat(items.Select(x => x.AssetId))
            .Where(x => x != 0)
            .Distinct()
            .ToList();

        var histories = assetIds.Select(assetId =>
        {
            var beforeForAsset = beforeItems.Where(x => x.AssetId == assetId).Select(x => new
            {
                x.Id,
                x.LineNo,
                x.AssetId,
                x.AssetNo,
                x.Name,
                x.Qty,
                x.WarehouseId,
                x.BinId,
                x.LocationId,
                x.Remark
            }).ToList();

            var afterForAsset = items.Where(x => x.AssetId == assetId).Select(x => new
            {
                x.Id,
                x.LineNo,
                x.AssetId,
                x.AssetNo,
                x.Name,
                x.Qty,
                x.WarehouseId,
                x.BinId,
                x.LocationId,
                x.Remark
            }).ToList();

            return AmAssetHistoryUtils.Build(
                tenantId,
                assetId,
                docEntity.DocType,
                docEntity.Id,
                "UPDATE",
                new
                {
                    Doc = beforeDoc == null ? null : new
                    {
                        beforeDoc.Id,
                        beforeDoc.DocNo,
                        beforeDoc.DocType,
                        beforeDoc.SubType,
                        beforeDoc.Status,
                        beforeDoc.BizTime,
                        beforeDoc.DueTime
                    },
                    Items = beforeForAsset
                },
                new
                {
                    Doc = new
                    {
                        docEntity.Id,
                        docEntity.DocNo,
                        docEntity.DocType,
                        docEntity.SubType,
                        docEntity.Status,
                        docEntity.BizTime,
                        docEntity.DueTime
                    },
                    Items = afterForAsset
                },
                remark: $"修改单据：{docEntity.DocNo}（{docEntity.DocType}）"
            );
        }).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(docEntity);
            await itemRepo.DeleteAsync(x => x.TenantId == tenantId && x.DocId == docEntity.Id);
            if (items.Count > 0)
            {
                await itemRepo.InsertRangeAsync(items);
            }
            if (histories.Count > 0)
            {
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }

    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        var tenantId = AppUtils.TenantId;
        var docs = await _thisRepository.AsQueryable()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id))
            .ToListAsync();

        var itemRepo = _thisRepository.ChangeRepository<SugarRepository<AmDocItem>>();
        var docIds = docs.Select(x => x.Id).ToList();
        var items = docIds.Count == 0
            ? new List<AmDocItem>()
            : await itemRepo.AsQueryable()
                .Where(x => x.TenantId == tenantId && docIds.Contains(x.DocId))
                .ToListAsync();

        var histories = items
            .Where(x => x.AssetId != 0)
            .Select(x =>
            {
                var doc = docs.FirstOrDefault(d => d.Id == x.DocId);
                var bizType = doc?.DocType ?? "DOC";
                var docNo = doc?.DocNo ?? x.DocId.ToString();
                return AmAssetHistoryUtils.Build(
                    tenantId,
                    x.AssetId,
                    bizType,
                    x.DocId,
                    "DELETE",
                    new
                    {
                        Doc = doc == null ? null : new
                        {
                            doc.Id,
                            doc.DocNo,
                            doc.DocType,
                            doc.SubType,
                            doc.Status,
                            doc.BizTime,
                            doc.DueTime
                        },
                        Item = new
                        {
                            x.Id,
                            x.LineNo,
                            x.AssetId,
                            x.AssetNo,
                            x.Name,
                            x.Qty,
                            x.WarehouseId,
                            x.BinId,
                            x.LocationId,
                            x.Remark
                        }
                    },
                    null,
                    remark: $"删除单据（逻辑删除）：{docNo}（{bizType}）"
                );
            }).ToList();

        var tran = await _thisRepository.Context.Ado.UseTranAsync(async () =>
        {
            await _thisRepository.UpdateAsync(
                x => new AmDoc { IsDel = true, UpdateTime = DateTime.Now },
                x => x.TenantId == tenantId && ids.Contains(x.Id));

            if (histories.Count > 0)
            {
                await _historyRepository.InsertRangeAsync(histories);
            }
        });

        return tran.IsSuccess;
    }
}
