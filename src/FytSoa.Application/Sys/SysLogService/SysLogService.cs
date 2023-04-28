using DotNetCore.CAP;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Common.Result;
using FytSoa.DynamicApi.Attributes;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 登录日志/操作日志/任务日志服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1"),NoAuditLog]
public class SysLogService : IApplicationService ,ICapSubscribe
{
    private readonly SugarRepository<SysLog> _thisRepository;
    public SysLogService(SugarRepository<SysLog> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysLogDto>> GetPagesAsync(SysLogParam param)
    {
        string btime = string.Empty, etime = string.Empty;
        if (!string.IsNullOrEmpty(param.Times))
        {
            (btime, etime) = TimeUtils.Splitting(param.Times);
        }
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Times),m=>SqlFunc.Between(m.OperateTime,btime,etime))
            .WhereIF(param.Level!=0,m => m.Level == (LogEnum)param.Level)
            .WhereIF(param.Type!=0,m => m.LogType == (LogTypeEnum)param.Type)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result = query.Adapt<PageResult<SysLogDto>>();
        foreach (var item in result.Items)
        {
            item.LevelName = item.Level.ToString();
        }
        return result;
    }

    /// <summary>
    /// 查询根据日志级别查询图表信息
    /// </summary>
    /// <returns></returns>
    public async Task<SysLogChartDto> GetChartAsync()
    {
        var btime = DateTime.Now.AddDays(-14);
        var list = await _thisRepository.Context.Queryable<SysLog>()
            .Where(m=>SqlFunc.Between(m.OperateTime,btime,DateTime.Now))
            .Select(m => new
            {
                m.Id,
                m.Level,
                Time = m.OperateTime.Date
            })
            .MergeTable()
            .GroupBy(m => new {m.Time,m.Level})
            .Select(m => new
            {
                m.Time,
                m.Level,
                Count = SqlFunc.AggregateCount(m.Id)
            })
            .ToListAsync();
        var res = new SysLogChartDto();
        var debug = new List<int>();
        var info = new List<int>();
        var warn = new List<int>();
        var error = new List<int>();
        var fatal = new List<int>();
        for (var i = 0; i < 15; i++)
        {
            var time = DateTime.Now.AddDays(value: -(14 - i));
            res.Time.Add(time.ToShortDateString());
            debug.Add(list.FirstOrDefault(m => m.Level == LogEnum.Debug && m.Time.Date==time.Date)==null ? 0 : 
                list.FirstOrDefault(m => m.Level == LogEnum.Debug && m.Time.Date==time.Date)!.Count);
            info.Add(list.FirstOrDefault(m => m.Level == LogEnum.Info && m.Time.Date==time.Date)==null ? 0 : 
                list.FirstOrDefault(m => m.Level == LogEnum.Info && m.Time.Date==time.Date)!.Count);
            warn.Add(list.FirstOrDefault(m => m.Level == LogEnum.Warn && m.Time.Date==time.Date)==null ? 0 : 
                list.FirstOrDefault(m => m.Level == LogEnum.Warn && m.Time.Date==time.Date)!.Count);
            error.Add(list.FirstOrDefault(m => m.Level == LogEnum.Error && m.Time.Date==time.Date)==null ? 0 : 
                list.FirstOrDefault(m => m.Level == LogEnum.Error && m.Time.Date==time.Date)!.Count);
            fatal.Add(list.FirstOrDefault(m => m.Level == LogEnum.Fatal && m.Time.Date==time.Date)==null ? 0 : 
                list.FirstOrDefault(m => m.Level == LogEnum.Fatal && m.Time.Date==time.Date)!.Count);
        }
        res.Count.Add(debug);
        res.Count.Add(info);
        res.Count.Add(error);
        res.Count.Add(warn);
        res.Count.Add(fatal);
        return res;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysLogDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysLogDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysLogDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<SysLog>());
    }
    
    

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysLogDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<SysLog>());
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync(string ids)
    {
        return await _thisRepository.DeleteAsync(m=>ids.StrToListLong().Contains(m.Id));
    }
    
    /// <summary>
    /// 清空日志
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> ClearAsync()
    {
        return await _thisRepository.DeleteAsync(m=>true);
    }
}
