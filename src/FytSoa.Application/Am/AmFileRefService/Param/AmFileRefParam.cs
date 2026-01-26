using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 附件关联分页查询参数
/// </summary>
public class AmFileRefParam : PageParam
{
    public string? BizType { get; set; }

    public long BizId { get; set; } = 0;

    public long FileId { get; set; } = 0;
}

