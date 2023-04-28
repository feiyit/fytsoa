using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using FytSoa.Application.User;
using FytSoa.Domain.Exam;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 评论
/// </summary>
public class ExamCommentDto : AppEntity
{
    /// <summary>
    /// 分类编号
    /// </summary>
    [Required]
    public long CategoryId { get; set; }
    
    public ExamCourseDto Course { get; set; }

    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long UserId { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    public MemberDto User { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    [Required]
    public bool Audit { get; set; } = false;

    /// <summary>
    /// 评论内容
    /// </summary>
    [Required]
    [StringLength(900)]
    public string Content { get; set; }

    /// <summary>
    /// 评星
    /// </summary>
    [Required]
    public int Star { get; set; } = 0;

    /// <summary>
    /// 回复内容
    /// </summary>
    public List<ExamComment.ExamCommentReply> ReplyBody { get; set; } = new();

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