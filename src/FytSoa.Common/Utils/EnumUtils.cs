using System.ComponentModel;
using System.Reflection;

namespace FytSoa.Common.Utils;

public static class EnumUtils
{
    /// <summary>
    /// 获得枚举的描述信息
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string ToDescription(this System.Enum enumValue)
    {
        var value = enumValue.ToString();
        var field = enumValue.GetType().GetField(value);
        object[] objs = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);    
        if (objs.Length == 0)   
            return value;
        DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
        return descriptionAttribute.Description;
    }
}