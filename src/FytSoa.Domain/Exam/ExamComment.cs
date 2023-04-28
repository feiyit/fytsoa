using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.User;
using SqlSugar;

namespace FytSoa.Domain.Exam;

/// <summary>
/// 评论
/// </summary>
[SugarTable("exam_comment")]
public class ExamComment:Entity
{
    /// <summary>
    /// 分类编号
    /// </summary>
    [Required]
    public long CategoryId { get; set; }
    
    /// <summary>
    /// 课程
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public ExamCourse Course { get; set; }

    /// <summary>
    /// 用户编号
    /// </summary>
    [Required]
    public long UserId { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public Member User { get; set; }
    
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
    [SugarColumn(IsJson = true)]
    public List<ExamCommentReply> ReplyBody { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
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

    /// <summary>
    /// 回复人信息
    /// </summary>
    public class ExamCommentReply
    {
        /// <summary>
        /// 回复人编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 回复人昵称
        /// </summary>
        public string NickName { get; set; }
        
        /// <summary>
        /// 回复人头像
        /// </summary>
        public string Avatar { get; set; }
        
        /// <summary>
        /// 回复人内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime ReplyTime { get; set; }=DateTime.Now;

        /// <summary>
        /// 被回复人编号
        /// </summary>
        public long ByUserId { get; set; } = 0;

        /// <summary>
        /// 被回复人昵称
        /// </summary>
        public string ByNickName { get; set; }
    }
}