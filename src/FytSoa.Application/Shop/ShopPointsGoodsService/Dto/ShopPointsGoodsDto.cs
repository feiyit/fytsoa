using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Shop;
using SqlSugar;

namespace FytSoa.Application.Shop;

/// <summary>
/// 积分商城-商品表
/// </summary>
public class ShopPointsGoodsDto : AppEntity
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
    public string Number { get; set; }

    /// <summary>
    /// 兑换需要积分
    /// </summary>
    [Required]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 原价
    /// </summary>
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
    public int Sales { get; set; } = 0;

    /// <summary>
    /// 上架状态
    /// </summary>
    public bool Status { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;


}