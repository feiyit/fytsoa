using FytSoa.Application.Exam;
using FytSoa.Application.User;
using FytSoa.Common.Param;
using FytSoa.Domain.Exam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FytSoa.Exam.Pages.Common;

[ValidateAntiForgeryToken]
public class IndexModel:PageModel
{
    private readonly ExamCommentService _commentService;
    private readonly ExamCourseService _courseService;
    private readonly MemberService _userService;
    public IndexModel(ExamCommentService commentService
    ,ExamCourseService courseService
    ,MemberService userService)
    {
        _commentService = commentService;
        _courseService = courseService;
        _userService = userService;
    }

    public void OnGet()
    {
    }
    
    /// <summary>
    /// 加载评论列表
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostInit([FromBody]PageParam param)
    {
        var res = await _commentService.GetPagesAsync(param);
        return new JsonResult(res);
    }
    
    /// <summary>
    /// 添加评论
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAddComment([FromBody]CommentParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new{Content ="参数验证失败~",StatusCode = 500}) ;
        }
        await _commentService.AddAsync(new ExamCommentDto()
        {
            CategoryId = param.CategoryId,
            UserId = param.UserId,
            Content = param.Text
        });
        return new JsonResult(new{StatusCode = 200});
    }
    
    /// <summary>
    /// 添加评论-回复
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAddCommentReply([FromBody]CommentReplyParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new{Content ="参数验证失败~",StatusCode = 500}) ;
        }

        var model = await _commentService.GetAsync(param.CommentId);
        var user = await _userService.GetAsync(param.UserId);
        var byUser = await _userService.GetAsync(param.ByUserId);
        model.ReplyBody.Add(new ExamComment.ExamCommentReply()
        {
            UserId = param.UserId,
            NickName = user.NickName,
            Avatar = user.Avatar,
            Content = param.Text,
            ByUserId = param.ByUserId,
            ByNickName = byUser.NickName
        });
        await _commentService.ModifyAsync(model);
        return new JsonResult(new{StatusCode = 200});
    }
    
    /// <summary>
    /// 支持
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostSupport([FromBody]SupportParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new{Content ="参数验证失败~",StatusCode = 500}) ;
        }
        await _courseService.ModifySupportAsync(param.Id,param.Type);
        return new JsonResult(new{StatusCode = 200});
    }
    
    public class SupportParam
    {
        public long Id { get; set; }
        
        public int Type { get; set; }
    }

    public class CommentParam
    {
        /// <summary>
        /// 当前登录人编号
        /// </summary>
        public long UserId { get; set; }
        
        /// <summary>
        /// 课程编号
        /// </summary>
        public long CategoryId { get; set; }
        
        /// <summary>
        /// 回复内容
        /// </summary>
        public string Text { get; set; }
    }
    
    public class CommentReplyParam:CommentParam
    {
        /// <summary>
        /// 回复评论编号
        /// </summary>
        public long CommentId { get; set; }
        
        /// <summary>
        /// 被回复人
        /// </summary>
        public long ByUserId { get; set; }
    }
}