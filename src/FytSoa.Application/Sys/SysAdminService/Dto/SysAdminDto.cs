using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 管理员表
/// </summary>
public class SysAdminDto : AppEntity
{
    /// <summary>
    /// 所属角色
    /// </summary>
    public List<string> RoleGroup { get; set; } = new ();
    
    /// <summary>
    /// 所属角色信息
    /// </summary>
    public string RoleGroupName { get; set; }
    
    /// <summary>
    /// 所属岗位
    /// </summary>
    public List<string> PostGroup { get; set; } = new ();

    /// <summary>
    /// 所属部门
    /// </summary>
    public long OrganizeId { get; set; }
    
    /// <summary>
    /// 所属部门
    /// </summary>
    public SysOrganizeDto OrganizeObj { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    public string LoginAccount { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    public string LoginPassWord { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Mobile { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public string Sex { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool IsDel { get; set; } = false;
    
    /// <summary>
    /// 是否超级管理员
    /// </summary>
    public bool IsSuper { get; set; } = false;

    /// <summary>
    /// 备注
    /// </summary>
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

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime? LoginTime { get; set; }

    /// <summary>
    /// 上次登录时间
    /// </summary>
    public DateTime? UpLoginTime { get; set; }

    /// <summary>
    /// 登录次数
    /// </summary>
    public int LoginCount { get; set; } = 0;


}