namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程实例历史 Dto
/// </summary>
public class WorkflowInstanceHistoryDto : AppEntity
{
    public long InstanceId { get; set; } = 0;

    public string EventType { get; set; } = string.Empty;

    public byte? FromStatus { get; set; }

    public byte? ToStatus { get; set; }

    public long OperatorId { get; set; } = 0;

    public string? OperatorName { get; set; }

    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; }=DateTime.Now;
}

