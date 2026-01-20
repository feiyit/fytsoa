using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：流程实例历史
/// 对应表：wf_instance_history
/// </summary>
[SugarTable("wf_instance_history")]
public class WorkflowInstanceHistory : Entity
{
    /// <summary>
    /// 实例 Id
    /// </summary>
    public long InstanceId { get; set; } = 0;

    /// <summary>
    /// 事件类型，如 START, COMPLETE, TERMINATE, RECALL
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    public byte? FromStatus { get; set; }

    public byte? ToStatus { get; set; }

    public long OperatorId { get; set; } = 0;

    public string? OperatorName { get; set; }

    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; }
}
