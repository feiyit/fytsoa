using System.Text.Json;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using FytSoa.Common.Result;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;
[ApiExplorerSettings(GroupName = "v1")]
public class SysSafetyService : IApplicationService
{
    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    public SafetySetting Get()
    {
        var model = new SafetySetting();
        var redisStr = RedisService.cli.Get(KeyUtils.SYSTEMSAFETY);
        if (!string.IsNullOrEmpty(redisStr))
        {
            model = JsonSerializer.Deserialize<SafetySetting>(redisStr);
        }
        return model;
    }
        
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public void Add(SafetySetting model)
    {
        RedisService.cli.Set(KeyUtils.SYSTEMSAFETY, JsonSerializer.Serialize(model));
    }
}