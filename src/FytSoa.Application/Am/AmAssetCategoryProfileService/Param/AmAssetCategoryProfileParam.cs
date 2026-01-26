using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产分类扩展信息分页查询参数
/// </summary>
public class AmAssetCategoryProfileParam : PageParam
{
    public long CategoryId { get; set; } = 0;
}

