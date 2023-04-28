using FytSoa.Application;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.ApiService.Controllers;

public class CaptchaController : ApiController
{
    [HttpGet("{identify}"),AllowAnonymous,NoJsonResult,NoAuditLog]
    public async Task<IActionResult> Get(string identify)
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
}