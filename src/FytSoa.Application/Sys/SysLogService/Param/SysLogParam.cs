using FytSoa.Common.Enum;
using FytSoa.Common.Param;

namespace FytSoa.Application.Sys;

public class SysLogParam:PageParam
{
    public int Level { get; set; } = 0;

    public int Type { get; set; } = 0;
}