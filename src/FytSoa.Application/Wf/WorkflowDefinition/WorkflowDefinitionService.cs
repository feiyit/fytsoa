using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程定义应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowDefinitionService : IApplicationService
{
    private readonly SugarRepository<WorkflowDefinition> _thisRepository;

    public WorkflowDefinitionService(SugarRepository<WorkflowDefinition> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 根据 Id 获取流程定义
    /// </summary>
    public async Task<WorkflowDefinitionDto?> GetAsync(long id)
    {
        var entity = await _thisRepository.Context
            .Queryable<WorkflowDefinition>()
            .FirstAsync(x => x.Id == id);

        return entity == null ? null : entity.Adapt<WorkflowDefinitionDto>();
    }

    /// <summary>
    /// 简单分页查询（按关键字 / 状态）
    /// </summary>
    public async Task<List<WorkflowDefinitionDto>> GetListAsync(
        long tenantId,
        string? keyword = null,
        byte? status = null)
    {
        var query = _thisRepository.Context.Queryable<WorkflowDefinition>()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.DefKey.Contains(keyword!) || x.DefName.Contains(keyword!));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var list = await query
            .OrderBy(x => x.DefKey).OrderByDescending(x => x.Version)
            .ToListAsync();

        return list.Adapt<List<WorkflowDefinitionDto>>();
    }

    /// <summary>
    /// 创建流程定义（默认版本 1、草稿）
    /// </summary>
    public async Task<long> CreateAsync(WorkflowDefinitionDto input)
    {
        var entity = input.Adapt<WorkflowDefinition>();

        entity.Version = entity.Version <= 0 ? 1 : entity.Version;
        entity.Status = 0; // 草稿

        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新流程定义（仅基础信息，不含模型）
    /// </summary>
    public async Task UpdateAsync(WorkflowDefinitionDto input)
    {
        var entity = input.Adapt<WorkflowDefinition>();
        await _thisRepository.UpdateAsync(entity);
    }

    /// <summary>
    /// 删除流程定义（简单物理删除，可根据需要改为逻辑删除）
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        await _thisRepository.DeleteAsync(x => x.Id == id);
    }
    
}
