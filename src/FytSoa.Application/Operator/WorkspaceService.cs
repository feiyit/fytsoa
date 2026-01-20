using FytSoa.Application.Cms;
using FytSoa.Application.Dto;
using FytSoa.Domain.Cms;
using FytSoa.Sugar;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Operator;

/// <summary>
/// 工作提
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class WorkspaceService : IApplicationService
{
    private readonly SugarRepository<CmsArticle> _articleRepository;
    public WorkspaceService(SugarRepository<CmsArticle> articleRepository)
    {
        _articleRepository = articleRepository;
    }

    /// <summary>
    /// 返回工作台内容
    /// </summary>
    /// <returns></returns>
    public async Task<WorkspaceDto> GetWorkspaceAsync()
    {
        var newest = await _articleRepository.AsQueryable().Take(5)
            .ToListAsync();
        var res=new WorkspaceDto
        {
            ArticleCountDay = await _articleRepository.AsQueryable().CountAsync(m=>SqlFunc.DateIsSame(m.CreateTime, DateTime.Now)),
            ArticleCountAll = await _articleRepository.AsQueryable().CountAsync(),
            ArticleCountCheck = await _articleRepository.AsQueryable().CountAsync(m=>!m.Status),
            ArticleCountComment = 0,
            ArticleCountDraft = await _articleRepository.AsQueryable().CountAsync(m=>m.Attr.ToString()!.Contains("5")),
            NewestArticle = newest.Adapt<List<CmsArticleDto>>()
        };
        return res;
    }

    /// <summary>
    /// 获得文章浏览数据，根据年月
    /// </summary>
    /// <returns></returns>
    public async Task<List<WorkspaceDto.ArticleViewDto>> GetArticleViewAsync(int year,int month,int type=1)
    {
        var res=new List<WorkspaceDto.ArticleViewDto>();
        if (type==1)
        {
            var list = await _articleRepository.AsQueryable()
                .Where(m => m.PublishTime.Year == year)
                .GroupBy(m => m.PublishTime.Month)
                .Select(m => new
                {   
                    count = SqlFunc.AggregateCount(m.Id),
                    month=m.PublishTime.Month
                })
                .ToListAsync();
            for (var i = 1; i < 13; i++)
            {
                var model = list.Find(m => m.month == i);
                if (model == null)
                {
                    res.Add(new WorkspaceDto.ArticleViewDto()
                    {
                        Name = i+"月",
                        Value = 0
                    });
                }
                else
                {
                    res.Add(new WorkspaceDto.ArticleViewDto()
                    {
                        Name = i+"月",
                        Value = model.count
                    });
                }
            }
        }
        else
        {
            var list = await _articleRepository.AsQueryable()
                .Where(m => m.PublishTime.Year == year && m.PublishTime.Month == month)
                .GroupBy(m => m.PublishTime.Day)
                .Select(m => new
                {   
                    count = SqlFunc.AggregateCount(m.Id),
                    day=m.PublishTime.Day
                })
                .ToListAsync();
            for (var i = 1; i < DateTime.DaysInMonth(year, month)+1; i++)
            {
                var model = list.Find(m => m.day == i);
                if (model == null)
                {
                    res.Add(new WorkspaceDto.ArticleViewDto()
                    {
                        Name = i+"日",
                        Value = 0
                    });
                }
                else
                {
                    res.Add(new WorkspaceDto.ArticleViewDto()
                    {
                        Name = i+"日",
                        Value = model.count
                    });
                }
            }
        }
        return res;
    }
}