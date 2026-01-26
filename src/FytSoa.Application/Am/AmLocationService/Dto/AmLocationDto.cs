using System.ComponentModel.DataAnnotations;

namespace FytSoa.Application.Am;

/// <summary>
/// 地点 DTO
/// </summary>
public class AmLocationDto : AppEntity
{
    public long ParentId { get; set; } = 0;

    /// <summary>
    /// 父编号集合(JSON数组，如 [0,1,2])
    /// </summary>
    public List<long> ParentIdList { get; set; } = new();

    public int Layer { get; set; } = 1;

    [StringLength(64)]
    public string? Code { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [StringLength(32)]
    public string? Type { get; set; }

    public int Sort { get; set; } = 0;

    public bool Status { get; set; } = true;

    [StringLength(512)]
    public string? Summary { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string? CreateUser { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? UpdateUser { get; set; }
}

