using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 投票日志
/// </summary>
[SugarTable("sys_vote_log")]
public class SysVoteLog:Entity
{
    /// <summary>
    /// 投票编号
    /// </summary>
    [Required]
    public long VoteId { get; set; } = 0;

    /// <summary>
    /// 投票项编号
    /// </summary>
    [Required]
    public long VoteItemId { get; set; }

    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }


}