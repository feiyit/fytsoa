using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程实例历史应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowInstanceHistoryService : IApplicationService
{
    /// <summary>
    /// 仓储对象，用于对流程实例历史表进行增删改查
    /// </summary>
    private readonly SugarRepository<WorkflowInstanceHistory> _thisRepository;

    /// <summary>
    /// 构造函数，注入当前实体对应的仓储
    /// </summary>
    /// <param name="thisRepository">流程实例历史仓储</param>
    public WorkflowInstanceHistoryService(SugarRepository<WorkflowInstanceHistory> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 根据租户和流程实例 Id，获取该实例的所有历史记录（按时间正序）
    /// </summary>
    /// <param name="tenantId">租户 Id</param>
    /// <param name="instanceId">流程实例 Id</param>
    /// <returns>流程实例历史记录列表</returns>
    public async Task<List<WorkflowInstanceHistoryDto>> GetByInstanceIdAsync(long tenantId, long instanceId)
    {
        var list = await _thisRepository.Context.Queryable<WorkflowInstanceHistory>()
            .Where(x => x.TenantId == tenantId && x.InstanceId == instanceId)
            .OrderBy(x => x.CreatedAt, OrderByType.Asc)
            .ToListAsync();

        // 使用 Mapster 处理实体到 Dto 的映射
        return list.Adapt<List<WorkflowInstanceHistoryDto>>();
    }

    /// <summary>
    /// 新增一条流程实例历史记录
    /// </summary>
    /// <param name="input">流程实例历史 Dto</param>
    /// <returns>新记录的 Id</returns>
    public async Task<long> CreateAsync(WorkflowInstanceHistoryDto input)
    {
        // 使用 Mapster 处理 Dto 到实体的映射
        var entity = input.Adapt<WorkflowInstanceHistory>();
        entity.CreatedAt = entity.CreatedAt == default ? DateTime.Now : entity.CreatedAt;
        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }
}
