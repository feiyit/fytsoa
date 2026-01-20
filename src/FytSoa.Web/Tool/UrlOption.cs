using FytSoa.Common.Utils;

namespace FytSoa.Web;

public static class UrlOption
{
    public static string baseUrl = AppUtils.Configuration["Service"];

    /// <summary>
    /// 处理单独Url信息
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string UrlReplace(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }
        if (url.ToLower().StartsWith("http"))
        {
            return url;
        }
        if (url.ToLower().Contains(baseUrl))
        {
            return url;
        }

        return baseUrl + url;
    }
}