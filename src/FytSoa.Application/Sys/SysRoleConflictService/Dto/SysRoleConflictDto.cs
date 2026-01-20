using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 角色互斥表
/// </summary>
public class SysRoleConflictDto : AppEntity
{
    /// <summary>
    /// 角色A
    /// </summary>
    [Required]
    public long RoleA { get; set; }
    
    /// <summary>
    /// 角色A信息
    /// </summary>
    public SysRoleNameDto RoleAObj { get; set; }

    /// <summary>
    /// 角色B
    /// </summary>
    [Required]
    public long RoleB { get; set; }
    
    /// <summary>
    /// 角色B信息
    /// </summary>
    public SysRoleNameDto RoleBObj { get; set; }

    /// <summary>
    /// 互斥说明
    /// </summary>
    [Required]
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}

public class SysRoleNameDto
{
    public string Name { get; set; }
}