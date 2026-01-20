using FytSoa.Common.Param;

namespace FytSoa.Application.Cms;

/// <summary>
/// 产品分页查询参数
/// </summary>
public class CmsProductParam : PageParam
{
    /// <summary>
    /// 栏目 Id 集合（分类）
    /// </summary>
    public List<long> ColumnList { get; set; } = new();

    /// <summary>
    /// 栏目英文标识
    /// </summary>
    public string EnTitle { get; set; }

    /// <summary>
    /// 最低价格
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// 最高价格
    /// </summary>
    public decimal? MaxPrice { get; set; }
}

