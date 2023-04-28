
using System.Data;

namespace FytSoa.Sugar;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class UnitOfWorkAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public UnitOfWorkAttribute()
    {
    }
    
    /// <summary>
    /// 事务隔离级别
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>
    /// <para>支持传入事务隔离级别 <see cref="IsolationLevel"/> 参数值</para>
    /// </remarks>
    /// <param name="isolationLevel">事务隔离级别</param>
    public UnitOfWorkAttribute(IsolationLevel isolationLevel)
    {
        IsolationLevel = isolationLevel;
    }
}