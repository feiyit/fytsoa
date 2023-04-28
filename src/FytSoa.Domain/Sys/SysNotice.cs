using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 通知模块
/// </summary>
[SugarTable("sys_notice")]
public class SysNotice:EntityBase
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
    public SysAdmin SendUser { get; set; }

    /// <summary>
    /// 接收人编号(0=全员)
    /// </summary>
    [Required]
    [SugarColumn(IsJson = true)]
    public List<long> AcceptUserIds { get; set; } = new ();

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
    /// 附件内容支持多个
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<SysNoticeFiles> Files { get; set; } = new();

    /// <summary>
    /// 1=草稿2=存档3=删除
    /// </summary>
    [Required]
    public int Status { get; set; } = 0;
    
    /// <summary>
    /// 是否为发送邮件
    /// </summary>
    public bool IsSend { get; set; } = false;

    /// <summary>
    /// 通知文件
    /// </summary>
    public class SysNoticeFiles
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 文件地址
        /// </summary>
        public string Url { get; set; }
    }
}