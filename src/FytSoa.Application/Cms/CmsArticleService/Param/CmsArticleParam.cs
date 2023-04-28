
using FytSoa.Common.Param;

namespace FytSoa.Application.Cms;

public class CmsArticleParam:PageParam
{
    public bool Recycle { get; set; } = false;

    public int? Attr { get; set; } = 0;

    public List<long> ColumnList { get; set; } = new();
}

public class CmsArticleCopyParam {
        
    public long ColumnId { get; set; } = 0;

    public string ArticleIds { get; set; }

    //1=copy 2=transfer
    public int Type { get; set; } = 1;
}