using FytSoa.ApiService.Middleware;
using FytSoa.Common.Utils;

namespace FytSoa.ApiService;

public static class UseMiddleware
{
    public static void UseSetup(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.AppSetup();
        // 跨域设置
        app.UseCors("FytSoaCors");
        
        // Rate limiting (needs routing; keep after CORS so rejected responses still include CORS headers)
        app.UseRateLimiter();

        // DI
        //AppUtils.ServiceProvider = app.ApplicationServices;
        
        app.Use(next => async context =>
        {
            context.Request.EnableBuffering();
            await next(context);
        });
        // 认证
        app.UseAuthentication();
        // 授权
        app.UseAuthorization();
        // 中间件异常处理
        app.UseMiddleware<ExceptionMiddleware>();
        // Jwt中间件处理
        app.UseMiddleware<JwtMiddleware>();
        
    }
}
