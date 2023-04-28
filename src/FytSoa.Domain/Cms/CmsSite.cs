using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Cms;

/// <summary>
/// 站点表 
/// </summary>
[SugarTable("cms_site")]
public class CmsSite:Entity
{
    /// <summary>
    /// 网站名称
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Name { get; set; }

    /// <summary>
    /// 网站Logo
    /// </summary>
    public string Logo { get; set; }

    /// <summary>
    /// 网站网址
    /// </summary>
    public string SiteUrl { get; set; }

    /// <summary>
    /// SEO标题
    /// </summary>
    public string SeoTitle { get; set; }

    /// <summary>
    /// SEO关键字
    /// </summary>
    public string SeoKey { get; set; }

    /// <summary>
    /// SEO描述
    /// </summary>
    public string SeoDescribe { get; set; }

    /// <summary>
    /// 网站版权信息
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    /// 网站开启状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 网站关闭原因
    /// </summary>
    public string CloseInfo { get; set; }

    /// <summary>
    /// 公司电话
    /// </summary>
    public string CompanyTel { get; set; }

    /// <summary>
    /// 公司传真
    /// </summary>
    public string CompanyFax { get; set; }

    /// <summary>
    /// 公司邮箱
    /// </summary>
    public string CompanyEmail { get; set; }

    /// <summary>
    /// 公司地址
    /// </summary>
    public string CompanyAddress { get; set; }

    /// <summary>
    /// 客服信息
    /// </summary>
    public string CustomerService { get; set; }

    /// <summary>
    /// 二维码Json(Key:Value)
    /// </summary>
    public string Codes { get; set; }

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