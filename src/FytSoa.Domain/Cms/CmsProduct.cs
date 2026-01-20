using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Cms;

/// <summary>
/// 产品表
/// </summary>
[SugarTable("cms_product")]
public class CmsProduct
{
    /// <summary>
    /// 唯一编号
    /// </summary>
    [SugarColumn(IsPrimaryKey = true,ColumnName = "id")]
    public long Id { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    [SugarColumn(ColumnName = "tenant_id")]
    public long TenantId { get; set; } = 0;
    
    /// <summary>
    /// 所属栏目 Id
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "column_id")]
    public long ColumnId { get; set; } = 0;

    /// <summary>
    /// 产品名称
    /// </summary>
    [Required]
    [StringLength(128)]
    [SugarColumn(ColumnName = "title")]
    public string Title { get; set; }

    /// <summary>
    /// 产品副标题
    /// </summary>
    [StringLength(255)]
    [SugarColumn(ColumnName = "sub_title")]
    public string SubTitle { get; set; }

    /// <summary>
    /// 产品编号/货号
    /// </summary>
    [StringLength(64)]
    [SugarColumn(ColumnName = "product_no")]
    public string ProductNo { get; set; }

    /// <summary>
    /// 计量单位，如 件/箱/套
    /// </summary>
    [StringLength(32)]
    [SugarColumn(ColumnName = "unit")]
    public string Unit { get; set; }

    /// <summary>
    /// 销售价
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "price")]
    public decimal Price { get; set; } = 0m;

    /// <summary>
    /// 市场价
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "market_price")]
    public decimal MarketPrice { get; set; } = 0m;

    /// <summary>
    /// 成本价
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "cost_price")]
    public decimal CostPrice { get; set; } = 0m;

    /// <summary>
    /// 封面图
    /// </summary>
    [SugarColumn(ColumnName = "img_url")]
    public string ImgUrl { get; set; }

    /// <summary>
    /// 产品相册(多图)，JSON 数组
    /// </summary>
    [SugarColumn(ColumnName = "album", IsJson = true)]
    public List<string> Album { get; set; } = new();

    /// <summary>
    /// 标签，JSON 数组
    /// </summary>
    [SugarColumn(ColumnName = "tags", IsJson = true)]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// 自定义参数，JSON
    /// </summary>
    [SugarColumn(ColumnName = "extend", IsJson = true)]
    public List<ExtendModel> Extend { get; set; } = new();

    public class ExtendModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// 简要介绍
    /// </summary>
    [StringLength(500)]
    [SugarColumn(ColumnName = "intro")]
    public string Intro { get; set; }

    /// <summary>
    /// 详细介绍/图文内容
    /// </summary>
    [SugarColumn(ColumnName = "content")]
    public string Content { get; set; }

    /// <summary>
    /// 排序(值越大越靠前)
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "sort")]
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 状态(上架/下架)
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 总点击量
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "hits")]
    public int Hits { get; set; } = 0;

    /// <summary>
    /// 当日点击量
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "day_hits")]
    public int DayHits { get; set; } = 0;

    /// <summary>
    /// 本周点击量
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "weed_hits")]
    public int WeedHits { get; set; } = 0;

    /// <summary>
    /// 本月点击量
    /// </summary>
    [Required]
    [SugarColumn(ColumnName = "month_hits")]
    public int MonthHits { get; set; } = 0;

    /// <summary>
    /// 最后点击时间
    /// </summary>
    [SugarColumn(ColumnName = "last_hit_date")]
    public DateTime? LastHitDate { get; set; }

    /// <summary>
    /// 删除时间(逻辑删除)
    /// </summary>
    [SugarColumn(ColumnName = "delete_time")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [SugarColumn(ColumnName = "publish_time")]
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    [SugarColumn(ColumnName = "create_user")]
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [SugarColumn(ColumnName = "update_time")]
    public DateTime UpdateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    [SugarColumn(ColumnName = "update_user")]
    public string UpdateUser { get; set; }
}
