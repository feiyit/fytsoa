using FytSoa.Application.Cms.Dto;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.Cms;
using FytSoa.Sugar;
using Mapster;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Cms;

[ApiExplorerSettings(GroupName = "v2")]
public class CmsVariateService : IApplicationService 
{
    private readonly SugarRepository<CmsVariate> _thisRepository;
    public CmsVariateService(SugarRepository<CmsVariate> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<CmsVariateDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .ToPageAsync(param.Page, param.Limit);
        var res=query.Adapt<PageResult<CmsVariateDto>>();
        foreach (var item in res.Items)
        {
            item.FieldTypeName = item.FieldType.GetDescription();
        }
        return res;
    }

    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <returns></returns>
    public async Task<List<CmsVariateDto>> GetListAsync()
    {
        var query = await _thisRepository.AsQueryable()
            .OrderBy(m=>new{m.Id},OrderByType.Asc)
            .ToListAsync();
        var res=query.Adapt<List<CmsVariateDto>>();
        foreach (var item in res)
        {
            item.FieldTypeName = item.FieldType.GetDescription();
        }
        return res;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsVariateDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<CmsVariateDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(CmsVariateDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<CmsVariate>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsVariateDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<CmsVariate>());
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