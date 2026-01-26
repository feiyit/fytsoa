namespace FytSoa.Application.Am;

/// <summary>
/// 附件批量绑定参数（以 BizType+BizId 为维度）
/// </summary>
public class AmFileRefBindParam : AppEntity
{
    public string BizType { get; set; } = string.Empty;

    public long BizId { get; set; } = 0;

    public List<AmFileRefDto> Files { get; set; } = new();
}
