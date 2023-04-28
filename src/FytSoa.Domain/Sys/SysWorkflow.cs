using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 工作流
/// </summary>
[SugarTable("sys_workflow")]
public class SysWorkflow:EntityBase
{
    /// <summary>
    /// 流程类型
    /// </summary>
    [Required]
    public long TypeId { get; set; } = 0;
    
    /// <summary>
    /// 类型信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(TypeId))]
    public SysCode TypeCode { get; set; } 

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
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 流程说明
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 具体流程Json
    /// </summary>
    [Required]
    [StringLength(0)]
    public string Flow { get; set; }

}