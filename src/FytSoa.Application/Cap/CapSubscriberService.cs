using DotNetCore.CAP;
using FytSoa.Application.Sys;
using FytSoa.Common.Utils;
using FytSoa.Domain.Sys;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace FytSoa.Application;

/// <summary>
/// Cap 消费
/// </summary>
public class CapSubscriberService:ICapSubscribe
{
    private readonly IServiceScopeFactory _scopeFactory;
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="scopeFactory"></param>
    public CapSubscriberService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    /// <summary>
    /// cap 保存日志
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [CapSubscribe("log.cap")]
    public async Task AddLogAsync(SysLogDto model)
    {
        model.Id = Unique.Id();
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<ISqlSugarClient>();
        if (context != null) await context.Insertable(model.Adapt<SysLog>()).ExecuteCommandAsync();
    }
}