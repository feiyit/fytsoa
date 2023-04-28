using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FytSoa.Application.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FytSoa.Exam.Pages.User;
[ValidateAntiForgeryToken]
public class LoginModel:PageModel
{
    public string ReturnUrl { get; private set; }
    
    private readonly MemberService _memberService;
    public LoginModel(MemberService memberService)
    {
        _memberService = memberService;
    }
    
    public async Task OnGetAsync(string returnUrl = null!)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ReturnUrl = returnUrl;
    }
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPost([FromBody] LoginParam param)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { Content = "参数验证失败~", StatusCode = 500 });
        }

        var user = await _memberService.GetSiteLoginAsync(new MemberLoginParam()
        {
            Email = param.Email,
            PassWord = param.Password
        });
        if (user.Id==0)
        {
            return new JsonResult(new{StatusCode = 500,Content="账号或密码不正确"});
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.LoginName), 
            new("UserId", user.Id.ToString()),
            new("UserName", user.NickName),
            new("Avatar", user.Avatar)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
        };
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties
        );
        return new JsonResult(new{StatusCode = 200});
    }
    
    public class LoginParam
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }
    }
}