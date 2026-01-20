using System.Diagnostics;
using FytSoa.Common.Cache;
using FytSoa.Common.Utils;
using FytSoa.Common.Jwt;
using FytSoa.Common.Jwt.Model;
using FytSoa.Common.Tenant;

namespace FytSoa.ApiService.Middleware;

public class JwtMiddleware
{
    private readonly List<string> _ignoreUrl = new()
    {
        "swagger",
        "/api/exammaterial/upload",
        "/fytapiui",
        "/chathub",
        "/api-config",
        "/fyt",
        "/upload/"
    };
    private readonly RequestDelegate _next;
    private readonly ICacheService _cacheService;

    public JwtMiddleware(RequestDelegate next,ICacheService cacheService)
    {
        _next = next;
        _cacheService=cacheService;
    }
    
    public Task Invoke(HttpContext context)
    {
        if (context.Request.Method == "OPTIONS")
        {
            return _next(context);
        }
        var headers = context.Request.Headers;
        #region 判断token是否在黑名单中
        var token = headers[JwtConstant.TokenName];
        if (!string.IsNullOrEmpty(token))
        {
            if (_cacheService.Exists("blacklist:" + AppUtils.TokenString))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return _next(context);
            }
        }

        #endregion
       
        //过滤，不要验证token的url
        var path = context.Request.Path.Value?.ToLower();
        var isIgnore = false;
        foreach (var item in _ignoreUrl.Where(item => path != null && path.Contains(item)))
        {
            isIgnore = true;
        }
        if (isIgnore)
        {
            return _next(context);
        }
        //自动刷新token
        
        if (string.IsNullOrEmpty(token))
        {
            return _next(context);
        }
        var jwtToken = JwtAuthService.SerializeJwt(token);
        var ts = jwtToken.Time.Subtract(DateTime.Now);
        Console.WriteLine("时间分钟："+ts.Minutes);
        if(ts.Minutes>40) return _next(context);
        //if (ts.Minutes is <= 30 or >= 60) return _next(context);
        var newToken = JwtAuthService.IssueJwt(new JwtToken()
        {
            Id = jwtToken.Id, FullName = jwtToken.FullName, Role = "Admin", RoleArray = jwtToken.RoleArray,
            TenantId = jwtToken.TenantId,
            EmployeeId = jwtToken.EmployeeId,
            Time = DateTime.Now.AddMinutes(60)
        });
        context.Response.Headers.Add("X-Refresh-Token", newToken);
        return _next(context);
    }
}