#nullable enable
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 组织闭包表，维护祖先/后代关系。
/// </summary>
[SugarTable("sys_org_unit_closure")]
public class SysOrgUnitClosure
{
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true)]
    public long Id { get; set; } = 0;
    
    [SugarColumn(ColumnName = "tenant_id", IsPrimaryKey = true)]
    public long TenantId { get; set; } = 0;

    [SugarColumn(ColumnName = "ancestor_id", IsPrimaryKey = true)]
    public long AncestorId { get; set; } = 0;

    [SugarColumn(ColumnName = "descendant_id", IsPrimaryKey = true)]
    public long DescendantId { get; set; } = 0;

    [SugarColumn(ColumnName = "depth")]
    public int Depth { get; set; }=1;
}
