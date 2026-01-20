using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：任务处理历史
/// 对应表：wf_task_history
/// </summary>
[SugarTable("wf_task_history")]
public class WorkflowTaskHistory : Entity
{
    /// <summary>
    /// 实例 Id
    /// </summary>
    public long InstanceId { get; set; } = 0;

    /// <summary>
    /// 当前记录关联的任务 Id
    /// </summary>
    public long TaskId { get; set; } = 0;

    /// <summary>
    /// 节点 Id
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    public long AssigneeId { get; set; } = 0;

    public string? AssigneeName { get; set; }

    public int LevelIndex { get; set; } = 0;

    /// <summary>
    /// 动作：agree/reject/transfer 等
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 任务创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 任务完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
