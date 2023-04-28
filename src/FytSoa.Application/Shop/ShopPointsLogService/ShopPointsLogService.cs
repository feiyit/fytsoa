using FytSoa.Domain.Shop;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Shop;

/// <summary>
/// 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v4")]
public class ShopPointsLogService : IApplicationService
{
    private readonly SugarRepository<ShopPointsLog> _thisRepository;
    public ShopPointsLogService(SugarRepository<ShopPointsLog> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ShopPointsLogDto>> GetPagesAsync(PageParam param)
    {
        var where = Expressionable.Create<ShopPointsLog>();
        if (!string.IsNullOrEmpty(param.Status))
        {
            where.And(m => m.Flag == int.Parse(param.Status));
        }
        if (param.Id!=0)
        {
            where.And(m => m.UserId == param.Id);
        }
        var query = await _thisRepository.AsQueryable()
            .Where(where.ToExpression())
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result = query.Adapt<PageResult<ShopPointsLogDto>>();
        foreach (var item in result.Items)
        {
            item.GetTypeName = item.GetType.ToDescription();
        }
        return result;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ShopPointsLogDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ShopPointsLogDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ShopPointsLogDto model) => 
        await _thisRepository.InsertAsync(model.Adapt<ShopPointsLog>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ShopPointsLogDto model) => 
        await _thisRepository.UpdateAsync(model.Adapt<ShopPointsLog>());

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
