using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：流程实例
/// 对应表：wf_instance
/// </summary>
[SugarTable("wf_instance")]
public class WorkflowInstance : Entity
{
    /// <summary>
    /// 流程定义 Id
    /// </summary>
    public long DefinitionId { get; set; } = 0;

    /// <summary>
    /// 流程定义编码（冗余）
    /// </summary>
    public string DefinitionKey { get; set; } = string.Empty;

    /// <summary>
    /// 业务主键，如单据 Id
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;

    /// <summary>
    /// 流程标题（一般包含业务摘要）
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 发起人 Id
    /// </summary>
    public long StartUserId { get; set; } = 0;

    /// <summary>
    /// 发起人名称
    /// </summary>
    public string? StartUserName { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 状态：0=运行中,1=已完成,2=已终止,3=已撤回
    /// </summary>
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 当前活跃节点 Id 列表（JSON 或逗号分隔字符串）
    /// </summary>
    public string? CurrentNodeIds { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    #region 导航属性

    /// <summary>
    /// 该实例下的任务列表（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowTask.InstanceId))]
    public List<WorkflowTask> Tasks { get; set; } = new();

    /// <summary>
    /// 该实例的历史记录（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowInstanceHistory.InstanceId))]
    public List<WorkflowInstanceHistory> Histories { get; set; } = new();

    #endregion
}
