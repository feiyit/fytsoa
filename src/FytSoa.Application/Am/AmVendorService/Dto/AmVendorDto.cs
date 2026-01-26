using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 供应商 DTO
/// </summary>
public class AmVendorDto : AppEntity
{
    [StringLength(64)]
    public string? Code { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ContactName { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(64)]
    public string? TaxNo { get; set; }

    [StringLength(100)]
    public string? BankName { get; set; }

    [StringLength(64)]
    public string? BankAccount { get; set; }

    public bool Status { get; set; } = true;

    [StringLength(512)]
    public string? Summary { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
}

