using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产分类扩展字段定义 DTO
/// </summary>
public class AmAssetAttrDefDto : AppEntity
{
    public long CategoryId { get; set; } = 0;

    [Required]
    [StringLength(64)]
    public string FieldKey { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string DataType { get; set; } = "string";

    public string? OptionsJson { get; set; }

    public bool IsRequired { get; set; } = false;

    public bool IsEnabled { get; set; } = true;

    public int Sort { get; set; } = 0;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
    
    /// <summary>
    /// 分类信（sys_codetype）
    /// </summary>
    public SysCodeSimpleDto CategoryObj { get; set; }
}

