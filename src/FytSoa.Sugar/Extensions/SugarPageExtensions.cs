using FytSoa.Common.Result;
using Mapster;
using SqlSugar;

namespace FytSoa.Sugar;

/// <summary>
/// 分页扩展
/// </summary>
public static class  SugarPageExtensions
{
    public static async Task<PageResult<T>> ToPageAsync<T>(this ISugarQueryable<T> query,
        int pageIndex,
        int pageSize,bool isMapper=true)
    {
        RefAsync<int> totalItems =0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, totalItems);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PageResult<T>()
        {
            Items = isMapper?items.Adapt<List<T>>():items,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}

