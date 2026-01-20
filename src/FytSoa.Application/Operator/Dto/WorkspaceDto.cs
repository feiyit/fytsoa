using FytSoa.Application.Cms;

namespace FytSoa.Application.Dto;

/// <summary>
/// 工作台返回信息
/// </summary>
public class WorkspaceDto
{
    /// <summary>
    /// 今日文章
    /// </summary>
    public int ArticleCountDay { get; set; }
    
    /// <summary>
    /// 所有文章
    /// </summary>
    public int ArticleCountAll { get; set; }
    
    /// <summary>
    /// 待审核
    /// </summary>
    public int ArticleCountCheck { get; set; }
    
    /// <summary>
    /// 待评论
    /// </summary>
    public int ArticleCountComment { get; set; }
    
    /// <summary>
    /// 草稿
    /// </summary>
    public int ArticleCountDraft { get; set; }

    /// <summary>
    /// 最新发布的文章
    /// </summary>
    public List<CmsArticleDto> NewestArticle { get; set; } = [];

    /// <summary>
    /// 文章浏览数据
    /// </summary>
    public List<ArticleViewDto> ViewList { get; set; } = [];
    
    /// <summary>
    /// 文章浏览对象
    /// </summary>
    public class ArticleViewDto
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}