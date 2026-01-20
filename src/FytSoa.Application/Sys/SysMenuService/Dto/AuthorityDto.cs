using System.Text.Json.Serialization;

namespace FytSoa.Application.Sys;

/// <summary>
/// 根据权限返回前端资源+指令权限
/// </summary>
public class AuthorityDto
{
    /// <summary>
    /// 资源权限
    /// </summary>
    public List<AuthorityMenuDto> Menu { get; set; } = new();

    /// <summary>
    /// 指令权限
    /// </summary>
    public List<string> Directive { get; set; } = new();

    public string Workbench { get; set; } = "work";
}

/// <summary>
/// 返回给前端的权限菜单
/// </summary>
public class AuthorityMenuDto
{
    /// <summary>
    /// 地址
    /// </summary>
    public string path { get; set; }

    /// <summary>
    /// 组件
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string redirect { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public AuthorityMeta meta { get; set; }

    //public bool alwaysShow { get; set; } = true;

    /// <summary>
    /// 子级
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AuthorityMenuDto> children { get; set; }
}
public class AuthorityMeta {

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 图表
    /// </summary>
    public string icon { get; set; }

    public bool affix { get; set; } = false;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string type { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool? hidden { get; set; } = null;
        
    [JsonIgnore]
    public bool? dynamicNewTab { get; set; } = null;
    
    /// <summary>
    /// 是否全屏
    /// </summary>
    public bool? fullpage { get; set; } = null;

    /// <summary>
    /// 是否关闭
    /// </summary>
    [JsonIgnore]
    public bool noClosable { get; set; } = false;

    //不缓存
    [JsonIgnore]
    public bool noKeepAlive { get; set; } = false;

    /// <summary>
    /// 按钮权限
    /// </summary>
    [JsonIgnore]
    public string[] roles { get; set; }
}