using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Shop;
using FytSoa.Domain.User;
using SqlSugar;

namespace FytSoa.Application.Shop;

/// <summary>
/// 积分商城-订单表
/// </summary>
public class ShopPointsOrderDto : AppEntity
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
    public int Status { get; set; } = 0;

    /// <summary>
    /// 订单日期
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 确认订单日期
    /// </summary>
    public DateTime? SuccessTime { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public Member User { get; set; }
    
    /// <summary>
    /// 商品信息
    /// </summary>
    public ShopPointsGoods Goods { get; set; }
}