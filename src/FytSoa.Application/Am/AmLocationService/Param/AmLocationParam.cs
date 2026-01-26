using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 地点分页查询参数
/// </summary>
public class AmLocationParam : PageParam
{
    public long ParentId { get; set; } = 0;
}

