using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Domain.User;

/// <summary>
/// 会员表
/// </summary>
[SugarTable("member")]
public class Member:Entity
{
    /// <summary>
    /// 会员组编号
    /// </summary>
    [Required]
    public long GroupId { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    [Required]
    [StringLength(90)]
    public string LoginName { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [Required]
    [StringLength(255)]
    public string LoginPwd { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required]
    [StringLength(90)]
    public string NickName { get; set; }
    
    /// <summary>
    /// 微信昵称
    /// </summary>
    public string WxName { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string Mobile { get; set; }
    
    /// <summary>
    /// 微信编号
    /// </summary>
    public string OpenId { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Sex { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public string Birthday { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    public string Autograph { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    [Required]
    public decimal Money { get; set; }

    /// <summary>
    /// 积分
    /// </summary>
    public int Point { get; set; } = 0;

    /// <summary>
    /// 删除状态
    /// </summary>
    [Required]
    public bool IsDel { get; set; } = false;

    /// <summary>
    /// 黑名单
    /// </summary>
    [Required]
    public bool IsBlacks { get; set; } = false;

    /// <summary>
    /// 状态
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    /// <summary>
    /// 抽奖次数
    /// </summary>
    public int PrizeDrawNum { get; set; } = 1;

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime? LoginTime { get; set; }

    /// <summary>
    /// 登录次数
    /// </summary>
    [Required]
    public int LoginSum { get; set; } = 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreateTime { get; set; }=DateTime.Now;

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