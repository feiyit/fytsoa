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
public class ExamMaterialCategoryService : IApplicationService
{
    private readonly SugarRepository<ExamMaterialCategory> _thisRepository;
    public ExamMaterialCategoryService(SugarRepository<ExamMaterialCategory> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamMaterialCategoryDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamMaterialCategoryDto>>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<ExamMaterialCategoryDto>> GetListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
                    .OrderBy(m=>m.Sort,OrderByType.Asc).ToListAsync();
        return list.Adapt<List<ExamMaterialCategoryDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamMaterialCategoryDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamMaterialCategoryDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamMaterialCategoryDto model)
    {
        var addId=await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<ExamMaterialCategory>());
        if (model.ParentId!=0)
        {
            var parentModel = await _thisRepository.GetByIdAsync(model.ParentId);
            parentModel.ParentIdList.Add(addId.ToString());
            model.ParentIdList=parentModel.ParentIdList;
        }
        else
        {
            model.ParentIdList=new List<string> {addId.ToString()};
        }
        var upModel = await _thisRepository.GetFirstAsync(m => true, m => m.Sort);
        model.Sort = upModel.Sort + 1;
        return await _thisRepository.UpdateAsync(m=>new ExamMaterialCategory()
        {
            Sort = model.Sort,
            ParentIdList = model.ParentIdList
        },m=>m.Id==addId);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamMaterialCategoryDto model)
    {
        if (model.ParentId!=0)
        {
            var parentModel = await _thisRepository.GetByIdAsync(model.ParentId);
            parentModel.ParentIdList.Add(model.Id.ToString());
            model.ParentIdList=parentModel.ParentIdList;
        }
        else
        {
            model.ParentIdList=new List<string> {model.Id.ToString()};
        }
        return await _thisRepository.UpdateAsync(model.Adapt<ExamMaterialCategory>());
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

