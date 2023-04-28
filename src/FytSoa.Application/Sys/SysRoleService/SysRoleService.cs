using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;

/// <summary>
/// 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysRoleService : IApplicationService
{
    private readonly SugarRepository<SysRole> _thisRepository;
    public SysRoleService(SugarRepository<SysRole> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysRoleDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .ToPageAsync(param.Limit, param.Page);
        return query.Adapt<PageResult<SysRoleDto>>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysRoleDto>> GetListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
            .WhereIF(param.TenantId!=0,m=>m.TenantId==param.TenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .Filter(null,param.TenantId!=0)
            .ToListAsync();
        return list.Adapt<List<SysRoleDto>>();
    }
    
    /// <summary>
    /// 提供支持租户参数查询，非过滤器
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysRoleDto>> GetTenantListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
            .Filter(null,true)
            .Where(m=>m.TenantId==param.TenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .ToListAsync();
        return list.Adapt<List<SysRoleDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysRoleDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<SysRoleDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<long> AddAsync(SysRoleDto model)
    {
        var addId= await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<SysRole>());
        if (model.ParentId==0)
        {
            await _thisRepository.UpdateAsync(m => new SysRole()
            {
                ParentIdList = new List<string> {addId.ToString() }
            },m=>m.Id==addId);
        }
        else
        {
            var parentModel = await _thisRepository.GetFirstAsync(m => m.Id == model.ParentId);
            parentModel.ParentIdList.Add(addId.ToString());
            await _thisRepository.UpdateAsync(m => new SysRole()
            {
                ParentIdList = parentModel.ParentIdList
            },m=>m.Id==addId);
        }

        return addId;
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyAsync(SysRoleDto model)
    {
        if (model.ParentId==0)
        {
            model.ParentIdList = new List<string>() {model.Id.ToString() };
            await _thisRepository.UpdateAsync(model.Adapt<SysRole>());
        }
        else
        {
            var parentModel = await _thisRepository.GetFirstAsync(m => m.Id == model.ParentId);
            parentModel.ParentIdList.Add(model.Id.ToString());
            model.ParentIdList = parentModel.ParentIdList;
            await _thisRepository.UpdateAsync(model.Adapt<SysRole>());
        }
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync(string ids) =>
        await _thisRepository.DeleteAsync(m => ids.StrToListLong().Contains(m.Id));

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

