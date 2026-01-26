using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 库位分页查询参数
/// </summary>
public class AmWarehouseBinParam : PageParam
{
    public long WarehouseId { get; set; } = 0;
}

