using FytSoa.Application.Sys.Param;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 广告位信息表 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysAdvInfoService : IApplicationService
{
    private readonly SugarRepository<SysAdvInfo> _thisRepository;
    private readonly SugarRepository<SysAdvColumn> _advColumnRepository;
    public SysAdvInfoService(SugarRepository<SysAdvInfo> thisRepository
    ,SugarRepository<SysAdvColumn> advColumnRepository)
    {
        _thisRepository = thisRepository;
        _advColumnRepository = advColumnRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysAdvInfoDto>> GetPagesAsync(CodePageParam param)
    {
        if (!string.IsNullOrEmpty(param.TypeCode))
        {
            var typeModel = await _advColumnRepository.GetSingleAsync(m=>m.Flag==param.TypeCode);
            if (typeModel!=null)
            {
                param.Id = typeModel.Id;
            }
        }
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.Id!=0,m => m.ColumnId == param.Id)
            .WhereIF(param.Status=="1",m=>m.Status)
            .WhereIF(param.Status=="2",m=>!m.Status)
            .OrderBy("sort desc,id desc")
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysAdvInfoDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysAdvInfoDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysAdvInfoDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysAdvInfoDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<SysAdvInfo>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysAdvInfoDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<SysAdvInfo>());
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids)
    {
        return await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
    }
}
