using System.Text.Json;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.ApiService.Controllers;

/// <summary>
/// 在线用户
/// </summary>
public class OnLineController : ApiController
{
    [HttpGet]
    public List<ClientUser> Get()
    {
        var redisStr = RedisService.cli.Get(KeyUtils.ONLINEUSERS);
        if (string.IsNullOrEmpty(redisStr))
        {
            return new List<ClientUser>();
        }

        return JsonSerializer.Deserialize<List<ClientUser>>(redisStr) ?? new List<ClientUser>();
    }
}