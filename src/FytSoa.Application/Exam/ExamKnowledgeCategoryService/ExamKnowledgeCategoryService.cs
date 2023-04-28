using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamKnowledgeCategoryService : IApplicationService
{
    private readonly SugarRepository<ExamKnowledgeCategory> _thisRepository;
    public ExamKnowledgeCategoryService(SugarRepository<ExamKnowledgeCategory> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamKnowledgeCategoryDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.Id!=0,m=>m.GradeId==param.Id)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .Includes(m=>m.GrandCode)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamKnowledgeCategoryDto>>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<ExamKnowledgeCategoryDto>> GetListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
            .WhereIF(param.Id!=0,m=>m.GradeId==param.Id)
            .Includes(m=>m.GrandCode)
            .OrderBy(m=>m.Id,OrderByType.Desc).ToListAsync();
        return list.Adapt<List<ExamKnowledgeCategoryDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamKnowledgeCategoryDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamKnowledgeCategoryDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamKnowledgeCategoryDto model)
    {
        var upModel = await _thisRepository.GetFirstAsync(m => true, m => m.Sort);
        model.Sort = upModel.Sort + 1;
        return await _thisRepository.InsertAsync(model.Adapt<ExamKnowledgeCategory>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamKnowledgeCategoryDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<ExamKnowledgeCategory>());
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) => 
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));

    /// <summary>
    /// 自定义排序
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task ColSort(SortParam param)
    {
        int a = 0, b = 0, c = 0;
            var list = await _thisRepository.GetListAsync(m => m.ParentId == param.Parent, m => m.Sort, Common.Enum.OrderEnum.Asc);
            if (list.Count <= 0) return;
            var index = 0;
            foreach (var item in list)
            {
                index++;
                if (index == 1)
                {
                    if (item.Id != param.Id) continue;
                    if (param.Type != 1) continue;
                    a = item.Sort;
                    b = list[index].Sort;
                    c = a;
                    a = b;
                    b = c;
                    item.Sort = a;
                    await _thisRepository.UpdateAsync(item);
                    var nitem = list[index];
                    nitem.Sort = b;
                    await _thisRepository.UpdateAsync(nitem);
                    break;
                }
                if (index == list.Count)
                {
                    if (item.Id != param.Id) continue;
                    if (param.Type != 0) continue;
                    a = item.Sort;
                    b = list[index - 2].Sort;
                    c = a;
                    a = b;
                    b = c;
                    item.Sort = a;
                    await _thisRepository.UpdateAsync(item);
                    var nitem = list[index - 2];
                    nitem.Sort = b;
                    await _thisRepository.UpdateAsync(nitem);
                    break;
                }
                if (item.Id != param.Id) continue;
                if (param.Type == 1) //下降一位
                {
                    a = item.Sort;
                    b = list[index].Sort;
                    c = a;
                    a = b;
                    b = c;
                    item.Sort = a;
                    await _thisRepository.UpdateAsync(item);
                    var nitem = list[index];
                    nitem.Sort = b;
                    await _thisRepository.UpdateAsync(nitem);
                    break;
                }
                else
                {
                    a = item.Sort;
                    b = list[index - 2].Sort;
                    c = a;
                    a = b;
                    b = c;
                    item.Sort = a;
                    await _thisRepository.UpdateAsync(item);
                    var nitem = list[index - 2];
                    nitem.Sort = b;
                    await _thisRepository.UpdateAsync(nitem);
                    break;
                }
            } 
    }
}

