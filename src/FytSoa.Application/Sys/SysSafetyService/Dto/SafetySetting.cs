namespace FytSoa.Application.Sys;
/// <summary>
/// 系统安全设置模型
/// </summary>
public class SafetySetting
{
    /// <summary>
    /// 敏感词
    /// </summary>
    public string Sensitive { get; set; }

    /// <summary>
    /// IP黑名单
    /// </summary>
    public string IpLimit { get; set; }

    /// <summary>
    /// 上传文件白名单
    /// </summary>
    public string UploadWhite { get; set; }
}