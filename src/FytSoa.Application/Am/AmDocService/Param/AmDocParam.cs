using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 单据分页查询参数
/// </summary>
public class AmDocParam : PageParam
{
    public string? DocType { get; set; }
    public string? SubType { get; set; }

    /// <summary>
    /// 单据状态：0=全部；其它值对应 am_doc.Status
    /// </summary>
    public int DocStatus { get; set; } = 0;

    /// <summary>
    /// 是否包含已删除（am_doc.IsDel）
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
}

