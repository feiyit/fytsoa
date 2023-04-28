using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.DynamicApi.Attributes;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysMenuService : IApplicationService
{
    private readonly SugarRepository<SysMenu> _thisRepository;
    private readonly SugarRepository<SysAdminRole> _adminRoleRepository;
    private readonly SugarRepository<SysRole> _roleRepository;
    public SysMenuService(SugarRepository<SysMenu> thisRepository
        ,SugarRepository<SysAdminRole> adminRoleRepository
        ,SugarRepository<SysRole> roleRepository)
    {
        _thisRepository = thisRepository;
        _adminRoleRepository = adminRoleRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysMenuDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysMenuDto>>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<SysMenuDto>> GetListAsync(WhereParam param)
    {
        var list = await _thisRepository.AsQueryable()
            .Filter(null,true)
            .Where(m=>m.TenantId==param.TenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key))
            .OrderBy(m=>m.Sort)
            .ToListAsync();
        return list.Adapt<List<SysMenuDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysMenuDto> GetAsync(long id)
    {
        var model = await _thisRepository
            .AsQueryable()
            .Filter(null,true).FirstAsync(m=>m.Id==id);
        return model.Adapt<SysMenuDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysMenuDto model)
    {
        var isAnyCode = await _thisRepository.IsAnyAsync(m => m.Code == model.Code);
        if (isAnyCode)
        {
            throw new BusinessException("别名不能重复~");
        }
        if (model.ParentIdList.All(m => m != "0"))
        {
            model.ParentId = long.Parse(model.ParentIdList.Last());
            var paramModel = await _thisRepository
                .AsQueryable()
                .Filter(null,true).FirstAsync(m=>m.Id==model.ParentId);
            model.Layer = paramModel.Layer + 1;
            model.ParentIdList.Add(model.Id.ToString());
        }
        else
        {
            model.ParentIdList = new List<string> {model.Id.ToString()};
        }

        var upModel = await _thisRepository
            .AsQueryable()
            .Filter(null,true)
            .OrderBy(m=>m.Sort,OrderByType.Desc)
            .FirstAsync();
        model.Sort = upModel.Sort + 1;
        return await _thisRepository.InsertAsync(model.Adapt<SysMenu>());
    }

    /// <summary>
    /// 添加-临时=未命名
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<SysMenuDto> AddTempAsync(SysMenuTempParam param)
    {
        var model = new SysMenu()
        {
            Name = param.Name,
            ParentId = param.ParentId,
            TenantId = param.TenantId,
            Types = "menu",
        };
        model.ParentIdList =
            param.ParentId != 0
                ? new List<string>() {param.ParentId.ToString(), model.Id.ToString()}
                : new List<string> {model.Id.ToString()};

        var upModel = await _thisRepository
            .AsQueryable()
            .Filter(null,true)
            .OrderBy(m=>m.Sort,OrderByType.Desc)
            .FirstAsync();
        model.Sort = upModel.Sort + 1;

        await _thisRepository.InsertAsync(model);
        var addModel = await _thisRepository
            .AsQueryable()
            .Filter(null,true).FirstAsync(m=>m.Id==model.Id);
        return addModel.Adapt<SysMenuDto>();
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyAsync(SysMenuDto model)
    {
        var isAnyCode = await _thisRepository.IsAnyAsync(m => m.Code == model.Code && m.Id!=model.Id);
        if (isAnyCode)
        {
            throw new BusinessException("别名不能重复~");
        }
        model.ParentIdList.RemoveAt(model.ParentIdList.Count - 1);
        if (model.ParentIdList.Count > 0 && model.ParentIdList.All(m => m != "0"))
        {
            model.ParentId = long.Parse(model.ParentIdList.Last());
            model.ParentIdList.Add(model.Id.ToString());
        }
        else
        {
            model.ParentIdList = new List<string> {model.Id.ToString()};
        }

        await _thisRepository
            .AsUpdateable(model.Adapt<SysMenu>())
            .IgnoreColumns(m=>new{m.TenantId})
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 拖动节点排序
    /// </summary>
    /// <returns></returns>
    public async Task ModifyDraggingAsync(SysMenuDropDto model)
    {
        var dragging = await _thisRepository.GetByIdAsync(model.DraggingNode.Id);
        var drop = await _thisRepository.GetByIdAsync(model.DropNode.Id);
        //向上
        if (model.DropType is "before" or "after" && model.DraggingNode.ParentId == model.DropNode.ParentId)
        {
            //同级上下处理
            drop.ParentIdList.RemoveAt(drop.ParentIdList.Count - 1);
            dragging.ParentIdList = drop.ParentIdList;
            dragging.ParentIdList.Add(dragging.Id.ToString());
            dragging.ParentId = drop.ParentId;

            //(dragging.Sort, drop.Sort) = (drop.Sort, dragging.Sort);
            await _thisRepository.UpdateRangeAsync(new[] {dragging, drop});
            //查出同级别列表
            var list = await _thisRepository.GetListAsync(m => m.ParentId == drop.ParentId && m.Id!=dragging.Id);
            var index = 1;
            foreach (var item in list.OrderBy(m=>m.Sort))
            {
                index++;
                item.Sort = index;
            }
            
            //处理同级向上，重新排序 +1
            if (model.DropType=="before")
            {
                var dropModel = list.FirstOrDefault(m => m.Id == drop.Id);
                if (dropModel!=null)
                {
                    Console.WriteLine("before-"+dropModel.Sort);
                    foreach (var item in list.Where(m=>m.Sort>dropModel.Sort).OrderBy(m=>m.Sort))
                    {
                        item.Sort += 1;
                    }
                    
                    await _thisRepository.UpdateRangeAsync(list);
                    await _thisRepository.UpdateAsync(m => new SysMenu()
                    {
                        Sort = dropModel.Sort
                    },m=>m.Id==dragging.Id);
                }
            }
            
            //处理同级向下，重新排序 -1
            if (model.DropType=="after")
            {
                var dropModel = list.FirstOrDefault(m => m.Id == drop.Id);
                if (dropModel!=null)
                {
                    Console.WriteLine("after-"+dropModel.Sort);
                    foreach (var item in list.Where(m=>m.Sort>dropModel.Sort).OrderBy(m=>m.Sort))
                    {
                        item.Sort += 1;
                    }
                    
                    await _thisRepository.UpdateRangeAsync(list);
                    await _thisRepository.UpdateAsync(m => new SysMenu()
                    {
                        Sort = dropModel.Sort+1
                    },m=>m.Id==dragging.Id);
                }
            }
        }
        switch (model.DropType)
        {
            case "before" or "after" when model.DraggingNode.ParentId != model.DropNode.ParentId:
                //同级内，向下级或上级处理
                drop.ParentIdList.RemoveAt(drop.ParentIdList.Count - 1);
                dragging.ParentIdList = drop.ParentIdList;
                dragging.ParentIdList.Add(dragging.Id.ToString());
                dragging.ParentId = drop.ParentId;

                await _thisRepository.UpdateAsync(dragging);
                break;
            case "inner":
                dragging.ParentIdList = drop.ParentIdList;
                dragging.ParentIdList.Add(dragging.Id.ToString());
                dragging.ParentId = drop.Id;
                await _thisRepository.UpdateAsync(dragging);
                break;
        }
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync(string ids)
    {
        return await _thisRepository.DeleteAsync(m => ids.StrToListLong().Contains(m.Id));
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

    /// <summary>
    /// 根据登录人ID查询权限菜单[SCUI]
    /// </summary>
    /// <returns></returns>
    public async Task<AuthorityDto> GetAuthorityMenuAsync()
    {
        var adminRepository = _thisRepository.ChangeRepository<SugarRepository<SysAdmin>>();
        //根据用户查询角色ID
        var adminModel = await adminRepository.AsQueryable()
            .Filter(null,true)
            .FirstAsync(m => m.Id == AppUtils.LoginId);
        var adminRole = await _adminRoleRepository.GetListAsync(m => m.AdminId == AppUtils.LoginId);
        var roleIds = adminRole.Select(m=>m.RoleId.ToString()).ToList();
        
        //处理 角色上级 继承
        var adminRoleList = await _roleRepository.GetListAsync(m => roleIds.Contains(m.Id.ToString()));
        var adminRoleIdArr = adminRoleList.SelectMany(item => item.ParentIdList).ToList();
        adminRoleIdArr = adminRoleIdArr.Distinct().ToList();
        //根据角色查询授权的菜单Id集合
        var permissionRepository = _thisRepository.ChangeRepository<SugarRepository<SysPermission>>();
        var permissionList = await permissionRepository
            .AsQueryable()
            .Filter(null,true)
            .Where(m => adminRoleIdArr.Contains(m.RoleId.ToString()))
            .ToListAsync();
        permissionList = permissionList.DistinctBy(m=>m.MenuId).ToList();
        
        #region 保存授权api

        var apiList = new List<SysMenuApiUrl>();
        foreach (var item in permissionList)
        {
            apiList.AddRange(item.Api);
        }
        RedisService.Instance.SetJson(KeyUtils.AUTHORIZZATIONAPI+":"+adminModel.Id, apiList);
        #endregion
        //查询菜单集合
        var menuIds = permissionList.Select(m => m.MenuId).ToList();

        //根据菜单ID查询菜单详细
        var menuList = await _thisRepository
            .AsQueryable()
            .Where(m => menuIds.Contains(m.Id))
            .Filter(null,true)
            .OrderBy(m=>m.Sort)
            .ToListAsync();
        
        #region 处理指令级资源权限

        var directiveList = new List<string>();
        foreach (var item in permissionList)
        {
            if (item.Api.Count==0) continue;
            var directiveMenu = menuList.FirstOrDefault(m => m.Id == item.MenuId);
            directiveList.AddRange(item.Api.Select(m => m.code).Select(row => directiveMenu?.Code + ":" + row));
        }
        #endregion
        
        var resMenu = new List<AuthorityMenuDto>();
        foreach (var item in menuList.Where(m => m.ParentId == 0).OrderBy(m => m.Sort))
        {
            resMenu.Add(new AuthorityMenuDto()
            {
                path = item.Urls,
                name = item.Code.ToLower(),
                meta = new AuthorityMeta()
                {
                    title = item.Name,
                    icon = item.Icon,
                    type = item.Types,
                    hidden = !item.Status?true:null,
                    fullpage = item.FullPage?true:null
                },
                children = RecursiveModuleSc(menuList, item)
            });
        }
        return new AuthorityDto(){Menu = resMenu,Directive = directiveList};
    }
    
    /// <summary>
    /// 递归模块列表
    /// </summary>
    /// <param name="source"></param>
    /// <param name="parentModel"></param>
    /// <returns></returns>
    [NonDynamicMethod]
    private List<AuthorityMenuDto> RecursiveModuleSc(List<SysMenu> source, SysMenu parentModel)
    {
        var result = new List<AuthorityMenuDto>();
        foreach (var item in source.Where(m => m.ParentId == parentModel.Id).OrderBy(m => m.Sort))
        {
            var recursiveList = RecursiveModuleSc(source, item);
            result.Add(new AuthorityMenuDto()
            {
                path = item.Urls,
                name = item.Code,
                component = item.VuePath,
                meta = new AuthorityMeta()
                {
                    title = item.Name,
                    icon = item.Icon,
                    type = item.Types,
                    hidden = !item.Status?true:null,
                    fullpage = item.FullPage?true:null,
                    affix = item.Code == "dashboard"
                },
                children = recursiveList
            });
        }

        return result;
    }
}