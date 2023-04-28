using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.User;

/// <summary>
/// 会员组
/// </summary>
public class MemberGroupDto
{
    /// <summary>
    /// 唯一编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 会员等级;关联数据字典
    /// </summary>
    [Required]
    public long LevelId { get; set; }

    /// <summary>
    /// 组名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// 组图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 升级积分
    /// </summary>
    [Required]
    public int UpPoint { get; set; } = 0;

    /// <summary>
    /// 升级金额
    /// </summary>
    [Required]
    public decimal UpMoney { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = false;

    /// <summary>
    /// 描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
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