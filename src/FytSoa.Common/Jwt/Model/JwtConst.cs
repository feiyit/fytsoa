namespace FytSoa.Common.Jwt.Model;

public class JwtConst
{
    /// <summary>
    /// 受理人，强制Token失效
    /// 用户id，生成token和验证token的时候获取到持久化的值去校验
    /// 如果重新登陆，则刷新这个值
    /// </summary>
    public static string ValidAudience;
}