namespace FytSoa.Domain;

/// <summary>
/// 公共自定义实体
/// </summary>
public class KeyValueBase
{
    public string Key { get; set; }
    
    public string Value { get; set; }
}

/// <summary>
/// 下拉实体
/// </summary>
public class SelectBase
{
    public string Label { get; set; }
    
    public string Value { get; set; }
}