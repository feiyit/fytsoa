using Snowflake.Core;

namespace FytSoa.Common.Utils;

/// <summary>
/// 唯一值  -  雪花算法
/// </summary>
public class Unique
{
    private static readonly Unique Instance = new Unique();
    private Unique() { }
    private static IdWorker _worker;
    public static Unique GetInstance()
    {
        _worker = new IdWorker(1, 1);
        return Instance;
    }

    /// <summary>
    /// 返回唯一ID值
    /// </summary>
    /// <returns></returns>
    public static long Id()
    {
        return _worker.NextId();
    }
}