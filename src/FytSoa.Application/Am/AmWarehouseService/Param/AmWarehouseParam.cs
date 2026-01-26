using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 仓库分页查询参数
/// </summary>
public class AmWarehouseParam : PageParam
{
    public long LocationId { get; set; } = 0;
}

