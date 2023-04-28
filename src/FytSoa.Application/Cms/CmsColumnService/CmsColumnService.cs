using FytSoa.Domain.Cms;
using FytSoa.Sugar;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Cms;

/// <summary>
/// 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v2")]
public class CmsColumnService : IApplicationService 
{
    private readonly SugarRepository<CmsColumn> _thisRepository;
    public CmsColumnService(SugarRepository<CmsColumn> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<CmsColumnDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<CmsColumnDto>>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<CmsColumnDto>> GetListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .OrderBy(m=>m.Id,OrderByType.Desc).ToListAsync();
        return list.Adapt<List<CmsColumnDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<CmsColumnDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<CmsColumnDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task AddAsync(CmsColumnDto model)
    {
        if (model.ParentIdList.All(m => m != "0"))
        {
            model.ParentId = long.Parse(model.ParentIdList.Last());
            var paramModel = await _thisRepository.GetByIdAsync(model.ParentId);
            model.Layer = paramModel.Layer + 1;
        }
       
        var templateRepository = _thisRepository.ChangeRepository<SugarRepository<CmsTemplate>>();
        var templateModel = await templateRepository.GetByIdAsync(model.TemplateId);
        model.TemplateName = templateModel.Name;
        var upModel = await _thisRepository.GetFirstAsync(m => true, m => m.Sort);
        model.Sort = upModel.Sort + 1;
        var id= await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<CmsColumn>());
        if (model.ParentIdList.All(m => m != "0"))
        {
            model.ParentIdList.Add(id.ToString());
        }
        else
        {
            model.ParentIdList = new List<string>() { id.ToString() };
        }
        await _thisRepository.UpdateAsync(m => new CmsColumn()
        {
            ParentIdList = model.ParentIdList
        },m=>m.Id==id);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(CmsColumnDto model)
    {
        if (model.ParentIdList.All(m => m != "0"))
        {
            model.ParentId = long.Parse(model.ParentIdList.Last());
            model.ParentIdList.Add(model.Id.ToString());
            var paramModel = await _thisRepository.GetByIdAsync(model.ParentId);
            model.Layer = paramModel.Layer + 1;
        }
        else
        {
            model.ParentIdList=new List<string> {model.Id.ToString()};
            model.Layer = 1;
        }
        var templateRepository = _thisRepository.ChangeRepository<SugarRepository<CmsTemplate>>();
        var templateModel = await templateRepository.GetByIdAsync(model.TemplateId);
        model.TemplateName = templateModel.Name;
        return await _thisRepository.UpdateAsync(model.Adapt<CmsColumn>());
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

