using System;
using System.Threading.Tasks;
using FreeRedis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
namespace FytSoa.Common.Cache;
public class CacheService : ICacheService
{
    private readonly bool _useRedis;
    private readonly RedisClient _redisCache; // FreeRedis 缓存实例
    private readonly IMemoryCache _memoryCache; // 内存缓存实例

    /// <summary>
    /// 构造函数：根据配置决定使用 Redis 还是内存缓存
    /// </summary>
    /// <param name="configuration">配置文件（用于读取 Redis 连接字符串）</param>
    /// <param name="memoryCache">内存缓存实例（通过 DI 注入）</param>
    public CacheService(IConfiguration configuration, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _useRedis = configuration.GetValue<bool>("Cache:isRedis");
        // 读取配置：是否启用 Redis 及连接字符串
        var redisConnectionString = configuration.GetValue<string>("Cache:Redis");
        // 初始化 Redis 缓存（如果启用）
        if (!_useRedis) return;
        _redisCache = new RedisClient(redisConnectionString);
            
        // 可选：配置 Redis 序列化方式（默认支持 JSON）
        _redisCache.Serialize = obj => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
        _redisCache.Deserialize = (bytes, type) => 
            System.Text.Json.JsonSerializer.Deserialize(bytes, type);
    }

    #region 同步方法
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (_useRedis)
        {
            // Redis 存储：expiration 为 null 时默认不过期
            _redisCache.Set(key, value, expiration ?? TimeSpan.FromSeconds(-1));
        }
        else
        {
            // 内存缓存：expiration 为 null 时默认不过期（但内存缓存可能因内存压力被回收）
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            
            _memoryCache.Set(key, value, options);
        }
    }

    public T Get<T>(string key)
    {
        return _useRedis 
            ? _redisCache.Get<T>(key) 
            : _memoryCache.Get<T>(key);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        if (_useRedis)
        {
            value = _redisCache.Get<T>(key);
            return value != null; // 注意：Redis 可能存储 null 值，需根据实际场景调整
        }
        else
        {
            return _memoryCache.TryGetValue(key, out value);
        }
    }

    public void Remove(string key)
    {
        if (_useRedis)
            _redisCache.Del(key);
        else
            _memoryCache.Remove(key);
    }

    public bool Exists(string key)
    {
        return _useRedis 
            ? _redisCache.Exists(key) 
            : _memoryCache.TryGetValue(key, out _);
    }
    #endregion

    #region 异步方法
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (_useRedis)
        {
            var timeoutSeconds = expiration?.TotalSeconds ?? TimeSpan.FromSeconds(-1).Seconds;
            await _redisCache.SetAsync(key, value, (int)Math.Round(timeoutSeconds));
        }
        else
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            
            _memoryCache.Set(key, value, options);
            await Task.CompletedTask; // 内存缓存无异步方法，返回已完成任务
        }
    }

    public async Task<T> GetAsync<T>(string key)
    {
        return _useRedis 
            ? await _redisCache.GetAsync<T>(key) 
            : await Task.FromResult(_memoryCache.Get<T>(key));
    }

    public Task<bool> TryGetValueAsync<T>(string key, out Task<T> value)
    {
        if (_useRedis)
        {
            value = _redisCache.GetAsync<T>(key);
            return Task.FromResult(true); // Redis 需等待结果后判断，这里简化处理
        }
        else
        {
            var exists = _memoryCache.TryGetValue(key, out T result);
            value = Task.FromResult(result);
            return Task.FromResult(exists);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (_useRedis)
            await _redisCache.DelAsync(key);
        else
        {
            _memoryCache.Remove(key);
            await Task.CompletedTask;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return _useRedis 
            ? await _redisCache.ExistsAsync(key) 
            : await Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
    #endregion
}