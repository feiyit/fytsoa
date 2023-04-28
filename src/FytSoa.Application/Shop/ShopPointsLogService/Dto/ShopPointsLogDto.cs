using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FytSoa.Common.Enum;
using SqlSugar;

namespace FytSoa.Application.Shop;

/// <summary>
/// 
/// </summary>
public class ShopPointsLogDto : AppEntity
{
    /// <summary>
    /// 会员编号
    /// </summary>
    [Required]
    public long UserId { get; set; } = 0;

    /// <summary>
    /// 增加还是减少——1=增加 2=减少
    /// </summary>
    [Required]
    public int Flag { get; set; } = 1;

    /// <summary>
    /// 获得积分方式
    /// </summary>
    public ShopPointLogEnum GetType { get; set; } = 0;
    
    /// <summary>
    /// 获得积分方式
    /// </summary>
    public string GetTypeName { get; set; } 
    
    /// <summary>
    /// 积分数
    /// </summary>
    [Required]
    public int Point { get; set; } = 0;

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime AddTime { get; set; }=DateTime.Now;


}