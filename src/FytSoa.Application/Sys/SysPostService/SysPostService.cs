using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 岗位表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysPostService : IApplicationService
{
    private readonly SugarRepository<SysPost> _thisRepository;
    public SysPostService(SugarRepository<SysPost> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysPostDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.TenantId!=0,m=>m.TenantId==param.TenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysPostDto>>();
    }


    /// <summary>
    /// 查询所有—
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysPostDto>> GetAllAsync()
    {
        var list = await _thisRepository.AsQueryable()
            .OrderBy(m=>m.Id,OrderByType.Desc).ToListAsync();
        return list.Adapt<List<SysPostDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysPostDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysPostDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysPostDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<SysPost>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysPostDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<SysPost>());
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
