using FytSoa.Application;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.ApiService.Controllers;

public class CaptchaController : ApiController
{
    [HttpGet("{identify}"),AllowAnonymous,NoJsonResult,NoAuditLog]
    public async Task<IActionResult> GetIdentify(string identify)
    {
        if (string.IsNullOrEmpty(identify))
        {
            identify = CommonUtils.GetIp();
        }
        var code = await CaptchaUtils.GenerateRandomCaptchaAsync(4);
        MemoryService.Default.SetCache(KeyUtils.CAPTCHACODE + identify, code,5);
        var captcha = await CaptchaUtils.GenerateCaptchaImageAsync(code);
        return File(captcha.ms.ToArray(), "image/gif");
    }

    [HttpGet("task"),AllowAnonymous]
    public IActionResult GetTask()
    {
        Console.WriteLine($"执行自动任务-HTTP：{DateTime.Now}");
        return new JsonResult("Success");
    }

    
}