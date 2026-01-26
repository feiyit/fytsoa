using System.ComponentModel.DataAnnotations;
using SqlSugar;
using FytSoa.Domain.Sys;

namespace FytSoa.Domain.Am;

/// <summary>
/// 资产分类扩展字段定义（按分类配置字段模板）。
/// 分类复用 sys_codetype。
/// 对应表：am_asset_attr_def
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_asset_attr_def")]
public class AmAssetAttrDef : Entity
{
    /// <summary>
    /// 分类Id（sys_codetype.Id）
    /// </summary>
    [Required]
    public long CategoryId { get; set; } = 0;

    /// <summary>
    /// 字段Key（唯一标识）
    /// </summary>
    [Required]
    [StringLength(64)]
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>
    /// 字段名称
    /// </summary>
    [Required]
    [StringLength(128)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型：string/number/date/bool/select/multi-select等
    /// </summary>
    [Required]
    [StringLength(32)]
    public string DataType { get; set; } = "string";

    /// <summary>
    /// 选项/校验等扩展配置(JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "json")]
    public string? OptionsJson { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    [Required]
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    [StringLength(50)]
    public string? CreateUser { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [StringLength(50)]
    public string? UpdateUser { get; set; }

    #region 导航属性

    /// <summary>
    /// 分类信息（sys_codetype）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CategoryId))]
    public SysCode? CategoryObj { get; set; }

    #endregion
}
