using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 通知模块
/// </summary>
public class SysNoticeDto : AppEntity
{
    /// <summary>
    /// 发送人编号
    /// </summary>
    [Required]
    public long SendUserId { get; set; }
    
    /// <summary>
    /// 发送人信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(SendUserId))]
    public SysAdminDto SendUser { get; set; }

    /// <summary>
    /// 接收人编号(0=全员)
    /// </summary>
    [Required]
    public List<long> AcceptUserIds { get; set; } = new ();
    
    /// <summary>
    /// 接收人
    /// </summary>
    public List<SysAdminDto> AcceptUserList { get; set; } = new ();

    /// <summary>
    /// 通知标题
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 附件内容支持多个,保存相对位置，例如 /upload/files/1.txt
    /// </summary>
    public List<SysNotice.SysNoticeFiles> Files { get; set; } = new();

    /// <summary>
    /// 1=草稿2=存档3=删除
    /// </summary>
    public int Status { get; set; } = 0;
    
    /// <summary>
    /// 是否为发送邮件
    /// </summary>
    public bool IsSend { get; set; } = false;
    
    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
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