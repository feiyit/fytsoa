using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 通知已读模块
/// </summary>
[SugarTable("sys_notice_read")]
public class SysNoticeRead:Entity
{
    /// <summary>
    /// 通知编号
    /// </summary>
    [Required]
    public long NoticeId { get; set; } = 0;

    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long UserId { get; set; } = 0;

    /// <summary>
    /// 是否已读
    /// </summary>
    [Required]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }


}