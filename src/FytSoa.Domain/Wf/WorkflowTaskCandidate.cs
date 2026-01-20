using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：任务候选人
/// 对应表：wf_task_candidate
/// </summary>
[SugarTable("wf_task_candidate")]
public class WorkflowTaskCandidate : Entity
{
  /// <summary>
  /// 任务 Id
  /// </summary>
  public long TaskId { get; set; } = 0;

  /// <summary>
  /// 候选人 Id
  /// </summary>
  public long UserId { get; set; } = 0;

  /// <summary>
  /// 候选人名称
  /// </summary>
  public string? UserName { get; set; }

  public DateTime CreatedAt { get; set; }
}
