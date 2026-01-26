using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产分类扩展字段定义分页查询参数
/// </summary>
public class AmAssetAttrDefParam : PageParam
{
    public long CategoryId { get; set; } = 0;
    public bool? IsEnabled { get; set; } = null;
    public bool? IsRequired { get; set; } = null;
}

