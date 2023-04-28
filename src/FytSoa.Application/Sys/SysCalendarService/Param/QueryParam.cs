using FytSoa.Common.Param;

namespace FytSoa.Application.Sys.Param;

public class CalendarQueryParam:CommonParam
{
    /// <summary>
    /// 根据类型查询
    /// </summary>
    public long TypeId { get; set; } = 0;
    
    /// <summary>
    /// 根据等级查询
    /// </summary>
    public long LevelId { get; set; } = 0;

    /// <summary>
    /// 根据日期查询
    /// </summary>
    public DateTime? ToDay { get; set; } = null;
}