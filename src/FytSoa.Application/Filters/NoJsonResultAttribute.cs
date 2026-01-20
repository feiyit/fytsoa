namespace FytSoa.Application;

/// <summary>
/// 不对结果做统一处理
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class NoJsonResultAttribute : Attribute
{
    
}