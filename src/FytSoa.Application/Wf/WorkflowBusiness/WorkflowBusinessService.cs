using System.Text.Json;
using FytSoa.Common.Utils;
using SqlSugar;
using FytSoa.Domain.Wf;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：业务数据服务（通用 wf_business_data）
/// </summary>
[ApiExplorerSettings(GroupName = "v10")]
public class WorkflowBusinessService : IApplicationService
{
    private readonly SugarRepository<WorkflowBusinessData> _thisRepository;

    public WorkflowBusinessService(SugarRepository<WorkflowBusinessData> thisRepository)
    {
        _thisRepository = thisRepository;
    }

    /// <summary>
    /// 保存业务数据输入参数
    /// </summary>
    public class SaveBusinessInput
    {
        public long TenantId { get; set; }
        public long DefinitionId { get; set; }
        public string DefinitionKey { get; set; } = string.Empty;
        public string? BusinessKey { get; set; }
        public object FormData { get; set; } = new { };
        public long CreatedBy { get; set; }
    }

    /// <summary>
    /// 保存业务数据（如果未传 businessKey 则自动生成）
    /// 返回最终 businessKey
    /// </summary>
    public async Task<string> SaveAsync(SaveBusinessInput input)
    {
        var bizKey = string.IsNullOrWhiteSpace(input.BusinessKey)
            ? $"BIZ_{Unique.Id()}"
            : input.BusinessKey!;

        var entity = new WorkflowBusinessData
        {
            TenantId = input.TenantId,
            DefinitionId = input.DefinitionId,
            DefinitionKey = input.DefinitionKey,
            BusinessKey = bizKey,
            FormDataJson = JsonSerializer.Serialize(input.FormData),
            CreatedBy = input.CreatedBy,
            CreatedAt = DateTime.Now,
        };

        await _thisRepository.InsertAsync(entity);
        return bizKey;
    }

    /// <summary>
    /// 根据  DefinitionKey + BusinessKey 获取业务数据
    /// </summary>
    public async Task<WorkflowBusinessData?> GetAsync(long tenantId, string definitionKey, string businessKey)
    {
        return await _thisRepository.Context.Queryable<WorkflowBusinessData>()
            .Where(x => x.DefinitionKey == definitionKey && x.BusinessKey == businessKey)
            .FirstAsync();
    }
}

