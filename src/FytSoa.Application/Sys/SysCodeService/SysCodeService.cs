using FytSoa.Application.Sys.Param;
using FytSoa.Common.Extensions;
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
/// 字典信息表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysCodeService : IApplicationService 
{
    private readonly SugarRepository<SysCode> _thisRepository;
    public SysCodeService(SugarRepository<SysCode> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysCodeDto>> GetPagesAsync(CodePageParam param)
    {
        if (!string.IsNullOrEmpty(param.TypeCode))
        {
            var typeRepository = _thisRepository.ChangeRepository<SugarRepository<SysCodetype>>();
            var typeModel = await typeRepository.GetSingleAsync(m=>m.Code==param.TypeCode);
            if (typeModel!=null)
            {
                param.Id = typeModel.Id;
            }
        }
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .WhereIF(param.Id!=0,m => m.TypeId == param.Id)
            .WhereIF(param.Type!=0,m=>m.Tag==param.Type)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysCodeDto>>();
    }
    
    /// <summary>
    /// 查询所有——所有
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<SysCodeDto>> GetListAsync(CodePageParam param)
    {
        if (!string.IsNullOrEmpty(param.TypeCode))
        {
            var typeRepository = _thisRepository.ChangeRepository<SugarRepository<SysCodetype>>();
            var typeModel = await typeRepository.AsQueryable()
                .WhereIF(param.Type!=0,m=>m.Types==param.Type)
                .FirstAsync(m=>m.Code==param.TypeCode);
            if (typeModel!=null)
            {
                param.Id = typeModel.Id;
            }
        }
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .WhereIF(param.Id!=0,m => m.TypeId == param.Id)
            .WhereIF(param.Type!=0,m=>m.Tag==param.Type)
            .ToListAsync();
        return query.Adapt<List<SysCodeDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysCodeDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysCodeDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task AddAsync(SysCodeDto model)
    {
        var isAny = await _thisRepository.IsAnyAsync(m => m.TypeId == model.TypeId && m.Name == model.Name);
        if (isAny)
        {
            throw new BusinessException("名称不能重复~");
        }
        await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<SysCode>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(SysCodeDto model)
    {
        var isAny = await _thisRepository.IsAnyAsync(m => m.TypeId == model.TypeId && m.Name == model.Name && m.Id!=model.Id);
        if (isAny)
        {
            throw new BusinessException("名称不能重复~");
        }
        return await _thisRepository.UpdateAsync(model.Adapt<SysCode>());
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
