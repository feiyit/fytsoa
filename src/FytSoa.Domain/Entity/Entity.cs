using FytSoa.Common.Utils;
using SqlSugar;

namespace FytSoa.Domain;

public class Entity
{
    protected Entity()
    {
        Id = Unique.Id();
    }
        
    /// <summary>
    /// 唯一编号
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    public long TenantId { get; set; } = 0;
}