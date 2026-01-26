using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;

namespace FytSoa.Application.Am;

/// <summary>
/// 仓库 DTO
/// </summary>
public class AmWarehouseDto : AppEntity
{
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    public long LocationId { get; set; } = 0;

    public long ManagerId { get; set; } = 0;

    [StringLength(50)]
    public string? Phone { get; set; }

    public bool Status { get; set; } = true;

    [StringLength(512)]
    public string? Summary { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
    
    #region 导航属性

    /// <summary>
    /// 所在地点信息
    /// </summary>
    public AmLocationDto? LocationObj { get; set; }

    /// <summary>
    /// 仓库管理员信息
    /// </summary>
    public SysAdminSimpleDto? ManagerObj { get; set; }

    #endregion
}

