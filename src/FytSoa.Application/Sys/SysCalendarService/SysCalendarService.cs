using FytSoa.Application.Sys.Param;
using FytSoa.Application.User;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.User;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 日程表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysCalendarService : IApplicationService 
{
    private readonly SugarRepository<SysCalendar> _thisRepository;
    public SysCalendarService(SugarRepository<SysCalendar> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<SysCalendarDto>> GetListAsync(CalendarQueryParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.TypeId!=0,m=>m.TypeId==param.TypeId)
            .WhereIF(param.LevelId!=0,m=>m.LevelId==param.LevelId)
            .WhereIF(param.Id!=0,m=>SqlFunc.ToString(m.UserIds).Contains(param.Id.ToString()))
            .WhereIF(param.ToDay!=null,m=>m.StartTime.Year==param.ToDay.Value.Year && m.StartTime.Month==param.ToDay.Value.Month)
            .Includes(m=>m.TypeCode)
            .Includes(m=>m.LevelCode)
            .OrderBy(m=>m.Id,OrderByType.Desc)
            .ToListAsync();
        var res=query.Adapt<List<SysCalendarDto>>();
        if (res.Count <= 0) return res;
        {
            var userRepository = _thisRepository.ChangeRepository<SugarRepository<SysAdmin>>();
            var userId = (from item in query from row in item.UserIds select row.Id).ToList();
            var userList = await userRepository.GetListAsync(m => userId.Contains(m.Id));
            foreach (var item in res)
            {
                var userIdArr = item.UserIds.Select(m => m.Id).ToList();
                item.User = userList.Where(m => userIdArr.Contains(m.Id)).Adapt<List<SysAdminDto>>();
            }
        }
        return res;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysCalendarDto> GetAsync(long id)
    {
        var model = await _thisRepository
            .AsQueryable()
            .Includes(m=>m.TypeCode)
            .Includes(m=>m.LevelCode)
            .FirstAsync(m=>m.Id==id);
        return model.Adapt<SysCalendarDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysCalendarDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<SysCalendar>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysCalendarDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<SysCalendar>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
