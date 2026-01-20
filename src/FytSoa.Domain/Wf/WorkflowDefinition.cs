using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：流程定义
/// 对应表：wf_definition
/// </summary>
[SugarTable("wf_definition")]
public class WorkflowDefinition : Entity
{
    /// <summary>
    /// 流程编码，如 LEAVE_APPLY
    /// </summary>
    public string DefKey { get; set; } = string.Empty;

    /// <summary>
    /// 流程名称
    /// </summary>
    public string DefName { get; set; } = string.Empty;

    /// <summary>
    /// 版本号，从 1 递增
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// 状态：0=草稿,1=已发布,2=停用
    /// </summary>
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 所属模块 / 分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 绑定表单 / 页面标识
    /// </summary>
    public string? FormSchemaId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建人 Id
    /// </summary>
    public long CreatedBy { get; set; } = 0;

    /// <summary>
    /// 创建时间（数据库默认 CURRENT_TIMESTAMP）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新人 Id
    /// </summary>
    public long UpdatedBy { get; set; } = 0;

    /// <summary>
    /// 最后更新时间（数据库 ON UPDATE CURRENT_TIMESTAMP）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    #region 导航属性

    /// <summary>
    /// 该流程定义下的设计模型列表（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowDefinitionModel.DefinitionId))]
    public List<WorkflowDefinitionModel> Models { get; set; } = new();

    /// <summary>
    /// 该流程定义下的流程实例列表（1 对多）
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(WorkflowInstance.DefinitionId))]
    public List<WorkflowInstance> Instances { get; set; } = new();

    #endregion
}
