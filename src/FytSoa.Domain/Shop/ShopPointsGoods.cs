using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Shop;

/// <summary>
/// 积分商城-商品表
/// </summary>
[SugarTable("shop_points_goods")]
public class ShopPointsGoods:Entity
{
    /// <summary>
    /// 商品名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Number { get; set; }

    /// <summary>
    /// 兑换需要积分
    /// </summary>
    [Required]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 原价
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// 商品图片
    /// </summary>
    public string ImgUrls { get; set; }
    
    /// <summary>
    /// 商品规格
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<ShopPointGoodsSpecs> Specs { get; set; } = new();
    
    /// <summary>
    /// 限购数量
    /// </summary>
    [Required]
    public int Limits { get; set; } = 0;

    /// <summary>
    /// 库存
    /// </summary>
    [Required]
    public int Stock { get; set; } = 0;

    /// <summary>
    /// 销售数量
    /// </summary>
    [Required]
    public int Sales { get; set; } = 0;

    /// <summary>
    /// 上架状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }=DateTime.Now;


}

/// <summary>
/// 商品规格
/// </summary>
public class ShopPointGoodsSpecs
{
    public string Name { get; set; }

    public string Value { get; set; }
}