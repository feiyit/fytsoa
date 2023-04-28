using System.Diagnostics;
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

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public Task Invoke(HttpContext context)
    {
        if (context.Request.Method == "OPTIONS")
        {
            return _next(context);
        }
        var headers = context.Request.Headers;
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
        var token = headers[JwtConstant.TokenName];
        if (string.IsNullOrEmpty(token))
        {
            return _next(context);
        }
        var jwtToken = JwtAuthService.SerializeJwt(token);
        var ts = DateTime.Now.Subtract(jwtToken.Time);
        if (ts.Minutes is <= 30 or >= 60) return _next(context);
        var newToken = JwtAuthService.IssueJwt(new JwtToken()
        {
            Id = jwtToken.Id, FullName = jwtToken.FullName, Role = "Admin", RoleArray = jwtToken.RoleArray,
            TenantId = jwtToken.TenantId,
            Time = DateTime.Now
        });
        context.Response.Headers.Add("X-Refresh-Token", newToken);
        return _next(context);
    }
}