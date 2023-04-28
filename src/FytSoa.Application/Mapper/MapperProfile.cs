using System.Reflection;
using FytSoa.Common.Result;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace FytSoa.Application;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        /*config.ForType<User,UserDto>()
            .Map(dest => dest.UserAge, src => src.Age);*/
    }
    
    
}

/// <summary>
/// 对象映射拓展类
/// </summary>
public static class MapperConfigExtensions
{
    private static TypeAdapterConfig GetConfiguredMappingConfig()
    {
        var config = new TypeAdapterConfig();
        config.Default.PreserveReference(true);
        config.AllowImplicitSourceInheritance = true;
        config.AllowImplicitDestinationInheritance = true;
        
        /*config.NewConfig<WBaseEntity, WBaseEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ChangeDate)
            .Ignore(dest => dest.ChangeUserId);*/
        return config;
    }
    
    /// <summary>
    /// 添加对象映射
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns></returns>
    public static void AddMapperProfile(this IServiceCollection services)
    {
        // 获取全局映射配置
        var config = TypeAdapterConfig.GlobalSettings;

        var assemblyService = Assembly.Load("FytSoa.Application");
        // 扫描所有继承  IRegister 接口的对象映射配置
        config.Scan(assemblyService);

        // 配置默认全局映射（支持覆盖）
        config.Default
            .NameMatchingStrategy(NameMatchingStrategy.Flexible)
            .PreserveReference(true);

        // 配置支持依赖注入
        //services.AddSingleton(GetConfiguredMappingConfig());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
    }
}

