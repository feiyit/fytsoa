namespace FytSoa.Application.Am;

/// <summary>
/// 单据状态变更参数
/// </summary>
public class AmDocStatusParam
{
    /// <summary>
    /// 单据ID集合
    /// </summary>
    public List<long> Ids { get; set; } = new();

    /// <summary>
    /// 目标状态：0=草稿,1=待审批,2=已通过,3=已驳回,4=执行中,5=已完成,6=已取消
    /// </summary>
    public int Status { get; set; }
}
