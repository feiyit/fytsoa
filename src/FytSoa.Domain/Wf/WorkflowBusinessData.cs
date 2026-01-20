using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：通用业务数据
/// 对应表：wf_business_data
/// </summary>
[SugarTable("wf_business_data")]
public class WorkflowBusinessData : Entity
{
    /// <summary>
    /// 流程定义 Id
    /// </summary>
    public long DefinitionId { get; set; } = 0;

    /// <summary>
    /// 流程定义编码
    /// </summary>
    public string DefinitionKey { get; set; } = string.Empty;

    /// <summary>
    /// 业务主键，如单据 Id
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;

    /// <summary>
    /// 表单业务数据 JSON（SoaForm 的 v-model）
    /// </summary>
    public string FormDataJson { get; set; } = string.Empty;

    public long CreatedBy { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
}

