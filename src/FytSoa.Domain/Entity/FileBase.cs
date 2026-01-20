namespace FytSoa.Domain;

/// <summary>
/// 文件基本信息
/// </summary>
public class FileBase
{
    /// <summary>
    /// 文件地址
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public Int64 Size { get; set; } = 0;
}