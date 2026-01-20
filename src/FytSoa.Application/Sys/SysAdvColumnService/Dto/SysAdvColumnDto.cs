using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 广告位栏目表 
/// </summary>
public class SysAdvColumnDto : AppEntity
{
    /// <summary>
    /// 父编号
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 父编号集合
    /// </summary>
    public List<string> ParentIdList { get; set; } = new();

    /// <summary>
    /// 层级
    /// </summary>
    public int Layer { get; set; } = 1;

    /// <summary>
    /// 栏目名称
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Name { get; set; }

    /// <summary>
    /// 栏目类型
    /// </summary>
    public string Flag { get; set; }

    /// <summary>
    /// 栏目宽度
    /// </summary>
    public int Width { get; set; } = 0;

    /// <summary>
    /// 栏目高度
    /// </summary>
    public int Height { get; set; } = 0;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 栏目状态
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 栏目说明
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 站点ID
    /// </summary>
    public long SiteId { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 添加人
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