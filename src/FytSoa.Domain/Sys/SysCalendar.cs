using System.ComponentModel.DataAnnotations;
using FytSoa.Common.Enum;
using SqlSugar;

namespace FytSoa.Domain.Sys;

/// <summary>
/// 日程表
/// </summary>
[SugarTable("sys_calendar")]
public class SysCalendar:EntityBase
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
    [Navigate(NavigateType.OneToOne, nameof(TypeId))]
    public SysCode TypeCode { get; set; } 

    /// <summary>
    /// 日程等级
    /// </summary>
    [Required]
    public long LevelId { get; set; } = 0;
    
    /// <summary>
    /// 日程等级信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LevelId))]
    public SysCode LevelCode { get; set; } 

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
    [SugarColumn(IsJson = true)]
    public List<CalendarUser> UserIds { get; set; } = new();

    /// <summary>
    /// 关联业务
    /// </summary>
    [SugarColumn(IsJson = true)]
    public List<CalendarToBusiness> ToBusiness { get; set; } = new();

    /// <summary>
    /// 提醒设置
    /// </summary>
    [SugarColumn(IsJson = true)]
    public CalendarRemind Remind { get; set; } = new();
    
    /// <summary>
    /// 参与人列表
    /// </summary>
    public class CalendarUser
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 姓名
        /// </summary>
        public string FullName { get; set; }
    }

    /// <summary>
    /// 日程关联业务对象
    /// </summary>
    public class CalendarToBusiness
    {
        /// <summary>
        /// 项目类型
        /// </summary>
        public CalendarProjectEnum ProjectType { get; set; }

        /// <summary>
        /// 项目唯一编号
        /// </summary>
        public List<long> ProjectIds { get; set; }
    }
    
    /// <summary>
    /// 提醒对象
    /// </summary>
    public class CalendarRemind
    {
        /// <summary>
        /// 提前数据
        /// </summary>
        public int Number { get; set; }
        
        /// <summary>
        /// 时间类型
        /// </summary>
        public CalendarTimeEnum TimeType { get; set; }
    }
}