using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using FytSoa.Application.Sys;
using FytSoa.Domain.Exam;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 课程
/// </summary>
public class ExamCourseDto : AppEntity
{
    /// <summary>
    /// 课程类型(直播、点播、图文)
    /// </summary>
    [Required]
    public int Type { get; set; } = 1;

    /// <summary>
    /// 讲师编号
    /// </summary>
    [Required]
    public long TeacherId { get; set; }
    
    /// <summary>
    /// 讲师
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(TeacherId))]
    public ExamTeacherDto Teacher { get; set; }

    /// <summary>
    /// 年级编号(多选)
    /// </summary>
    [Required]
    public List<string> GradeId { get; set; }
    
    /// <summary>
    /// 年级具体内容
    /// </summary>
    public string GradeNames { get; set; }

    /// <summary>
    /// 学科编号(多选)
    /// </summary>
    [Required]
    public List<string> SubjectId { get; set; }
    
    /// <summary>
    /// 学科具体内容
    /// </summary>
    public string SubjectNames { get; set; }

    /// <summary>
    /// 难度编号
    /// </summary>
    [Required]
    public long DifficultyId { get; set; }
    
    /// <summary>
    /// 难度
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DifficultyId))]
    public SysCodeDto Difficulty { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 直播地址
    /// </summary>
    public string Urls { get; set; }

    /// <summary>
    /// 课程列表Json(名称+视频+打点)
    /// </summary>
    public List<ExamCourse.ExamCourses> Courses { get; set; } = new();

    /// <summary>
    /// 封面
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Cover { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    [Required]
    public bool Audit { get; set; } = false;

    /// <summary>
    /// 上架状态(立即、定时、暂不)
    /// </summary>
    [Required]
    public int Status { get; set; } = 1;

    /// <summary>
    /// 定时时间
    /// </summary>
    public DateTime? Timing { get; set; }

    /// <summary>
    /// 点击量
    /// </summary>
    [Required]
    public int Hits { get; set; } = 0;
    
    /// <summary>
    /// 课程支持状态
    /// </summary>
    public ExamCourse.ExamCourseSupport Support { get; set; } = new();

    /// <summary>
    /// 属性数组(评论、互动、推荐、弹幕、下载、投票)
    /// </summary>
    public List<int> Attr { get; set; } = new();

    /// <summary>
    /// 打点Json(位置+提示语)
    /// </summary>
    public string Dot { get; set; }

    /// <summary>
    /// 课件(名称、排序、文件)
    /// </summary>
    public List<ExamCourse.ExamCourseware> Courseware { get; set; } = new();

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public bool IsDelete { get; set; } = false;

    /// <summary>
    /// 视频描述
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 视频内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.Now;

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