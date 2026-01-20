using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 租户管理服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysTenantService : IApplicationService 
{
    private readonly SugarRepository<SysTenant> _thisRepository;
    private readonly SysAdminService _adminService;
    private readonly SysRoleService _roleService;
    private readonly SugarRepository<SysRole> _roleRepository;
    private readonly SugarRepository<SysAdmin> _adminRepository;
    private readonly SugarRepository<SysAdminRole> _adminRoleRepository;
    public SysTenantService(SugarRepository<SysTenant> thisRepository
    ,SysAdminService adminService
    ,SysRoleService roleService
    ,SugarRepository<SysRole> roleRepository
    ,SugarRepository<SysAdmin> adminRepository
    ,SugarRepository<SysAdminRole> adminRoleRepository)
    {
        _thisRepository = thisRepository;
        _adminService = adminService;
        _roleService = roleService;
        _roleRepository = roleRepository;
        _adminRepository = adminRepository;
        _adminRoleRepository = adminRoleRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysTenantDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Filter(null,true)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key) || m.Person.Contains(param.Key))
            .OrderBy(m=>m.Id,OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<SysTenantDto>>();
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<SysTenantDto>> GetListAsync(WhereParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Filter(null, true)
            .WhereIF(!string.IsNullOrEmpty(param.Key), m => m.Name.Contains(param.Key) || m.Person.Contains(param.Key))
            .OrderBy(m => m.Id, OrderByType.Desc)
            .ToListAsync();
        return query.Adapt<List<SysTenantDto>>();
    }


    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysTenantDto> GetAsync(long id)
    {
        var model = await _thisRepository
            .AsQueryable()
            .Filter(null,true)
            .FirstAsync(m=>m.Id==id);
        return model.Adapt<SysTenantDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [UnitOfWork]
    public async Task AddAsync(SysTenantDto model)
    {
        model.TenantId = Unique.Id();
        await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<SysTenant>());
        
        //增加角色
        var roleId=await _roleService.AddAsync(new SysRoleDto()
        {
            TenantId =  model.TenantId,
            Name = "超级管理员",
            Number = "10000",
            IsSystem = true
        });
        var adminId=await _adminService.AddAsync(new SysAdminDto()
        {
            TenantId =  model.TenantId,
            OrganizeId = 0,
            LoginAccount = model.Account,
            LoginPassWord = model.PassWord,
            Avatar = "/upload/avatar/6.jpg",
            FullName = model.Person,
            Mobile = model.Phone
        });

        await _adminRoleRepository.InsertAsync(new SysAdminRole()
        {
            TenantId = model.TenantId,
            AdminId = adminId,
            RoleId = roleId
        });
    }
    
    /// <summary>
    /// 密码重置
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task PassResetAsync(List<long> id)
    {
        var newPwd = "123456a!".MDString();
        await _thisRepository.AsUpdateable()
            .IgnoreColumns(m => new { m.TenantId })
            .SetColumns(m=>new SysTenant()
            {
                PassWord = newPwd
            })
            .Where(m=>id.Contains(m.Id))
            .ExecuteCommandAsync();
        var tenantIdArr = await _thisRepository.AsQueryable()
            .Filter(null, true)
            .Where(m => id.Contains(m.Id))
            .Select(m => m.TenantId)
            .ToListAsync();
        Console.WriteLine("租户数量："+tenantIdArr.Count);
        // 根据租户查询最早创建的用户
        var adminList = await _adminRepository.AsQueryable()
            .Filter(null, true)
            .Where(m=>tenantIdArr.Contains(m.TenantId))
            .ToListAsync();
        Console.WriteLine("用户数量："+adminList.Count);
        var updateIdArr = new List<long>();
        foreach (var item in tenantIdArr)
        {
            var admin = adminList
                .Where(m=>m.TenantId==item).OrderBy(m => m.Id).FirstOrDefault();
            if (admin==null)
            {
                continue;
            }
            updateIdArr.Add(admin.Id);
        }
        if (updateIdArr.Count>0)
        {
            var adminPwd = newPwd.AESEncrypt();
            await _adminRepository.AsUpdateable()
                .IgnoreColumns(m => new { m.TenantId })
                .SetColumns(m => new SysAdmin()
                {
                    LoginPassWord = adminPwd
                })
                .Where(m => updateIdArr.Contains(m.Id))
                .ExecuteCommandAsync();
        }
    }


    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyAsync(SysTenantDto model)
    {
        await _thisRepository.AsUpdateable(model.Adapt<SysTenant>())
            .IgnoreColumns(m => new { m.TenantId })
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
