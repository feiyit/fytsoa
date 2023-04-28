using FytSoa.Domain.Cms;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Cms;

/// <summary>
/// 站点表 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v2")]
public class CmsSiteService : IApplicationService 
{
    private readonly SugarRepository<CmsSite> _thisRepository;
    public CmsSiteService(SugarRepository<CmsSite> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<CmsSiteDto>> GetListAsync()
    {
        var query = await _thisRepository.AsQueryable()
            .ToListAsync();
        return query.Adapt<List<CmsSiteDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsSiteDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<CmsSiteDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(CmsSiteDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<CmsSite>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsSiteDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<CmsSite>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
