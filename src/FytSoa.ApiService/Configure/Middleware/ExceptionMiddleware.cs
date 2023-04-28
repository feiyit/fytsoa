using System.Text.Json;
using FytSoa.Common.Result;
using FytSoa.Common.Utils;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace FytSoa.ApiService.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostingEnvironment _hostingEnvironment;
    public ExceptionMiddleware(RequestDelegate next
    ,IHostingEnvironment hostingEnvironment)
    {
        _next = next;
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ExceptionHandlerAsync(context, ex);
        }
    }

    private async Task ExceptionHandlerAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        var message = "服务端发生异常，请稍后重试~";
        if (_hostingEnvironment.IsDevelopment())
        {
            message = ex.Message;
            Logger.Error("服务端异常："+ex.Message);
        }
        var result = JsonSerializer.Serialize(new ApiResult<string>()
        {
            Code = 500,
            Message = message
        });

        await context.Response.WriteAsync(result.ToLower());
    }
}