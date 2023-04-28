using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 工作流
/// </summary>
public class SysWorkflowDto : AppEntity
{
    /// <summary>
    /// 流程类型
    /// </summary>
    [Required]
    public long TypeId { get; set; }
    
    /// <summary>
    /// 类型信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(TypeId))]
    public SysCodeDto TypeCode { get; set; } 

    /// <summary>
    /// 流程名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Icon { get; set; }

    /// <summary>
    /// 审批被拒后状态
    /// </summary>
    [Required]
    public int Refused { get; set; } = 1;

    /// <summary>
    /// 状态
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 流程说明
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 具体流程Json
    /// </summary>
    [Required]
    public string Flow { get; set; }

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