using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产折旧分页查询参数
/// </summary>
public class AmAssetDepreciationParam : PageParam
{
    public long AssetId { get; set; } = 0;

    /// <summary>
    /// 方法：0=全部；其它值对应 am_asset_depreciation.Method
    /// </summary>
    public int Method { get; set; } = 0;

    /// <summary>
    /// 状态：0=全部；其它值对应 am_asset_depreciation.Status
    /// </summary>
    public int DepStatus { get; set; } = 0;
}

