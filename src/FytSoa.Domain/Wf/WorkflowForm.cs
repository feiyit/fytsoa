using SqlSugar;

namespace FytSoa.Domain.Wf;

/// <summary>
/// 工作流：表单定义
/// 对应表：wf_form
/// </summary>
[SugarTable("wf_form")]
public class WorkflowForm : Entity
{
    /// <summary>
    /// 表单编码（唯一），例如：LEAVE_FORM
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 表单名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表单设计 JSON（包含画布结构、表单配置等）
    /// </summary>
    public string SchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// 对应的前端路由路径（可选），例如：/biz/leave/apply
    /// </summary>
    public string? RoutePath { get; set; }

    /// <summary>
    /// 状态：0=草稿,1=启用,2=停用
    /// </summary>
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建人 Id
    /// </summary>
    public long CreatedBy { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新人 Id
    /// </summary>
    public long UpdatedBy { get; set; } = 0;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

