using FytSoa.Quartz.Enum;
using FytSoa.Quartz.Model;
using FytSoa.Quartz.Service;
using FytSoa.Quartz.Tools;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;

/// <summary>
/// 任务调度
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysQuartzService : IApplicationService
{
    
    private readonly IQuartzHandle _quartzHandle;
    private readonly IQuartzLogService _logService;
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="quartzHandle"></param>
    /// <param name="logService"></param>
    public SysQuartzService(IQuartzHandle quartzHandle
        , IQuartzLogService logService)
    {
        _quartzHandle = quartzHandle;
        _logService = logService;
    }
    
    /// <summary>
    /// 获取任务列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<QuarzTask>> GetAsync() => 
        await _quartzHandle.GetJobs();
    
    /// <summary>
    /// 新建任务
    /// </summary>
    /// <returns></returns>
    public async Task<ResultQuartzData> AddAsync([FromBody]QuarzTask model)
    {
        var date = await  _quartzHandle.AddJob(model);
        model.Status = Convert.ToInt32(JobState.暂停);
        return date;
    }
    
    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutPauseJob([FromBody]QuarzTask model) =>
        await _quartzHandle.Pause(model);
    
    /// <summary>
    /// 开启任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutStartJob([FromBody]QuarzTask model) => 
        await _quartzHandle.Start(model);
    
    /// <summary>
    /// 立即执行任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> PutRunJob([FromBody]QuarzTask model) => 
        await _quartzHandle.Run(model);
    
    /// <summary>
    /// 修改任务
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultQuartzData> Put([FromBody]QuarzTask model) => 
        await _quartzHandle.Update(model);
    
    /// <summary>
    /// 删除任务
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ResultQuartzData> Delete([FromBody]QuarzTask model) => 
        await _quartzHandle.Remove(model);
    
    /// <summary>
    /// 获取任务执行记录
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultData<QuarzTasklog>> JobRecord(string taskName, string groupName, int current, int size) => 
        await _logService.GetLogs(taskName,groupName, current, size);
}