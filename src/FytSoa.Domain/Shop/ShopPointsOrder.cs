using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Shop;

/// <summary>
/// 积分商城-订单表
/// </summary>
[SugarTable("shop_points_order")]
public class ShopPointsOrder:Entity
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// 商品信息
    /// </summary>
    [Required]
    public long GoodsId { get; set; }

    /// <summary>
    /// 支付积分
    /// </summary>
    [Required]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 兑换数量
    /// </summary>
    [Required]
    public int Count { get; set; } = 1;

    /// <summary>
    /// 订单状态
    /// </summary>
    [Required]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 订单日期
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 确认订单日期
    /// </summary>
    public DateTime? SuccessTime { get; set; }


}