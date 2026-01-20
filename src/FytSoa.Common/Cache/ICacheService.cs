using System;

namespace FytSoa.Common.Cache;

/// <summary>
/// 缓存接口
/// </summary>
public interface ICacheService
{
    // 同步方法
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    T Get<T>(string key);
    bool TryGetValue<T>(string key, out T value);
    void Remove(string key);
    bool Exists(string key);

    // 异步方法
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T> GetAsync<T>(string key);
    Task<bool> TryGetValueAsync<T>(string key, out Task<T> value);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}