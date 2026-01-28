using FytSoa.Common.Scheduler.Models;
using FytSoa.Common.Scheduler.Services;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;

/// <summary>
/// 任务调度
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysQuartzService : IApplicationService
{
    
    private readonly IFytSchedulerService _scheduler;
    private readonly IFytSchedulerLogService _logService;
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="scheduler"></param>
    /// <param name="logService"></param>
    public SysQuartzService(IFytSchedulerService scheduler
        , IFytSchedulerLogService logService)
    {
        _scheduler = scheduler;
        _logService = logService;
    }
    
    /// <summary>
    /// 获取任务列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<QuartzTask>> GetAsync() => 
        await _scheduler.GetJobs();
    
    /// <summary>
    /// 新建任务
    /// </summary>
    /// <returns></returns>
    public async Task<ResultQuartzData> AddAsync([FromBody]QuartzTask model)
    {
        var res = await _scheduler.AddJob(model);
        model.Status = Convert.ToInt32(JobState.暂停);
        return res;
    }
    
    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutPauseJob([FromBody]QuartzTask model) =>
        await _scheduler.Pause(model);
    
    /// <summary>
    /// 开启任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutStartJob([FromBody]QuartzTask model) => 
        await _scheduler.Start(model);
    
    /// <summary>
    /// 立即执行任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutRunJob([FromBody]QuartzTask model) => 
        await _scheduler.Run(model);
    
    /// <summary>
    /// 修改任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> Put([FromBody]QuartzTask model) => 
        await _scheduler.Update(model);
    
    /// <summary>
    /// 删除任务
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ResultQuartzData> Delete([FromBody]QuartzTask model) => 
        await _scheduler.Remove(model);
    
    /// <summary>
    /// 获取任务执行记录
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultData<QuartzTaskLog>> JobRecord(string taskName, string groupName, int current, int size) => 
        await _logService.GetLogs(taskName,groupName, current, size);
}
