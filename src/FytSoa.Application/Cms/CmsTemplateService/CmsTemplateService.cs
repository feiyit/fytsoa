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
/// 模板表 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v2")]
public class CmsTemplateService : IApplicationService 
{
    private readonly SugarRepository<CmsTemplate> _thisRepository;
    public CmsTemplateService(SugarRepository<CmsTemplate> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<CmsTemplateDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<CmsTemplateDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsTemplateDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<CmsTemplateDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(CmsTemplateDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<CmsTemplate>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsTemplateDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<CmsTemplate>());
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
