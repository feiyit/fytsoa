using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：待办/已办任务
/// 对应表：wf_task
/// </summary>
[SugarTable("wf_task")]
public class WorkflowTask : Entity
{
    /// <summary>
    /// 流程实例 Id
    /// </summary>
    public long InstanceId { get; set; } = 0;

    /// <summary>
    /// 节点 Id（设计器中的节点 key）
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 当前处理人 Id（单人）
    /// </summary>
    public long AssigneeId { get; set; } = 0;

    /// <summary>
    /// 当前处理人名称
    /// </summary>
    public string? AssigneeName { get; set; }

    /// <summary>
    /// 任务状态：0=待处理,1=已处理,2=已转办,3=已撤回
    /// </summary>
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 最终动作：agree/reject/transfer 等
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 连续主管审批索引
    /// </summary>
    public int LevelIndex { get; set; } = 0;

    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    #region 导航属性

    /// <summary>
    /// 任务候选人列表（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowTaskCandidate.TaskId))]
    public List<WorkflowTaskCandidate> Candidates { get; set; } = new();

    /// <summary>
    /// 任务处理历史记录（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowTaskHistory.TaskId))]
    public List<WorkflowTaskHistory> Histories { get; set; } = new();

    #endregion
}
