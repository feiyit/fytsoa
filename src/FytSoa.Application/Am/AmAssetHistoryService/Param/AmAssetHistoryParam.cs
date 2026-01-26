using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产历史分页查询参数
/// </summary>
public class AmAssetHistoryParam : PageParam
{
    public long AssetId { get; set; } = 0;
    public string? BizType { get; set; }
    public long BizId { get; set; } = 0;
    public string? Operation { get; set; }
}

