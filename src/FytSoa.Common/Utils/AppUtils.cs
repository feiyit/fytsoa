using System.Security.Claims;
using FytSoa.Common.Jwt;
using FytSoa.Common.Jwt.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FytSoa.Common.Utils;


public static class AppUtils
{
    /// <summary>
    /// 容器注册服务
    /// </summary>
    public static IServiceCollection ServiceCollectio;
    
    /// <summary>
    /// 统一的服务提供程序
    /// </summary>
    public static IServiceProvider ServiceProvider { get; set; }
    
    /// <summary>
    /// 应用程序配置
    /// </summary>
    public static IConfiguration Configuration;
    
    /// <summary>
    /// 项目根目录
    /// </summary>
    public static string AppRoot=Environment.CurrentDirectory;


    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext HttpContext => GetService<IHttpContextAccessor>()?.HttpContext;
    
    /// <summary>
    /// 获取请求上下文用户
    /// </summary>
    /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
    public static ClaimsPrincipal User => HttpContext?.User;
    
    
    /// <summary>
    /// 获取Token信息
    /// </summary>
    public static JwtToken Token
    {
        get
        {
            var paramToken = HttpContext.Request.Headers["accessToken"].ToString();
            return string.IsNullOrEmpty(paramToken) ? new JwtToken() : JwtAuthService.SerializeJwt(paramToken);
        }
    }
    
    /// <summary>
    /// 获取登录人编号
    /// </summary>
    public static long LoginId
    {
        get
        {
            var userId = HttpContext?.User.FindFirst(nameof (JwtToken.Id))?.Value;
            return !string.IsNullOrEmpty(userId) ? long.Parse(userId) : 0;
        }
    }
    
    /// <summary>
    /// 获取租户编号
    /// </summary>
    public static string LoginUser
    {
        get
        {
            var user = HttpContext?.User.FindFirst(nameof (JwtToken.FullName))?.Value;
            return !string.IsNullOrEmpty(user) ? user : "";
        }
    }
    
    /// <summary>
    /// 获取租户编号
    /// </summary>
    public static long TenantId
    {
        get
        {
            var tenantId = HttpContext?.User.FindFirst(nameof (JwtToken.TenantId))?.Value;
            return !string.IsNullOrEmpty(tenantId) ? long.Parse(tenantId) : 0;
        }
    }
    

    public static void Init(IWebHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
        {
            Configuration = hostContext.Configuration;
            ServiceCollectio = services;
        });
    }
    
    public static void AppSetup(this IApplicationBuilder app)
    {
        ServiceProvider = app.ApplicationServices;
    }
    
    public static IConfigurationSection GetConfig(string path)
    { 
        return Configuration.GetSection(path);
    }
    
    /// <summary>
    /// 手动获取注入的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetService<T>() where T : class
    {
        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        return httpContextAccessor.HttpContext.RequestServices.GetService<T>();
    }

    
    private static string _mySqlConnectionString = string.Empty;
    /// <summary>
    /// MySql默认连接串
    /// </summary>
    public static string MySqlConnectionString
    {
        get
        {
            if (string.IsNullOrEmpty(_mySqlConnectionString))
            {
                _mySqlConnectionString = Configuration["SqlConnectionString:MySql"];
            }
            return _mySqlConnectionString;
        }
    }
        
    private static string _redisConnectionString = string.Empty;
    /// <summary>
    /// Redis默认连接串
    /// </summary>
    public static string RedisConnectionString
    {
        get
        {
            if (string.IsNullOrEmpty(_redisConnectionString))
            {
                _redisConnectionString = Configuration["Cache:Redis"];
            }
            return _redisConnectionString;
        }
    }
    
    
}
