namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：任务候选人 Dto
/// </summary>
public class WorkflowTaskCandidateDto : AppEntity
{
    public long TaskId { get; set; } = 0;

    public long UserId { get; set; } = 0;

    public string? UserName { get; set; }

    public DateTime CreatedAt { get; set; }=DateTime.Now;
}

