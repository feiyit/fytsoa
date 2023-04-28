using System.ComponentModel.DataAnnotations;
using FytSoa.Common.Enum;
using SqlSugar;

namespace FytSoa.Domain.Shop;

/// <summary>
/// 
/// </summary>
[SugarTable("shop_points_log")]
public class ShopPointsLog:Entity
{
    /// <summary>
    /// 会员编号
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// 增加还是减少——1=增加 2=减少
    /// </summary>
    [Required]
    public int Flag { get; set; } = 1;
    
    /// <summary>
    /// 积分数
    /// </summary>
    [Required]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    public ShopPointLogEnum GetType { get; set; } = 0;

    /// <summary>
    /// 时间
    /// </summary>
    [Required]
    public DateTime AddTime { get; set; }=DateTime.Now;


}