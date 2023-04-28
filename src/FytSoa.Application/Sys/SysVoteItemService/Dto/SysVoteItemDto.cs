using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Sys;

/// <summary>
/// 投票项
/// </summary>
public class SysVoteItemDto : AppEntity
{
    /// <summary>
    /// 投票编号
    /// </summary>
    [Required]
    public long VoteId { get; set; }

    /// <summary>
    /// 投票项标题
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Title { get; set; }

    /// <summary>
    /// 投票数量
    /// </summary>
    [Required]
    public int Count { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }


}