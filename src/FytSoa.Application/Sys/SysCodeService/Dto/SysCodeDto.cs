using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 字典信息表
/// </summary>
public class SysCodeDto : AppEntity
{
    /// <summary>
    /// 分类编号
    /// </summary>
    [Required]
    public long TypeId { get; set; }
    
    /// <summary>
    /// 标记 1=默认  2=其他
    /// </summary>
    public int Tag { get; set; } = 1;

    /// <summary>
    /// 字典值名称
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    /// <summary>
    /// 字典值阈值
    /// </summary>
    public string CodeValues { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 备注
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
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