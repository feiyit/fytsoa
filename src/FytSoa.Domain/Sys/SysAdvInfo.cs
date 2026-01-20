using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 广告位信息表 
/// </summary>
[SugarTable("sys_adv_info")]
public class SysAdvInfo:Entity
{
    /// <summary>
    /// 栏目Id
    /// </summary>
    [Required]
    public long ColumnId { get; set; }

    /// <summary>
    /// 广告名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Title { get; set; }

    /// <summary>
    /// 广告位类型
    /// </summary>
    [Required]
    public int Types { get; set; } = 1;

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 广告位图片
    /// </summary>
    public string ImgUrl { get; set; }

    /// <summary>
    /// 连接地址
    /// </summary>
    public string LinkUrl { get; set; }

    /// <summary>
    /// 跳转方式
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Target { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 广告位代码
    /// </summary>
    public string Codes { get; set; }

    /// <summary>
    /// 是否开启时间限制
    /// </summary>
    [Required]
    public bool IsTimeLimit { get; set; } = false;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? BeginTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 广告点击率
    /// </summary>
    [Required]
    public int Hits { get; set; } = 0;

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