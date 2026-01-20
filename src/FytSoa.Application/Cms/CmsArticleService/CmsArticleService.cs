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
/// 文章表 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v2")]
public class CmsArticleService : IApplicationService
{
    private readonly SugarRepository<CmsArticle> _thisRepository;
    private readonly CmsColumnService _columnService;
    public CmsArticleService(SugarRepository<CmsArticle> thisRepository
    ,CmsColumnService columnService)
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
    public async Task<PageResult<CmsArticleDto>> PagesAsync([FromBody]CmsArticleParam param)
    {
        var where = Expressionable.Create<CmsArticle>();
        if (!string.IsNullOrEmpty (param.Key)) {
            where.And (m => m.Title.Contains (param.Key) || m.KeyWord.Contains (param.Key) || m.SubTitle.Contains (param.Key) || m.Summary.Contains (param.Key) || m.Author.Contains (param.Key) || m.Source.Contains (param.Key) || m.Tag.Equals (param.Key));
        }
        if (param.Id != 0) {
            where.And (m => m.ColumnId==param.Id);
        }
        if (param.ColumnList.Count>0) {
            where.And (m => param.ColumnList.Contains(m.ColumnId));
        }

        if (!string.IsNullOrEmpty(param.EnTitle))
        {
            where.And(m=>SqlFunc.Subqueryable<CmsColumn>().Where(s => s.EnTitle == param.EnTitle).Any());
        }
        switch (param.Status)
        {
            case "1":
                @where.And (m => m.Status);
                break;
            case "2":
                @where.And (m => !m.Status);
                break;
        }
        if (param.Attr != 0 && param.Attr!=null) {
            where.And (m => m.Attr.ToString()!.Contains (param.Attr.ToString() ?? string.Empty));
        }

        var queryQuery = _thisRepository.AsQueryable()
            .Where(where.ToExpression())
            .WhereIF(!string.IsNullOrEmpty(param.Query), m => m.Status);
        if (!string.IsNullOrEmpty(param.Query))
        {
            var jsonParam=_thisRepository.Context.Utilities.JsonToConditionalModels(param.Query);
            queryQuery.Where(jsonParam);
        }
        var query=await queryQuery
            .OrderBy(m=>new{m.Sort,m.UpdateTime,m.Id},OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);
        var columnList = await _columnService.GetListAsync(new WhereParam());
        var res=query.Adapt<PageResult<CmsArticleDto>>();
        foreach (var item in res.Items)
        {
            var columnModel = columnList.FirstOrDefault(m => m.Id == item.ColumnId);
            if (columnModel == null) continue;
            item.ColumnName = columnModel.Title;
        }
        return res;
    }

    /// <summary>
    /// 查询推荐阅读
    /// </summary>
    /// <returns></returns>
    public async Task<List<CmsArticleDto>> GetTopAsync(long columnId,long id)
    {
        var res=await _thisRepository.AsQueryable().Take(5)
            .Where(m=>m.ColumnId==columnId && m.Id!=id)
            .OrderByDescending(m=>m.PublishTime).ToListAsync();
        return res.Adapt<List<CmsArticleDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsArticleDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        var res=model.Adapt<CmsArticleDto>();
        var columnList = await _columnService.GetListAsync(new WhereParam());
        var columnModel = columnList.FirstOrDefault(m => m.Id == model.ColumnId);
        if (columnModel!=null)
        {
            res.ColumnArr = columnModel.ParentIdList;
        }

        return res;
    }
    
    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CmsArticleDto> GetByIdAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        model.Hits += 1;
        var currentTime = DateTime.Now; 
        model.LastHitDate = currentTime;

        if (!model.LastHitDate.HasValue || model.LastHitDate.Value.Date != currentTime.Date)
        {
            model.DayHits = 1; // 当天首次点击，重置为1
        }
        else
        {
            model.DayHits += 1; // 当天非首次点击，累加
        }

        // 本周点击量：判断最后点击时间是否不在本周（注意：中国周一是一周开始，默认是周日）
        var currentWeekStart = GetWeekStartDate(currentTime);
        var lastHitWeekStart = model.LastHitDate.HasValue ? GetWeekStartDate(model.LastHitDate.Value) : DateTime.MinValue;
        if (lastHitWeekStart != currentWeekStart)
        {
            model.WeedHits = 1; // 本周首次点击，重置为1（注意：你原属性名是WeedHits，建议修正为WeekHits）
        }
        else
        {
            model.WeedHits += 1; // 本周非首次点击，累加
        }

        if (!model.LastHitDate.HasValue || model.LastHitDate.Value.Month != currentTime.Month || model.LastHitDate.Value.Year != currentTime.Year)
        {
            model.MonthHits = 1; 
        }
        else
        {
            model.MonthHits += 1; 
        }

        await _thisRepository.UpdateAsync(model);

        return model.Adapt<CmsArticleDto>();
    }
    
    private DateTime GetWeekStartDate(DateTime date)
    {
        // 计算当前日期与周一的差值（周日的DayOfWeek是0，所以特殊处理）
        int diff = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(CmsArticleDto model)
    {
        return await _thisRepository.InsertAsync(model.Adapt<CmsArticle>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsArticleDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<CmsArticle>());
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
    
    /// <summary>
    /// 添加到回收站
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UpdateRecycleAsync(List<long> ids)
    {
        var list = await _thisRepository.GetListAsync (m => ids.Contains (m.Id));
        list.ForEach (m => {
            m.Attr.Add(5);
        });
        return await _thisRepository.UpdateRangeAsync(list);
    }
    
    /// <summary>
    /// 回收站恢复
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UpdateRecoveryAsync(List<long> ids)
    {
        var list = await _thisRepository.GetListAsync (m => ids.Contains (m.Id));
        foreach (var item in list)
        {
            item.Attr.RemoveAll(m => m == 5);
        }
        return await _thisRepository.UpdateRangeAsync(list);
    }
}
