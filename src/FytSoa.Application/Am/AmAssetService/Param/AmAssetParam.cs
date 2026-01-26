using FytSoa.Common.Param;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产分页查询参数
/// </summary>
public class AmAssetParam : PageParam
{
    /// <summary>
    /// 资产状态：0=全部；其它值对应 am_asset.Status
    /// </summary>
    public int AssetStatus { get; set; } = 0;

    /// <summary>
    /// 分类Id（sys_code.Id）
    /// </summary>
    public long CategoryId { get; set; } = 0;

    /// <summary>
    /// 部门Id（sys_org_unit.Id）
    /// </summary>
    public long OrgUnitId { get; set; } = 0;

    /// <summary>
    /// 地点Id（am_location.Id）
    /// </summary>
    public long LocationId { get; set; } = 0;

    /// <summary>
    /// 仓库Id（am_warehouse.Id）
    /// </summary>
    public long WarehouseId { get; set; } = 0;

    /// <summary>
    /// 供应商Id（am_vendor.Id）
    /// </summary>
    public long VendorId { get; set; } = 0;

    /// <summary>
    /// 是否包含已删除数据（am_asset.IsDel）
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
}
