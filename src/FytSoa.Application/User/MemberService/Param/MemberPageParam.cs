using FytSoa.Common.Param;

namespace FytSoa.Application.User;

public class MemberPageParam:PageParam
{
    /// <summary>
    /// 支持多个用户查询
    /// </summary>
    public string UserIds { get; set; }
}