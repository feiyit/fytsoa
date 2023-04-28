using FytSoa.Domain.Sys;

namespace FytSoa.Application.Sys;

/// <summary>
/// 权限参数
/// </summary>
public class SysAuthorityParam
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 授权菜单列表
    /// </summary>
    public List<SysAuthorityMenu> Menus { get; set; }
}

public class SysAuthorityAdminByRoleParam
{
    /// <summary>
    /// 角色编号，可多个
    /// </summary>
    public List<string> RoleArr { get; set; }

    /// <summary>
    /// 用户编号
    /// </summary>
    public List<string> AdminArr { get; set; }
}

/// <summary>
/// 授权菜单列表
/// </summary>
public class SysAuthorityMenu
{
    /// <summary>
    /// 菜单Id
    /// </summary>
    public long MenuId { get; set; }
    
    /// <summary>
    /// 授权的Api
    /// </summary>
    public List<SysMenuApiUrl> Api { get; set; } = new();

}