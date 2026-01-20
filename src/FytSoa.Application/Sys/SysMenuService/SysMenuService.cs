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
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key)).OrderByDescending(m=>m.Sort)
            .OrderBy(m=>m.Sort)
            .ToListAsync();
        return list.Adapt<List<SysMenuDto>>().OrderBy(m=>m.Sort).ToList();
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
            Types = "menu",
        };
        /*model.ParentIdList =
            param.ParentId != 0
                ? new List<string>() {param.ParentId.ToString(), model.Id.ToString()}
                : new List<string> {model.Id.ToString()};*/

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

        await _thisRepository
            .AsUpdateable(model.Adapt<SysMenu>())
            .IgnoreColumns(m=>new{m.TenantId})
            .ExecuteCommandAsync();
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
    /// 根据登录人ID查询权限菜单[Vban]
    /// </summary>
    /// <returns></returns>
    public async Task<AuthorityVbanDto> GetVbanMenuAsync()
    {
        List<SysMenu> menuList;
        var directiveList = new List<string>();
        var adminRoleList = new List<SysRole>();
        if (AppUtils.LoginId==1)
        {
            //超级管理员
            menuList = await _thisRepository
                .AsQueryable()
                .Filter(null,true)
                .Where(m=>m.TenantId==0)
                .OrderBy(m=>m.Sort)
                .ToListAsync();
            foreach (var item in menuList)
            {
                if (item.Api.Count==0) continue;
                directiveList.AddRange(item.Api.Select(row => item.Code + ":" + row.code));
            }
        }
        else
        {
            // var adminRepository = _thisRepository.ChangeRepository<SugarRepository<SysAdmin>>();
            //根据用户查询角色ID
            /*var adminModel = await adminRepository.AsQueryable()
                .Filter(null,true)
                .FirstAsync(m => m.Id == AppUtils.LoginId);*/
            var adminRole = await _adminRoleRepository.GetListAsync(m => m.AdminId == AppUtils.LoginId);
            var roleIds = adminRole.Select(m=>m.RoleId.ToString()).ToList();
            
            //处理 角色上级 继承
            /*adminRoleList = await _roleRepository.GetListAsync(m => roleIds.Contains(m.Id.ToString()));
            var adminRoleIdArr = adminRoleList.SelectMany(item => item.ParentIdList).ToList();
            adminRoleIdArr = adminRoleIdArr.Distinct().ToList();*/
            //根据角色查询授权的菜单Id集合
            var permissionRepository = _thisRepository.ChangeRepository<SugarRepository<SysPermission>>();
            var permissionList = await permissionRepository
                .AsQueryable()
                .Filter(null,true)
                .Where(m => roleIds.Contains(m.RoleId.ToString()))
                .ToListAsync();
            permissionList = permissionList.DistinctBy(m=>m.MenuId).ToList();
            
            #region 保存授权api

            /*var apiList = new List<SysMenuApiUrl>();
            foreach (var item in permissionList)
            {
                apiList.AddRange(item.Api);
            }
            RedisService.Instance.SetJson(KeyUtils.AUTHORIZZATIONAPI+":"+adminModel.Id, apiList);*/
            #endregion
            //查询菜单集合
            var menuIds = permissionList.Select(m => m.MenuId).ToList();

            //根据菜单ID查询菜单详细
            menuList = await _thisRepository
                .AsQueryable()
                .Where(m => menuIds.Contains(m.Id))
                .Filter(null,true)
                .OrderBy(m=>m.Sort)
                .ToListAsync();
            
            #region 处理指令级资源权限

            foreach (var item in permissionList)
            {
                if (item.Api.Count==0) continue;
                var directiveMenu = menuList.FirstOrDefault(m => m.Id == item.MenuId);
                directiveList.AddRange(item.Api.Select(m => m.code).Select(row => directiveMenu?.Code + ":" + row));
            }
            #endregion
            
        }
        
        var resMenu = new List<RouteVbanRecord>();
        foreach (var item in menuList.Where(m => m.ParentId == 0).OrderBy(m => m.Sort))
        {
            resMenu.Add(new RouteVbanRecord()
            {
                Path = item.Urls,
                Name = item.Code.ToLower(),
                Meta = new RouteVbanMeta()
                {
                    Title = item.Name,
                    Icon = item.Icon,
                    AffixTab=_workList.Contains(item.Code),
                    Link = item.Types=="link"? item.Urls:""
                    
                },
                Type = item.Types,
                Component = item.VuePath,
                Redirect = item.Redirect,
                Children = RecursiveModuleVban(menuList, item)
            });
        }
        return new AuthorityVbanDto(){Menu = resMenu,Directive = directiveList,Workbench = "workspace"};
    }
    
    /// <summary>
    /// 递归模块列表
    /// </summary>
    /// <param name="source"></param>
    /// <param name="parentModel"></param>
    /// <returns></returns>
    [NonDynamicMethod]
    private List<RouteVbanRecord> RecursiveModuleVban(List<SysMenu> source, SysMenu parentModel)
    {
        var result = new List<RouteVbanRecord>();
        foreach (var item in source.Where(m => m.ParentId == parentModel.Id).OrderBy(m => m.Sort))
        {
            var recursiveList = RecursiveModuleVban(source, item);
            result.Add(new RouteVbanRecord()
            {
                Path = item.Urls,
                Name = item.Code.ToLower(),
                Meta = new RouteVbanMeta()
                {
                    Title = item.Name,
                    Icon = item.Icon,
                    AffixTab=_workList.Contains(item.Code),
                    Link = item.Types=="link"? item.Urls:"",
                },
                Type = item.Types,
                Component = item.VuePath,
                Redirect = item.Redirect,
                Children = recursiveList
            });
        }

        return result;
    }

    private readonly List<string> _workList = new() { "workspace","crm-analytical","erp-analytical"};
}