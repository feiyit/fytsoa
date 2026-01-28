using FytSoa.Common.Scheduler.Models;

namespace FytSoa.Common.Scheduler.Services;

public interface IFytSchedulerLogService
{
    Task<ResultData<QuartzTaskLog>> GetLogs(string taskName, string groupName, int page, int pageSize);
    Task<QuartzTaskLog?> GetLastLog(string taskName, string groupName);
}

