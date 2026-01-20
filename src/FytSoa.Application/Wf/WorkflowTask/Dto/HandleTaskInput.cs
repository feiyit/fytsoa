namespace FytSoa.Application.Wf;

/// <summary>
/// 任务处理输入参数（同意 / 驳回 / 转办等）
/// </summary>
public class HandleTaskInput
{
    public long TenantId { get; set; }
    public long TaskId { get; set; }
    /// <summary>
    /// 动作：Agree / Reject / Transfer 等
    /// </summary>
    public string Action { get; set; } = "Agree";
    /// <summary>
    /// 审批意见
    /// </summary>
    public string? Comment { get; set; }
    public long OperatorId { get; set; }
    public string? OperatorName { get; set; }
    /// <summary>
    /// 驳回目标节点 Id（RejectMode=Specific 时有效）
    /// </summary>
    public string? RejectToNodeId { get; set; }
    /// <summary>
    /// 驳回模式：Start=发起人，Previous=上一节点，Specific=指定节点
    /// </summary>
    public string? RejectMode { get; set; }
    /// <summary>
    /// 审批时如需修改表单，可将最新表单值传入 ExtraData.formData
    /// </summary>
    public object? ExtraData { get; set; }
}