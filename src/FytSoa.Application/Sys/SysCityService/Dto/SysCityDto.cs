using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 城市表
/// </summary>
public class SysCityDto : AppEntity
{
    /// <summary>
    /// 城市名称
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Name { get; set; }

    /// <summary>
    /// 所属上级
    /// </summary>
    [Required]
    public long ParentId { get; set; }

    /// <summary>
    /// 所属上级组
    /// </summary>
    public List<string> ParentIdList { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [Required]
    public int Layer { get; set; } = 1;

    /// <summary>
    /// 城市编号
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public string Longitude { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    public string Dimension { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 添加时间
    /// </summary>
    [Required]
    public DateTime AddTime { get; set; }=DateTime.Now;


}