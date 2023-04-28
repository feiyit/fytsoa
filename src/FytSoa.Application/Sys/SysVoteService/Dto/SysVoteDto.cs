using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Sys;

/// <summary>
/// 投票表
/// </summary>
public class SysVoteDto : AppEntity
{
    /// <summary>
    /// 投票标题
    /// </summary>
    [Required]
    [StringLength(90)]
    public string Title { get; set; }

    /// <summary>
    /// 投票类型（图文、视频、分组）
    /// </summary>
    [Required]
    public int Type { get; set; } = 1;

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 勾选规则（单选、多选）
    /// </summary>
    [Required]
    public int TickRule { get; set; } = 1;

    /// <summary>
    /// 防刷规则（IP限制）
    /// </summary>
    [Required]
    public bool SwipeRule { get; set; } = true;

    /// <summary>
    /// 文件地址
    /// </summary>
    public string FileUrl { get; set; }

    /// <summary>
    /// 规则
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }= DateTime.Now;

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

    public List<SysVoteItemDto> Items { get; set; }
}