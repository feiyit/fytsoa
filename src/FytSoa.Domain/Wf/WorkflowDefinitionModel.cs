using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：流程定义设计模型（画布 JSON）
/// 对应表：wf_definition_model
/// </summary>
[SugarTable("wf_definition_model")]
public class WorkflowDefinitionModel : Entity
{
    /// <summary>
    /// 对应的流程定义 Id
    /// </summary>
    public long DefinitionId { get; set; } = 0;

    /// <summary>
    /// 流程设计 JSON（节点、连线、表单权限等）
    /// </summary>
    public string ModelJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否最新模型：1=是,0=否
    /// </summary>
    public bool IsLatest { get; set; } = true;

    public long CreatedBy { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
}
