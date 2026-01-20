namespace FytSoa.Application.Sys;

/// <summary>
/// 根据权限返回前端资源+指令权限
/// </summary>
public class AuthorityVbanDto
{
    /// <summary>
    /// 资源权限
    /// </summary>
    public List<RouteVbanRecord> Menu { get; set; } = new();

    /// <summary>
    /// 指令权限
    /// </summary>
    public List<string> Directive { get; set; } = new();

    public string Workbench { get; set; } = "workspace";
}

/// <summary>
/// 路由记录实体类，对应前端的RouteRecordRaw
/// </summary>
public class RouteVbanRecord
{
    /// <summary>
    /// 路由元信息
    /// </summary>
    public RouteVbanMeta Meta { get; set; }
        
    /// <summary>
    /// 路由名称
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// 路由路径
    /// </summary>
    public string Path { get; set; }
        
    /// <summary>
    /// 重定向路径
    /// </summary>
    public string Redirect { get; set; }
        
    /// <summary>
    /// 子路由集合
    /// </summary>
    public List<RouteVbanRecord> Children { get; set; } = new List<RouteVbanRecord>();
        
    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }
    
    /// <summary>
    /// 路由类型
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
/// 路由元信息实体类
/// </summary>
public class RouteVbanMeta
{
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 是否缓存组件
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// 排序序号
    /// </summary>
    public int Order { get; set; } = 1;
        
    /// <summary>
    /// 标题（国际化键值）
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 固定状态
    /// </summary>
    public bool AffixTab { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    public string Link { get; set; }
}