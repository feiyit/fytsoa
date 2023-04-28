using System.Security.Cryptography;
using FytSoa.Application;
using FytSoa.Common.Utils;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.ApiService.Controllers;

public class QuartzController : ApiController
{
    public QuartzController()
    {
    }

    /// <summary>
    /// 执行任务Http
    /// </summary>
    /// <returns></returns>
    [HttpGet("job"),AllowAnonymous]
    public IActionResult TestJob()
    {
        Logger.Info("执行任务："+DateTime.Now);
        return Ok("Success");
    }

    [HttpGet]
    public IActionResult Jiami()
    {
        var key = AppUtils.GetConfig(Security.Name).Get<Security>();
        var jiaStr = "123456".AESEncrypt(key.AesKey,CipherMode.ECB);
        Console.WriteLine("加密字符串："+jiaStr);
        Console.WriteLine("解密原文："+jiaStr.AESDecrypt(key.AesKey,CipherMode.ECB));
        return Ok("Success");
    }
    
    [HttpGet("long")]
    public IActionResult GetLong()
    {
        var s = "123456".DesEncrypt();
        Console.WriteLine("密码这是："+ s);
        return Ok(new List<long>()
        {
            1513338417361063936,
            1513338429503574016,
            1513338441746747392
        });
    }
    [HttpGet("pwd"),AllowAnonymous,NoAuditLog]
    public IActionResult TestPassword()
    {
        var res = "21232f297a57a5a743894a0e4a801fc3".AESEncrypt();
        return Ok(res);
    }
}