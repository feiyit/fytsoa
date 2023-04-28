namespace FytSoa.Exam;

public static class UrlOption
{
    public static string baseUrl = "http://localhost:5000";

    /// <summary>
    /// 处理单独Url信息
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string UrlReplace(this string url)
    {
        if (url.ToLower().StartsWith("http"))
        {
            return url;
        }
        if (string.IsNullOrEmpty(url))
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