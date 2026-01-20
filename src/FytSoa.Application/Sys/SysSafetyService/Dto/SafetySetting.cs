namespace FytSoa.Application.Sys;
/// <summary>
/// 系统安全设置模型
/// </summary>
public class SafetySetting
{
    /// <summary>
    /// 敏感词
    /// </summary>
    public string Sensitivity { get; set; }

    /// <summary>
    /// IP黑名单
    /// </summary>
    public string IpBlacklist { get; set; }

    /// <summary>
    /// 上传文件白名单
    /// </summary>
    public string UploadWhitelist { get; set; }
}