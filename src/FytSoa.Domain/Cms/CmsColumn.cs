using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Cms;

/// <summary>
/// 栏目表 
/// </summary>
[SugarTable("cms_column")]
public class CmsColumn:Entity
{
    /// <summary>
    /// 父编号
    /// </summary>
    [Required]
    public long ParentId { get; set; }

    /// <summary>
    /// 父编号集合
    /// </summary>
    [Required]
    [StringLength(500)]
    [SugarColumn(IsJson = true)]
    public List<string> ParentIdList { get; set; }

    /// <summary>
    /// 栏目编号
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// 栏目标题
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Title { get; set; }

    /// <summary>
    /// 栏目英文标题
    /// </summary>
    public string EnTitle { get; set; }

    /// <summary>
    /// 栏目副标题
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [Required]
    public int Layer { get; set; } = 1;

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Sort { get; set; } = 1;

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 站点ID
    /// </summary>
    [Required]
    public long SiteId { get; set; }

    /// <summary>
    /// 模板ID
    /// </summary>
    [Required]
    public long TemplateId { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string TemplateName { get; set; }

    /// <summary>
    /// 栏目图片
    /// </summary>
    public string ImgUrl { get; set; }

    /// <summary>
    /// 栏目外链
    /// </summary>
    public string LinkUrl { get; set; }

    /// <summary>
    /// 栏目关键字
    /// </summary>
    public string KeyWord { get; set; }

    /// <summary>
    /// 栏目描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 栏目内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;

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