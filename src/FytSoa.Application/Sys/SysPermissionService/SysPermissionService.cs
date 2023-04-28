using FytSoa.Common.Extensions;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 授权表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysPermissionService : IApplicationService
{
    private readonly SugarRepository<SysPermission> _thisRepository;
    private readonly SugarRepository<SysAdminRole> _adminRoleRepository;
    private readonly SugarRepository<SysRole> _roleRepository;
    private readonly SugarRepository<SysRoleConflict> _roleConflictRepository;
    public SysPermissionService(SugarRepository<SysPermission> thisRepository
    ,SugarRepository<SysAdminRole> adminRoleRepository
    ,SugarRepository<SysRoleConflict> roleConflictRepository
    ,SugarRepository<SysRole> roleRepository)
    {
        _thisRepository = thisRepository;
        _adminRoleRepository = adminRoleRepository;
        _roleConflictRepository = roleConflictRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 根据角色获得权限
    /// </summary>
    /// <returns></returns>
    [HttpGet("{roleId}")]
    public async Task<List<SysPermissionDto>> GetByRoleAsync(long roleId)
    {
        var list = await _thisRepository.GetListAsync(m => m.RoleId == roleId && m.Types == 3,
            m => m.MenuId);
        return list.Adapt<List<SysPermissionDto>>();
    }

    /// <summary>
    /// 根据用户获得授权的角色
    /// </summary>
    /// <returns></returns>
    [HttpGet("{adminId}")]
    public async Task<List<string>> GetByAdminAsync(long adminId)
    {
        var model = await _adminRoleRepository.GetListAsync(m => m.AdminId == adminId);
        return model.Select(m=>m.RoleId.ToString()).ToList();
    }

    /// <summary>
    /// 为用户授权多个角色
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task AddRoleAsync(SysAuthorityAdminByRoleParam param)
    {
        //根据角色查询互斥内容
        var roleConflict = await _roleConflictRepository.GetListAsync();
        if (roleConflict.Count>0)
        {
            if (roleConflict.Any(item => param.RoleArr.Contains(item.RoleA.ToString()) && param.RoleArr.Contains(item.RoleB.ToString())))
            {
                throw new BusinessException("角色存在互斥关系，无法授权！~");
            }
        }
        
        //查询角色列表
        var roleList = await _roleRepository.GetListAsync(m => param.RoleArr.Contains(m.Id.ToString()));
        //查询已授权角色信息
        var adminRoleList = await _adminRoleRepository.GetListAsync(m => param.RoleArr.Contains(m.RoleId.ToString()));
        foreach (var item in roleList)
        {
            if (item is { MaxLength: 0 })
            {
                continue;
            }

            if (item != null && item.MaxLength<=adminRoleList.Count(m=>m.RoleId==item.Id))
            {
                throw new BusinessException("["+item.Name+"]-已达到角色设置最大边界值！~");
            }
        }
        
        var addRole = new List<SysAdminRole>();
        var adminRoleArr = await _adminRoleRepository.GetListAsync(m => param.AdminArr.Contains(m.AdminId.ToString()));
        foreach (var item in param.AdminArr)
        {
            var roleIds = adminRoleArr.Where(m => m.AdminId == long.Parse(item))
                .Select(m => m.RoleId.ToString()).ToList();
            var differenceQuery = param.RoleArr.Except(roleIds);
            addRole.AddRange(differenceQuery.Select(row => new SysAdminRole() { AdminId = long.Parse(item), RoleId = long.Parse(row) }));
        }
        if (addRole.Count>0)
        {
            await _adminRoleRepository.InsertRangeAsync(addRole);
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(SysAuthorityParam model)
    {
        await _thisRepository.DeleteAsync(m => m.RoleId == model.RoleId && m.Types == 3);
        var list = model.Menus.Select(item => new SysPermission()
            {
                RoleId = model.RoleId, MenuId = item.MenuId, Api = item.Api, Types = 3
            })
            .ToList();

        return await _thisRepository.InsertRangeAsync(list);
    }
}