using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FytSoa.Common.Param;

public class WhereParam:CommonParam
{
        
}
    
public class WhereParam<T>
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public T Filter { get; set; }
}

/// <summary>
/// 公共条件参数
/// </summary>
public class CommonParam
{
    /// <summary>
    /// 编号
    /// </summary>
    public long Id { get; set; } = 0;
    
    /// <summary>
    /// 租户编号
    /// </summary>
    public long TenantId { get; set; } = 0;
        
    /// <summary>
    /// 关键字
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 状态值  1=true  2=false  0=默认
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 类型值
    /// </summary>
    public int Type { get; set; } = 0;

    /// <summary>
    /// 时间，支持区间
    /// </summary>
    public string Times { get; set; }

    /// <summary>
    /// 高级查询Json，支持and or 等
    /// </summary>
    public string Query { get; set; }

}

/// <summary>
/// 高级查询Json
/// </summary>
public class CommonJsonParam
{
    /// <summary>
    /// 查询的字段
    /// </summary>
    public string FieldName { get; set; }
    
    /// <summary>
    /// 查询的条件
    /// </summary>
    public int ConditionalType { get; set; }
    
    /// <summary>
    /// 查询的值
    /// </summary>
    public string FieldValue { get; set; }
}