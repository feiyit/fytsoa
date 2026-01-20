using FytSoa.Common.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Operator;

[ApiExplorerSettings(GroupName = "v1")]
public class SliderValidateService : IApplicationService
{
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(5);
    private const string CachePrefix = "slider-token:";

    public SliderValidateService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    [AllowAnonymous]
    public SliderVerifyResponse CreateToken([FromBody]SliderVerifyRequest request)
    {
        var token = Guid.NewGuid().ToString("N");
        var key = CachePrefix + token;

        var entry = new SliderTokenInfo
        {
            Account = request.Account ?? string.Empty,
            ExpireAt = DateTimeOffset.UtcNow.Add(_tokenLifetime)
        };

        _cacheService.Set(key, entry,entry.ExpireAt - DateTimeOffset.UtcNow);
        return new SliderVerifyResponse(){ SliderToken = token};
    }
    
    public bool ValidateToken(string token, string account)
    {
        var key = CachePrefix + token;
        if (!_cacheService.TryGetValue<SliderTokenInfo>(key, out var info))
        {
            return false;
        }
        Console.WriteLine($"缓存：{info.Account},过期时间：{info.ExpireAt.ToString()}");
        Console.WriteLine($"当前时间：{DateTimeOffset.UtcNow}");
        if (info.ExpireAt < DateTimeOffset.UtcNow)
        {
            _cacheService.Remove(key);
            return false;
        }

        if (!string.IsNullOrEmpty(info.Account) &&
            !string.Equals(info.Account, account, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        _cacheService.Remove(key);
        return true;
    }

    
    private sealed class SliderTokenInfo
    {
        public string Account { get; set; } = string.Empty;
        public DateTimeOffset ExpireAt { get; set; }
    }
}