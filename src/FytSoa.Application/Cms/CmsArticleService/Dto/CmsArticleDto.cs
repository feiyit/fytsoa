using System;
using FytSoa.Common.Utils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Cms;

/// <summary>
/// 文章表 
/// </summary>
public class CmsArticleDto : AppEntity
{
    /// <summary>
    /// 栏目Id
    /// </summary>
    public long ColumnId { get; set; }
    
    /// <summary>
    /// 栏目名称
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 栏目Id-List
    /// </summary>
    public List<string> ColumnArr { get; set; } = new ();

    /// <summary>
    /// 文章标题
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Title { get; set; }

    /// <summary>
    /// 标题颜色
    /// </summary>
    public string TitleColor { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// 权重
    /// </summary>
    public int Sort { get; set; } = 0;

    /// <summary>
    /// 外部链接地址
    /// </summary>
    public string LinkUrl { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tag { get; set; }=new();

    /// <summary>
    /// 宣传图 支持多图
    /// </summary>
    public string ImgUrl { get; set; }

    /// <summary>
    /// 视频地址
    /// </summary>
    public string VideoUrl { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// Seo关键字
    /// </summary>
    public string KeyWord { get; set; }

    /// <summary>
    /// Seo描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 文章内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public List<int> Attr { get; set; }=new();

    /// <summary>
    /// 点击量
    /// </summary>
    public int Hits { get; set; } = 0;

    /// <summary>
    /// 当日点击量
    /// </summary>
    public int DayHits { get; set; } = 0;

    /// <summary>
    /// 星期点击量
    /// </summary>
    public int WeedHits { get; set; } = 0;

    /// <summary>
    /// 月点击量
    /// </summary>
    public int MonthHits { get; set; } = 0;

    /// <summary>
    /// 最后点击时间
    /// </summary>
    public DateTime? LastHitDate { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeleteTime { get; set; }

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
    public DateTime UpdateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}