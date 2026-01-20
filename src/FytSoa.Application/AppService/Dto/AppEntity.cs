namespace FytSoa.Application;

public class AppEntity
{
    /// <summary>
    /// 唯一编号
    /// </summary>
    public long Id { get; set; } = 0;

    /// <summary>
    /// 租户编号
    /// </summary>
    public long TenantId { get; set; } = 0;
}