using SqlSugar;
using Mapster;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：任务候选人应用服务
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowTaskCandidateService : IApplicationService
{
    private readonly SugarRepository<WorkflowTaskCandidate> _thisRepository;

    public WorkflowTaskCandidateService(SugarRepository<WorkflowTaskCandidate> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    public async Task<List<WorkflowTaskCandidateDto>> GetByTaskIdAsync(long tenantId, long taskId)
    {
        var list = await _thisRepository.Context.Queryable<WorkflowTaskCandidate>()
            .Where(x => x.TenantId == tenantId && x.TaskId == taskId)
            .ToListAsync();

        return list.Adapt<List<WorkflowTaskCandidateDto>>();
    }

    public async Task<long> CreateAsync(WorkflowTaskCandidateDto input)
    {
        var entity = input.Adapt<WorkflowTaskCandidate>();
        entity.CreatedAt = entity.CreatedAt == default ? DateTime.Now : entity.CreatedAt;
        await _thisRepository.InsertAsync(entity);
        return entity.Id;
    }

    public async Task DeleteByTaskAsync(long tenantId, long taskId)
    {
        await _thisRepository.DeleteAsync(x => x.TenantId == tenantId && x.TaskId == taskId);
    }

}
