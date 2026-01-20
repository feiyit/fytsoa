using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程定义模型应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowDefinitionModelService : IApplicationService
{
    private readonly SugarRepository<WorkflowDefinitionModel> _thisRepository;

    public WorkflowDefinitionModelService(SugarRepository<WorkflowDefinitionModel> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 获取指定定义的最新模型
    /// </summary>
    public async Task<WorkflowDefinitionModelDto?> GetLatestByDefinitionIdAsync(long definitionId, long tenantId)
    {
        var entity = await _thisRepository.Context.Queryable<WorkflowDefinitionModel>()
            .Where(x => x.DefinitionId == definitionId)
            .OrderBy(x => x.IsLatest, OrderByType.Desc)
            .OrderBy(x => x.CreatedAt, OrderByType.Desc)
            .FirstAsync();

        return entity == null ? null : entity.Adapt<WorkflowDefinitionModelDto>();
    }

    /// <summary>
    /// 保存模型并标记最新
    /// </summary>
    public async Task<long> SaveAsync(WorkflowDefinitionModelDto input)
    {
        var entity = input.Adapt<WorkflowDefinitionModel>();

        if (input.Id == 0)
        {
            // 将同一 DefinitionId 下旧记录的 IsLatest 置为 false
            await _thisRepository.Context.Updateable<WorkflowDefinitionModel>()
                .SetColumns(x => new WorkflowDefinitionModel { IsLatest = false })
                .Where(x => x.DefinitionId == entity.DefinitionId && x.TenantId == entity.TenantId)
                .ExecuteCommandAsync();

            entity.IsLatest = true;
            await _thisRepository.InsertAsync(entity);
        }
        else
        {
            await _thisRepository.UpdateAsync(entity);
        }
        return entity.Id;
    }

}
