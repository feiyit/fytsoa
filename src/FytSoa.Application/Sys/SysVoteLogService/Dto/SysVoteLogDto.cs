using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Sys;

/// <summary>
/// 投票日志
/// </summary>
public class SysVoteLogDto : AppEntity
{
    /// <summary>
    /// 投票编号
    /// </summary>
    [Required]
    public long VoteId { get; set; }

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
    [Required]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }


}