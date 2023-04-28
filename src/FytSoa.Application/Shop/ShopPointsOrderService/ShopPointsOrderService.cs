using FytSoa.Domain.Shop;
using FytSoa.Sugar;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.User;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Shop;

/// <summary>
/// 积分商城-订单表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v4")]
public class ShopPointsOrderService : IApplicationService
{
    private readonly SugarRepository<ShopPointsOrder> _thisRepository;
    public ShopPointsOrderService(SugarRepository<ShopPointsOrder> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ShopPointsOrderDto>> GetPagesAsync(PageParam param)
    {
        var where = Expressionable.Create<ShopPointsOrder>();
        if (!string.IsNullOrEmpty(param.Status))
        {
            where.And(m => m.Status == int.Parse(param.Status));
        }
        if (param.Id!=0)
        {
            where.And(m => m.UserId == param.Id);
        }
        var query = await _thisRepository.AsQueryable()
            .Where(where.ToExpression())
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result = query.Adapt<PageResult<ShopPointsOrderDto>>();
        var userIds = result.Items.Select(m => m.UserId).ToList();
        var userList = new List<Member>();
        if (userIds.Count>0)
        {
            var userRepository = _thisRepository.ChangeRepository<SugarRepository<Member>>();
            userList = await userRepository.GetListAsync(m => userIds.Contains(m.Id));
        }
        var goodsIds = result.Items.Select(m => m.GoodsId).ToList();
        var goodsList = new List<ShopPointsGoods>();
        if (goodsIds.Count>0)
        {
            var prizeRepository = _thisRepository.ChangeRepository<SugarRepository<ShopPointsGoods>>();
            goodsList = await prizeRepository.GetListAsync(m => goodsIds.Contains(m.Id));
        }
        foreach (var item in result.Items)
        {
            item.User = userList.FirstOrDefault(m => m.Id == item.UserId) ?? new Member();
            item.Goods = goodsList.FirstOrDefault(m => m.Id == item.GoodsId) ?? new ShopPointsGoods();
        }
        return result;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ShopPointsOrderDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ShopPointsOrderDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ShopPointsOrderDto model) => 
        await _thisRepository.InsertAsync(model.Adapt<ShopPointsOrder>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ShopPointsOrderDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ShopPointsOrder>());
    
    /// <summary>
    /// 处理查看状态
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ModifyStatusAsync(List<long> ids)
    {
        return await _thisRepository.UpdateAsync(m=>new ShopPointsOrder()
        {
            Status = (int)ShopPointEnum.Completed
        },m=>ids.Contains(m.Id));
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
