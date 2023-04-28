using System.ComponentModel.DataAnnotations;
using FytSoa.Application.User;
using FytSoa.Common.Utils;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FytSoa.Exam.Pages.User;
[ValidateAntiForgeryToken]
public class RegisterModel:PageModel
{
    private readonly MemberService _memberService;
    public RegisterModel(MemberService memberService)
    {
        _memberService = memberService;
    }
    
    public async Task OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPost([FromBody] RegisterParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { Content = "参数验证失败~", StatusCode = 500 });
        }

        var res=await _memberService.AddAsync(new MemberDto()
        {
            GroupId=1471788590005620738,
            LoginName= StringUtils.RandomString(),
            LoginPwd=param.Password.AESEncrypt(),
            NickName=param.NickName,
            Email = param.Email,
            Sex = "男",
            Avatar="/assets/images/avatars/avatar-2.jpg",
        });
        return new JsonResult(new{StatusCode = res?200:500,message=res?"":"账号或邮箱已存在，请更改~"});
    }
    
    public class RegisterParam
    {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string NickName { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }
    }
}