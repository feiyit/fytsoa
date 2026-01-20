namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程实例 Dto
/// </summary>
public class WorkflowInstanceDto : AppEntity
{
    public long DefinitionId { get; set; } = 0;

    public string DefinitionKey { get; set; } = string.Empty;

    public string BusinessKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public long StartUserId { get; set; } = 0;

    public string? StartUserName { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public byte Status { get; set; } = 0;

    public string? CurrentNodeIds { get; set; }

    public DateTime CreatedAt { get; set; }=DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}

