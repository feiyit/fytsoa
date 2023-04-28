using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 投票项
/// </summary>
[SugarTable("sys_vote_item")]
public class SysVoteItem:Entity
{
    /// <summary>
    /// 投票编号
    /// </summary>
    [Required]
    public long VoteId { get; set; } = 0;

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
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }


}