using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 用户角色关系表
/// </summary>
[SugarTable("sys_admin_role")]
public class SysAdminRole:EntityBase
{
    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long AdminId { get; set; }

    /// <summary>
    /// 角色编号
    /// </summary>
    [Required]
    public long RoleId { get; set; }

}