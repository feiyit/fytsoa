using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;
using FytSoa.Domain.Sys;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产分类扩展信息 DTO
/// </summary>
public class AmAssetCategoryProfileDto : AppEntity
{
    [Required]
    public long CategoryId { get; set; } = 0;

    public byte DepMethod { get; set; } = 0;

    public int DepLifeMonths { get; set; } = 0;

    public decimal SalvageRate { get; set; } = 0m;

    /// <summary>
    /// 分类扩展配置(JSON)
    /// </summary>
    public string? ExtJson { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
    
    public SysCodeSimpleDto? CategoryObj { get; set; }
}

