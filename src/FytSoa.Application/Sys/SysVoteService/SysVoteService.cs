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
/// 投票表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysVoteService : IApplicationService 
{
    private readonly SugarRepository<SysVote> _thisRepository;
    public SysVoteService(SugarRepository<SysVote> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysVoteDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Includes(m=>m.Items)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysVoteDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysVoteDto> GetAsync(long id)
    {
        var model = await _thisRepository.AsQueryable().Includes(m=>m.Items).SingleAsync(m=>m.Id==id);
        return model.Adapt<SysVoteDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [UnitOfWork]
    public async Task AddAsync(SysVoteDto model)
    {
        var voteId= await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<SysVote>());
        if (model.Items.Count>0)
        {
            foreach (var item in model.Items)
            {
                item.VoteId = voteId;
            }
            var voteItemRepository= _thisRepository.ChangeRepository<SugarRepository<SysVoteItem>>();
            await voteItemRepository.InsertRangeAsync(model.Items.Adapt<List<SysVoteItem>>());
        }
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [UnitOfWork]
    public async Task ModifyAsync(SysVoteDto model)
    {
        var vote = model.Adapt<SysVote>();
        var entity = await _thisRepository.AsQueryable().Includes(m=>m.Items).SingleAsync(m=>m.Id==model.Id);
        await _thisRepository.UpdateAsync(vote);
        
        var voteItemRepository= _thisRepository.ChangeRepository<SugarRepository<SysVoteItem>>();
        var upList = vote.Items.Where(m => entity.Items.Select(s => s.Id).Contains(m.Id)).ToList();
        if (upList.Any())
        {
            await voteItemRepository.UpdateRangeAsync(upList);
        }
        var delList = entity.Items.Where(m=>!vote.Items.Where(s=> s.Id!=0).Select(s=>s.Id).Contains(m.Id)).ToList();
        var addList = vote.Items.Where(m=>m.Id==0).ToList();
        if (addList.Any())
        {
            foreach (var item in addList)
            {
                item.VoteId = vote.Id;
            }
            await voteItemRepository.InsertRangeAsync(addList);
        }
        if (delList.Any())
        {
            await voteItemRepository.DeleteAsync(delList);
        }
    }

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete,UnitOfWork]
    public async Task DeleteAsync([FromBody] List<long> ids)
    {
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
        var voteItemRepository= _thisRepository.ChangeRepository<SugarRepository<SysVoteItem>>();
        await voteItemRepository.DeleteAsync(m => ids.Contains(m.VoteId));
    }
}
