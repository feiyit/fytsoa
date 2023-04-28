using System;
using FytSoa.Common.Utils;
using System.ComponentModel.DataAnnotations;
using FytSoa.Domain.Sys;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 日程表
/// </summary>
public class SysCalendarDto : AppEntity
{
    /// <summary>
    /// 日程标题
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// 日程类型
    /// </summary>
    [Required]
    public long TypeId { get; set; } = 0;
    
    /// <summary>
    /// 日程类型信息
    /// </summary>
    public SysCodeDto TypeCode { get; set; } 
    
    /// <summary>
    /// 日程等级
    /// </summary>
    [Required]
    public long LevelId { get; set; } = 0;
    
    /// <summary>
    /// 日程等级信息
    /// </summary>
    public SysCodeDto LevelCode { get; set; } 

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required]
    public DateTime EndTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 参与人列表
    /// </summary>
    public List<SysCalendar.CalendarUser> UserIds { get; set; } = new();

    /// <summary>
    /// 关联业务
    /// </summary>
    public List<SysCalendar.CalendarToBusiness> ToBusiness { get; set; } = new();

    /// <summary>
    /// 提醒设置
    /// </summary>
    public SysCalendar.CalendarRemind Remind { get; set; } = new();

    /// <summary>
    /// 参与人详细信息
    /// </summary>
    public List<SysAdminDto> User { get; set; }

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