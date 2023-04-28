using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Sys;

/// <summary>
/// 登录参数
/// </summary>
public class LoginParam
{
    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    public string Account { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [Required]
    public string Password { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [Required]
    public string Code { get; set; }

    /// <summary>
    /// 验证码Key
    /// </summary>
    [Required]
    public string CodeKey { get; set; }
}