namespace FytSoa.Application.Sys;

/// <summary>
/// 个人信息基本修改
/// </summary>
public class BasicParam
{
    public long Id { get; set; }
    public string FullName { get; set; }
    
    public string Sex { get; set; }
    
    public string Summary { get; set; }
}