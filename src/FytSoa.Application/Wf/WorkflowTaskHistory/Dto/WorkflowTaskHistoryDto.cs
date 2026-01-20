namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：任务历史 Dto
/// </summary>
public class WorkflowTaskHistoryDto : AppEntity
{
    public long InstanceId { get; set; } = 0;

    public long TaskId { get; set; } = 0;

    public string NodeId { get; set; } = string.Empty;

    public string NodeName { get; set; } = string.Empty;

    public long AssigneeId { get; set; } = 0;

    public string? AssigneeName { get; set; }

    public string? Action { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }=DateTime.Now;

    public DateTime? CompletedAt { get; set; }
}

