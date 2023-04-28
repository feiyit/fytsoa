using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 授权表
/// </summary>
[SugarTable("sys_permission")]
public class SysPermission
{
    /// <summary>
    /// 角色编号
    /// </summary>
    [Required]
    public long RoleId { get; set; }

    /// <summary>
    /// 菜单编号
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 接口权限
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<SysMenuApiUrl> Api { get; set; } = new();

    /// <summary>
    /// 授权类型1=角色-菜单
    /// </summary>
    [Required]
    public int Types { get; set; } = 1;


}