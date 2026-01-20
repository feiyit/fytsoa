namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：任务 Dto
/// </summary>
public class WorkflowTaskDto : AppEntity
{
    public long InstanceId { get; set; } = 0;

    public string NodeId { get; set; } = string.Empty;

    public string NodeName { get; set; } = string.Empty;

    public long AssigneeId { get; set; } = 0;

    public string? AssigneeName { get; set; }

    public byte Status { get; set; } = 0;

    public string? Action { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }=DateTime.Now;

    public DateTime? CompletedAt { get; set; }
}

