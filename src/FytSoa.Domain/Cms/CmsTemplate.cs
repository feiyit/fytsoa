using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Cms;

/// <summary>
/// 模板表 
/// </summary>
[SugarTable("cms_template")]
public class CmsTemplate:Entity
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Name { get; set; }

    /// <summary>
    /// 模板地址
    /// </summary>
    public string Urls { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 添加人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}