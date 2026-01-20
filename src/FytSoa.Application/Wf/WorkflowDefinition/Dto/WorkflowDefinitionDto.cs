namespace FytSoa.Application.Wf;

/// <summary>
/// 工作流：流程定义 Dto
/// 与 WorkflowDefinition 实体字段基本一致，
/// 通过 AppEntity 复用 Id / TenantId 等通用字段。
/// </summary>
public class WorkflowDefinitionDto : AppEntity
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

    public long CreatedBy { get; set; } = 0;

    public DateTime CreatedAt { get; set; }=DateTime.Now;

    public long UpdatedBy { get; set; } = 0;

    public DateTime? UpdatedAt { get; set; }
}
