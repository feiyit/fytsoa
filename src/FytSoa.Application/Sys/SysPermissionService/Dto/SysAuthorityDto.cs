using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 授权表
/// </summary>
public class SysPermissionDto
{
    /// <summary>
    /// 租户编号
    /// </summary>
    public long Tenant { get; set; } = 0;
    
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