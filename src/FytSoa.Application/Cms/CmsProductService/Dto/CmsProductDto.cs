using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Cms;

namespace FytSoa.Application.Cms;

/// <summary>
/// 产品表 DTO
/// </summary>
public class CmsProductDto : AppEntity
{
    /// <summary>
    /// 所属栏目 Id
    /// </summary>
    public long ColumnId { get; set; }

    /// <summary>
    /// 栏目名称
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Title { get; set; }

    /// <summary>
    /// 产品副标题
    /// </summary>
    [StringLength(255)]
    public string SubTitle { get; set; }

    /// <summary>
    /// 产品编号/货号
    /// </summary>
    [StringLength(64)]
    public string ProductNo { get; set; }

    /// <summary>
    /// 计量单位，如 件/箱/套
    /// </summary>
    [StringLength(32)]
    public string Unit { get; set; }

    /// <summary>
    /// 销售价
    /// </summary>
    public decimal Price { get; set; } = 0m;

    /// <summary>
    /// 市场价
    /// </summary>
    public decimal MarketPrice { get; set; } = 0m;

    /// <summary>
    /// 成本价
    /// </summary>
    public decimal CostPrice { get; set; } = 0m;

    /// <summary>
    /// 封面图
    /// </summary>
    public string ImgUrl { get; set; }

    /// <summary>
    /// 产品相册(多图)
    /// </summary>
    public List<string> Album { get; set; } = new();

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// 自定义参数
    /// </summary>
    public List<CmsProduct.ExtendModel> Extend { get; set; } = new();

    /// <summary>
    /// 简要介绍
    /// </summary>
    [StringLength(500)]
    public string Intro { get; set; }

    /// <summary>
    /// 详细介绍/图文内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 排序(值越大越靠前)
    /// </summary>
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 状态(上架/下架)
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 总点击量
    /// </summary>
    public int Hits { get; set; } = 0;

    /// <summary>
    /// 当日点击量
    /// </summary>
    public int DayHits { get; set; } = 0;

    /// <summary>
    /// 本周点击量
    /// </summary>
    public int WeedHits { get; set; } = 0;

    /// <summary>
    /// 本月点击量
    /// </summary>
    public int MonthHits { get; set; } = 0;

    /// <summary>
    /// 最后点击时间
    /// </summary>
    public DateTime? LastHitDate { get; set; }

    /// <summary>
    /// 删除时间(逻辑删除)
    /// </summary>
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }
}
