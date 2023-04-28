using System.Reflection;
using FytSoa.Application;
using FytSoa.Common.Cache;
using FytSoa.Common.Utils;
using FytSoa.Common.Jwt;
using FytSoa.DynamicApi;
using FytSoa.Generator;
using FytSoa.Quartz.Extensions;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Savorboard.CAP.InMemoryMessageQueue;

namespace FytSoa.CrossCutting;

public static  class BootStrapperIoC
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // application services
        var assemblyService = Assembly.Load("FytSoa.Application");
        var serviceType = assemblyService.GetTypes().Where(u => u.IsClass && !u.IsAbstract && !u.IsGenericType && u.Name.EndsWith("Service")).ToList();
        foreach (var item in serviceType.Where(s => !s.IsInterface))
        {
            services.AddScoped(item);
        }

        services.AddMemoryCache();
        
        services.AddScoped<ICacheService, MemoryService>();
            
        // code generator
        services.AddScoped<IGeneratorService, GeneratorService>();
        
        
        services.SqlSugarSetup();

        // log
        Logger.AddLogger();

        // id
        Unique.GetInstance();
        
        // Quartz
        services.AddQuartz();
        services.AddQuartzClassJobs();
        
        // Cap
        services.AddCap(x =>
        {
            x.UseInMemoryStorage();
            x.UseInMemoryMessageQueue();
        });
        services.AddTransient<CapSubscriberService>();
            
        // dynamic webapi
        services.AddDynamicWebApi();
        
        // Jwt Config
        services.AddJwtConfiguration ();
    }
}