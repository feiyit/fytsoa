using System.Net;
using FytSoa.Application.Operator;
using FytSoa.Application.Sys;
using FytSoa.Common.Enum;
using FytSoa.Common.Extensions;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using FytSoa.Domain.Sys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FytSoa.ApiService.Filters;
/// <summary>
/// 全局异常处理
/// </summary>
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    readonly IWebHostEnvironment _hostEnvironment;
    private SysLogService _logService;
    public GlobalExceptionFilter(IWebHostEnvironment hostEnvironment
        ,SysLogService logService)
    {
        _hostEnvironment = hostEnvironment;
        _logService = logService;
    }
    
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.ExceptionHandled) return;
        
        #region 保存异常日志

        var type = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerTypeInfo.AsType();
        if (type != null)
        {
            var logInfo = new SysLogDto()
            {
                Level = LogEnum.Error,
                LogType = LogTypeEnum.Operate,
                Module = type.FullName,
                Method = context.HttpContext.Request.Method,
                OperateUser = AppUtils.LoginUser,
                IP = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Address = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString,
                Browser = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                Message=context.Exception.Message
            };
            //保存日志信息
            await _logService.AddAsync(logInfo);
        }
        #endregion
        
        var result = new ApiResult<string?>
        {
            Code = (int)HttpStatusCode.InternalServerError,
            Message = context.Exception.Message
        };
        
        if (context.Exception is BusinessException e)
        {
            result.Code = (int)HttpStatusCode.Found;
            result.Message = e.GetMessage();
        }
        context.Result = new JsonResult(result);
        context.ExceptionHandled = true;
    }
}