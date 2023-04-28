using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.User;

/// <summary>
/// 会员组
/// </summary>
[SugarTable("member_group")]
public class MemberGroup:Entity
{
    /// <summary>
    /// 会员等级;关联数据字典
    /// </summary>
    [Required]
    public long LevelId { get; set; }

    /// <summary>
    /// 组名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// 组图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 升级积分
    /// </summary>
    [Required]
    public int UpPoint { get; set; } = 0;

    /// <summary>
    /// 升级金额
    /// </summary>
    [Required]
    public decimal UpMoney { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = false;

    /// <summary>
    /// 描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string UpdateUser { get; set; }


}