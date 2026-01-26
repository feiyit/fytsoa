using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 资产盘点计划 DTO
/// </summary>
public class AmInventoryPlanDto : AppEntity
{
    [Required]
    [StringLength(64)]
    public string PlanNo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 盘点范围(JSON)：地点/部门/分类/仓库等
    /// </summary>
    public string? ScopeJson { get; set; }

    /// <summary>
    /// 盘点执行人(JSON)：用户Id列表等
    /// </summary>
    public string? AssigneeIdsJson { get; set; }

    /// <summary>
    /// 状态：0=草稿,1=进行中,2=已完成,3=已取消
    /// </summary>
    public byte Status { get; set; } = 0;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [StringLength(1000)]
    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? CreateUser { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string? UpdateUser { get; set; }

    /// <summary>
    /// 盘点明细（详情页返回）
    /// </summary>
    public List<AmInventoryItemDto> Items { get; set; } = new();
}

