using FytSoa.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Operator;
/// <summary>
/// 工作台
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class WorkbenchService : IApplicationService
{
    /// <summary>
    /// 获得资源使情况
    /// </summary>
    /// <returns></returns>
    public DeviceUse GetResourceUse()
    {
        return DeviceUtils.GetInstance().GetSystemRateInfo();
    }

    /// <summary>
    /// 获得系统信息
    /// </summary>
    /// <returns></returns>
    public dynamic GetSystemInfo()
    {
        return DeviceUtils.GetInstance().GetSystemInfo();
    }
    
}