namespace FytSoa.Generator.Utils;

/// <summary>
/// 常量信息设置
/// </summary>
public class EntityConstant
{
    /// <summary>
    /// 忽略非公共类属性
    /// </summary>
    public static List<string> IgnoreDtoColumn = new () { "Id","TenantId"};
}