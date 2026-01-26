using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.Am;

/// <summary>
/// 供应商。
/// 对应表：am_vendor
/// 说明：公司维度使用 TenantId（继承自 Entity.TenantId）。
/// </summary>
[SugarTable("am_vendor")]
public class AmVendor : Entity
{
    /// <summary>
    /// 供应商编码
    /// </summary>
    [StringLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 供应商名称
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 联系人
    /// </summary>
    [StringLength(50)]
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [StringLength(50)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(255)]
    public string? Address { get; set; }

    /// <summary>
    /// 税号
    /// </summary>
    [StringLength(64)]
    public string? TaxNo { get; set; }

    /// <summary>
    /// 开户行
    /// </summary>
    [StringLength(100)]
    public string? BankName { get; set; }

    /// <summary>
    /// 银行账号
    /// </summary>
    [StringLength(64)]
    public string? BankAccount { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    [StringLength(512)]
    public string? Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人
    /// </summary>
    [StringLength(50)]
    public string? CreateUser { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [StringLength(50)]
    public string? UpdateUser { get; set; }
}
