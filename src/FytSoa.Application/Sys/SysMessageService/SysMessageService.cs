using FytSoa.Application.Operator;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 留言消息表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysMessageService : IApplicationService
{
    private readonly SugarRepository<SysMessage> _thisRepository;
    public SysMessageService(SugarRepository<SysMessage> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysMessageDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Where(m=>m.UserId==AppUtils.LoginId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Tags.Contains(param.Key) || m.Title.Contains(param.Key) || m.Summary.Contains(param.Key))
            .WhereIF(param.Status=="1", m => m.IsRead)
            .WhereIF(param.Status == "2", m => !m.IsRead)
            .WhereIF(param.Status == "3", m => m.IsDelete)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result = query.Adapt<PageResult<SysMessageDto>>();
        foreach (var item in result.Items)
        {
            item.TypesName = item.Types.ToString();
        }
        return result;
    }
    
    /// <summary>
    /// 查询消息的汇总
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<SysMessageTotalDto> GetTotalAsync()
    {
        var user = AppUtils.LoginId;
        var allCount = await _thisRepository.CountAsync (m => m.UserId==user);
        var unReadCount = await _thisRepository.CountAsync (m => !m.IsRead && m.UserId==user);
        var recycleCount = await _thisRepository.CountAsync (m => m.IsDelete && m.UserId==user);
        return new SysMessageTotalDto () {
            AllCount = allCount,
            UnReadCount = unReadCount,
            RecycleCount = recycleCount
        };
    }


    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysMessageDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysMessageDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysMessageDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<SysMessage>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysMessageDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<SysMessage>());
    }
    
    /// <summary>
    /// 设置已读
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task<bool> ReadAsync ([FromBody]List<long> ids)
    {
        return ids.Count>0 
            ? await _thisRepository.UpdateAsync (m => new SysMessage(){ IsRead = true}, m => ids.Contains (m.Id)) 
            : await _thisRepository.UpdateAsync (m => new SysMessage(){ IsRead = true}, m => true);
    }
    
    /// <summary>
    /// 删除到回收站
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> RecycleAsync (string ids) {
        return await _thisRepository.UpdateAsync (m =>new SysMessage(){ IsDelete = true}, m => ids.StrToListLong().Contains (m.Id));
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
}
