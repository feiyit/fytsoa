using FytSoa.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace FytSoa.Sugar;

public static class SugarStrapper
{
    /// <summary>
    /// 注册单例上下午服务
    /// </summary>
    /// <param name="services"></param>
    public static void SqlSugarSetup(this IServiceCollection services)
    {
        services.AddSingleton<ISqlSugarClient>(provider =>
        {
            var sugarScope = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = DbType.MySql,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
                ConnectionString = AppUtils.MySqlConnectionString,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    EntityService = (c, p) =>
                    {
                        if (c.PropertyType.IsGenericType &&
                            c.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            p.IsNullable = true;
                        }
                    }
                }
            });
            return sugarScope;
        });
        //注册仓储
        services.AddScoped(typeof(SugarRepository<>));
    }
}