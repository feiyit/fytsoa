using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 素材
/// </summary>
public class ExamMaterialDto : AppEntity
{
    /// <summary>
    /// 分类编号
    /// </summary>
    [Required]
    public long CategoryId { get; set; }
    
    /// <summary>
    /// 分类信息
    /// </summary>
    public ExamMaterialCategoryDto Category { get; set; } 

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 原名称
    /// </summary>
    [Required]
    [StringLength(255)]
    public string SourceName { get; set; }

    /// <summary>
    /// 扩展名
    /// </summary>
    [Required]
    [StringLength(90)]
    public string Ext { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [Required]
    public int Size { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Urls { get; set; }

    /// <summary>
    /// 视频(封面)
    /// </summary>
    public string Cover { get; set; }

    /// <summary>
    /// 文件模式(上传Upload/转换Convert/剪辑Clip/合并/Merge)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 审核
    /// </summary>
    [Required]
    public bool Audit { get; set; } = false;

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