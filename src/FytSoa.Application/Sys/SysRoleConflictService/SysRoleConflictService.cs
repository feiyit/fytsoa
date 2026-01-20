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
/// 角色互斥表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysRoleConflictService : IApplicationService 
{
    private readonly SugarRepository<SysRoleConflict> _thisRepository;
    public SysRoleConflictService(SugarRepository<SysRoleConflict> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysRoleConflictDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Summary.Contains(param.Key))
            .Includes(m=>m.RoleAObj)
            .Includes(m=>m.RoleBObj)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysRoleConflictDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysRoleConflictDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysRoleConflictDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysRoleConflictDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<SysRoleConflict>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysRoleConflictDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<SysRoleConflict>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
