using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 角色互斥表
/// </summary>
[SugarTable("sys_role_conflict")]
public class SysRoleConflict:EntityBase
{
    /// <summary>
    /// 角色A
    /// </summary>
    [Required]
    public long RoleA { get; set; }
    
    /// <summary>
    /// 角色A信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RoleA))]
    public SysRole RoleAObj { get; set; }
    
    /// <summary>
    /// 角色B
    /// </summary>
    [Required]
    public long RoleB { get; set; }
    
    /// <summary>
    /// 角色B信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RoleB))]
    public SysRole RoleBObj { get; set; }
    
    /// <summary>
    /// 互斥说明
    /// </summary>
    [Required]
    public string Summary { get; set; }
}