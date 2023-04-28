using FytSoa.Domain.Shop;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Shop;

/// <summary>
/// 积分商城-商品表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v4")]
public class ShopPointsGoodsService : IApplicationService 
{
    private readonly SugarRepository<ShopPointsGoods> _thisRepository;
    public ShopPointsGoodsService(SugarRepository<ShopPointsGoods> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ShopPointsGoodsDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ShopPointsGoodsDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ShopPointsGoodsDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ShopPointsGoodsDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ShopPointsGoodsDto model)
    {
        model.Number = StringUtils.RandomString(6);
        return await _thisRepository.InsertAsync(model.Adapt<ShopPointsGoods>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ShopPointsGoodsDto model) => 
        await _thisRepository.UpdateAsync(model.Adapt<ShopPointsGoods>());
    
    /// <summary>
    /// 处理查看状态
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ModifyStatusAsync(List<long> ids)
    {
        var list = await _thisRepository.GetListAsync(m => ids.Contains(m.Id));
        foreach (var item in list)
        {
            item.Status = !item.Status;
        }
        return await _thisRepository.UpdateRangeAsync(list);
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
