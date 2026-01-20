using FytSoa.Common.Param;
using FytSoa.Common.Result;
using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：任务历史应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowTaskHistoryService : IApplicationService
{
    private readonly SugarRepository<WorkflowTaskHistory> _thisRepository;

    public WorkflowTaskHistoryService(SugarRepository<WorkflowTaskHistory> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    public async Task<List<WorkflowTaskHistoryDto>> GetByInstanceIdAsync(long tenantId, long instanceId)
    {
        var list = await _thisRepository.Context.Queryable<WorkflowTaskHistory>()
            .Where(x => x.TenantId == tenantId && x.InstanceId == instanceId)
            .OrderBy(x => x.CreatedAt, OrderByType.Asc)
            .ToListAsync();

        // 使用 Mapster 处理实体到 Dto 的映射
        return list.Adapt<List<WorkflowTaskHistoryDto>>();
    }

    /// <summary>
    /// 分页查询当前用户经办过的任务历史（“我的审批记录”）
    /// </summary>
    public async Task<PageResult<WorkflowTaskHistoryDto>> GetMyHandledPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Where(x => x.AssigneeId == param.userId)
            .OrderBy(x => x.CreatedAt, OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);

        return query.Adapt<PageResult<WorkflowTaskHistoryDto>>();
    }

    public async Task<long> CreateAsync(WorkflowTaskHistoryDto input)
    {
        // 使用 Mapster 处理 Dto 到实体的映射
        var entity = input.Adapt<WorkflowTaskHistory>();
        entity.CreatedAt = entity.CreatedAt == default ? DateTime.Now : entity.CreatedAt;
        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }
}