using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 仓库库位 DTO
/// </summary>
public class AmWarehouseBinDto : AppEntity
{
    public long WarehouseId { get; set; } = 0;

    [StringLength(64)]
    public string? Code { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    public int Sort { get; set; } = 0;

    public bool Status { get; set; } = true;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
    
    #region 导航属性

    /// <summary>
    /// 所属仓库信息
    /// </summary>
    public AmWarehouseDto? WarehouseObj { get; set; }

    #endregion
}

