using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.Cms;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Cms;

/// <summary>
/// 产品表 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v2")]
public class CmsProductService : IApplicationService
{
    private readonly SugarRepository<CmsProduct> _thisRepository;
    private readonly CmsColumnService _columnService;

    public CmsProductService(
        SugarRepository<CmsProduct> thisRepository,
        CmsColumnService columnService)
    {
        _thisRepository = thisRepository;
        _columnService = columnService;
    }

    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<PageResult<CmsProductDto>> PagesAsync([FromBody] CmsProductParam param)
    {
        var where = Expressionable.Create<CmsProduct>();

        // 关键字查询：名称、编号、简介、内容
        if (!string.IsNullOrEmpty(param.Key))
        {
            where.And(m =>
                m.Title.Contains(param.Key) ||
                m.ProductNo.Contains(param.Key) ||
                m.Intro.Contains(param.Key) ||
                m.Content.Contains(param.Key));
        }

        // 栏目筛选：单个栏目 Id
        if (param.Id != 0)
        {
            where.And(m => m.ColumnId == param.Id);
        }

        // 栏目筛选：多个栏目 Id 集合
        if (param.ColumnList.Count > 0)
        {
            where.And(m => param.ColumnList.Contains(m.ColumnId));
        }

        // 栏目英文标识筛选
        if (!string.IsNullOrEmpty(param.EnTitle))
        {
            where.And(m =>
                SqlFunc.Subqueryable<CmsColumn>()
                    .Where(s => s.EnTitle == param.EnTitle && s.Id == m.ColumnId)
                    .Any());
        }

        // 上下架状态筛选
        switch (param.Status)
        {
            case "1":
                where.And(m => m.Status);
                break;
            case "2":
                where.And(m => !m.Status);
                break;
        }

        // 价格区间筛选
        if (param.MinPrice.HasValue)
        {
            where.And(m => m.Price >= param.MinPrice.Value);
        }

        if (param.MaxPrice.HasValue)
        {
            where.And(m => m.Price <= param.MaxPrice.Value);
        }

        var query = _thisRepository.AsQueryable()
            .Where(where.ToExpression());

        // 高级条件查询（Json）
        if (!string.IsNullOrEmpty(param.Query))
        {
            var jsonParam = _thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            query = query.Where(jsonParam);
        }

        var pageQuery = await query
            .OrderBy(m => new { m.Sort, m.PublishTime, m.Id }, OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);

        // 补充栏目名称
        var columnList = await _columnService.GetListAsync(new WhereParam());
        var res = pageQuery.Adapt<PageResult<CmsProductDto>>();
        foreach (var item in res.Items)
        {
            var columnModel = columnList.FirstOrDefault(m => m.Id == item.ColumnId);
            if (columnModel == null) continue;
            item.ColumnName = columnModel.Title;
        }

        return res;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsProductDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<CmsProductDto>();
    }

    /// <summary>
    /// 根据主键查询并更新点击量
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CmsProductDto> GetByIdAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        if (model == null)
        {
            return null;
        }

        model.Hits += 1;
        var currentTime = DateTime.Now;
        var lastHit = model.LastHitDate;

        // 日点击量
        if (!lastHit.HasValue || lastHit.Value.Date != currentTime.Date)
        {
            model.DayHits = 1;
        }
        else
        {
            model.DayHits += 1;
        }

        // 周点击量
        var currentWeekStart = GetWeekStartDate(currentTime);
        var lastWeekStart = lastHit.HasValue ? GetWeekStartDate(lastHit.Value) : DateTime.MinValue;
        if (lastWeekStart != currentWeekStart)
        {
            model.WeedHits = 1;
        }
        else
        {
            model.WeedHits += 1;
        }

        // 月点击量
        if (!lastHit.HasValue || lastHit.Value.Month != currentTime.Month || lastHit.Value.Year != currentTime.Year)
        {
            model.MonthHits = 1;
        }
        else
        {
            model.MonthHits += 1;
        }

        model.LastHitDate = currentTime;

        await _thisRepository.UpdateAsync(model);

        return model.Adapt<CmsProductDto>();
    }

    private DateTime GetWeekStartDate(DateTime date)
    {
        // 以周一为每周第一天
        var diff = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(CmsProductDto model)
    {
        model.CreateTime=DateTime.Now;
        return await _thisRepository.InsertAsync(model.Adapt<CmsProduct>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsProductDto model)
    {
        model.UpdateTime=DateTime.Now;
        return await _thisRepository.UpdateAsync(model.Adapt<CmsProduct>());
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">Id 集合</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody] List<long> ids)
    {
        return await _thisRepository.DeleteAsync(m => ids.Contains(m.Id));
    }
}

