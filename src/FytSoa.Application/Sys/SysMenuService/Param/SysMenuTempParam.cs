namespace FytSoa.Application.Sys;

public class SysMenuTempParam
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 父级
    /// </summary>
    public long ParentId { get; set; } = 0;
    
    /// <summary>
    /// 租户
    /// </summary>
    public long TenantId { get; set; } = 0;
}