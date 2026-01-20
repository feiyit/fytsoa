namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程定义模型 Dto
/// 对应实体：WorkflowDefinitionModel
/// </summary>
public class WorkflowDefinitionModelDto : AppEntity
{
    /// <summary>
    /// 所属流程定义 Id
    /// </summary>
    public long DefinitionId { get; set; } = 0;

    /// <summary>
    /// 流程设计 JSON
    /// </summary>
    public string ModelJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否最新模型：true=最新
    /// </summary>
    public bool IsLatest { get; set; } = true;

    public long CreatedBy { get; set; } = 0;

    public DateTime CreatedAt { get; set; }=DateTime.Now;
}

