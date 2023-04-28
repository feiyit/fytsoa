using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Domain.Exam;

/// <summary>
/// 讲师
/// </summary>
[SugarTable("exam_teacher")]
public class ExamTeacher:Entity
{
    /// <summary>
    /// 专业编号
    /// </summary>
    [Required]
    public long ProfessionId { get; set; }
    
    /// <summary>
    /// 专业对象
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProfessionId))]
    public SysCode ProfessionCode { get; set; } 

    /// <summary>
    /// 教师姓名
    /// </summary>
    [Required]
    [StringLength(90)]
    public string Name { get; set; }

    /// <summary>
    /// 职称
    /// </summary>
    [Required]
    [StringLength(90)]
    public string PostName { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [Required]
    public int Age { get; set; } = 0;

    /// <summary>
    /// 头像
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Avatar { get; set; }
    
    /// <summary>
    /// 访问量
    /// </summary>
    [Required]
    public int Hits { get; set; } = 0;
    
    /// <summary>
    /// 被关注数
    /// </summary>
    [Required]
    public int Focus { get; set; } = 0;

    /// <summary>
    /// 介绍
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
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