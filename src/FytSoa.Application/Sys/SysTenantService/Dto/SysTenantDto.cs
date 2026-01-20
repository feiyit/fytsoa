using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 租户管理
/// </summary>
public class SysTenantDto : AppEntity
{

    /// <summary>
    /// 租户名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// 负责人
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Person { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// 默认账号
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PassWord { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

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