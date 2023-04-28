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
/// 投票项服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysVoteItemService : IApplicationService 
{
    private readonly SugarRepository<SysVoteItem> _thisRepository;
    public SysVoteItemService(SugarRepository<SysVoteItem> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysVoteItemDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Limit, param.Page);
        return query.Adapt<PageResult<SysVoteItemDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysVoteItemDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysVoteItemDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysVoteItemDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<SysVoteItem>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysVoteItemDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<SysVoteItem>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
