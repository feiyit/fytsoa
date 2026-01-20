namespace FytSoa.Application.Sys;

public class SysFileDto
{
    /// <summary>
    /// 文件地址
    /// </summary>
    public string path { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public Int64 size { get; set; } = 0;
}