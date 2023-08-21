using FytSoa.Application.Operator;
using FytSoa.Common.Extensions;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Cache;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.DynamicApi.Attributes;
using Mapster;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
    
namespace FytSoa.Application.Sys;

/// <summary>
/// 管理员表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysAdminService : IApplicationService
{
    private readonly SugarRepository<SysAdmin> _thisRepository;
    private readonly SugarRepository<SysRole> _roleRepository;
    private readonly SugarRepository<SysAdminRole> _adminRoleRepository;
    private readonly SugarRepository<SysRoleConflict> _roleConflictRepository;
    public SysAdminService(SugarRepository<SysAdmin> thisRepository
    ,SugarRepository<SysRole> roleRepository
    ,SugarRepository<SysAdminRole> adminRoleRepository
    ,SugarRepository<SysRoleConflict> roleConflictRepository)
    {
        _thisRepository = thisRepository;
        _roleRepository = roleRepository;
        _adminRoleRepository = adminRoleRepository;
        _roleConflictRepository = roleConflictRepository;
    }

    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysAdminDto>> GetPagesAsync(AdminWhereParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.TenantId!=0,m=>m.TenantId==param.TenantId)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m => m.FullName.Contains(param.Key) || m.Mobile.Contains(param.Key) || m.Email.Contains(param.Key) ||
                                                           m.LoginAccount.Contains(param.Key))
            .WhereIF(!string.IsNullOrEmpty(param.Status), m => m.Status == (param.Status == "1"))
            .WhereIF(param.Id!=1 && param.Id!=0,m=> SqlFunc.Subqueryable<SysAdminRole>().Where(r=>r.AdminId==m.Id && r.RoleId==param.Id).Any())
            .WhereIF(param.OrgId!=0,m=>m.OrganizeId==param.OrgId)
            .Filter(null,true)
            .Includes(m=>m.OrganizeObj)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result = query.Adapt<PageResult<SysAdminDto>>();
        if (result.Items.Count==0)
        {
            return result;
        }
        var groupList = await _roleRepository.GetListAsync();
        foreach (var item in result.Items)
        {
            var group = groupList.Where(m => item.RoleGroup.Contains(m.Id.ToString()));
            item.RoleGroupName = string.Join(",", group.Select(m => m.Name).ToArray());;
        }
        return result;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<SysAdminDto> GetAsync(long id)
    {
        var model = await _thisRepository
            .AsQueryable()
            .Filter(null,true)
            .FirstAsync(m=>m.Id==id);
        model.LoginPassWord = model.LoginPassWord.AESDecrypt();
        var result=model.Adapt<SysAdminDto>();
        var adminRole = await _adminRoleRepository.GetListAsync(m => m.AdminId == id);
        result.RoleGroup = adminRole.Select(m => m.RoleId.ToString()).ToList();
        return result;
    }
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginParam"></param>
    /// <returns></returns>
    [NonDynamicMethod]
    public async Task<SysAdminDto> LoginAsync(LoginParam loginParam)
    {
        var code = MemoryService.Default.GetCache<string>(KeyUtils.CAPTCHACODE+loginParam.CodeKey);
        if (!string.Equals(code, loginParam.Code, StringComparison.CurrentCultureIgnoreCase)) throw new ArgumentException("验证码输入错误！~");

        //判断是否为超级管理员
        var superAccount = AppUtils.Configuration["SuperUser:Account"];
        var superPassWord = AppUtils.Configuration["SuperUser:Password"];
        if (loginParam.Account==superAccount)
        {
            if (loginParam.Password.AESEncrypt()!=superPassWord.AESEncrypt())
            {
                throw new BusinessException("密码输入错误！~");
            }

            return new SysAdminDto()
            {
                Id=1,
                IsSuper = true,
                TenantId = 0,
                Avatar = "/upload/avatar/avatar-5.jpeg",
                FullName="超级管理员"
            };
        }
        
        var model = await _thisRepository
            .AsQueryable()
            .Filter(null,true)
            .FirstAsync(m=> !m.IsDel && m.LoginAccount==loginParam.Account);
        if (model == null) throw new BusinessException("账号输入错误！~");

        if (model.LoginPassWord != loginParam.Password.ToLower().AESEncrypt())  throw new BusinessException("密码输入错误！~");

        if (!model.Status)  throw new BusinessException("账号被冻结，请联系管理员！~");

        model.LoginTime = DateTime.Now;
        model.LoginCount += 1;
        model.UpLoginTime = model.LoginTime;
        var tenantId = model.TenantId;
        await _thisRepository.AsUpdateable(model)
            .IgnoreColumns(m => new { m.TenantId })
            .ExecuteCommandAsync();
        model.TenantId = tenantId;
        return model.Adapt<SysAdminDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<long> AddAsync(SysAdminDto model)
    {
        //根据角色查询互斥内容
        var roleConflict = await _roleConflictRepository.GetListAsync();
        if (roleConflict.Count>0)
        {
            if (roleConflict.Any(item => model.RoleGroup.Contains(item.RoleA.ToString()) && model.RoleGroup.Contains(item.RoleB.ToString())))
            {
                throw new BusinessException("角色存在互斥关系，无法添加！~");
            }
        }
        //查询角色列表
        var roleList = await _roleRepository.GetListAsync(m => model.RoleGroup.Contains(m.Id.ToString()));
        //查询已授权角色信息
        var adminRoleList = await _adminRoleRepository.GetListAsync(m => model.RoleGroup.Contains(m.RoleId.ToString()));
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
        model.LoginPassWord = model.LoginPassWord.ToLower().AESEncrypt();
        var id= await _thisRepository.InsertReturnSnowflakeIdAsync(model.Adapt<SysAdmin>());
        var addAdminRoleList = model.RoleGroup.Select(item => new SysAdminRole() { AdminId = id, RoleId = long.Parse(item) }).ToList();
        await _adminRoleRepository.InsertRangeAsync(addAdminRoleList);
        return id;
    }
    
    /// <summary>
    /// 修改个人基本信息
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyBasicAsync(BasicParam model)
    {
        await _thisRepository.UpdateAsync(m=>new SysAdmin()
        {
            FullName = model.FullName,
            Sex = model.Sex,
            Summary = model.Summary
        },m=>m.Id==model.Id);
    }
    
    /// <summary>
    /// 设置新密码
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifySettingPwdAsync(SetNewPasswordParam model)
    {
        var source = model.SourcePwd.MDString().AESEncrypt();
        var isAny = await _thisRepository.IsAnyAsync(m => m.LoginPassWord == source && m.Id==model.Id);
        if (!isAny)
        {
            throw new BusinessException("原密码输入错误~");
        }

        var newPwd = model.PassWord.MDString().AESEncrypt();
        await _thisRepository.UpdateAsync(m => new SysAdmin()
        {
            LoginPassWord = newPwd
        },m=>m.Id==model.Id);
    }
    
    /// <summary>
    /// 密码重置
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task PassResetAsync(List<long> id)
    {
        var newPwd = "123456a!".MDString().ToLower().AESEncrypt();
        await _thisRepository.UpdateAsync(m => new SysAdmin()
        {
            LoginPassWord =newPwd
        }, m => id.Contains(m.Id));
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyAsync(SysAdminDto model)
    {
        //根据角色查询互斥内容
        var roleConflict = await _roleConflictRepository.GetListAsync();
        if (roleConflict.Count>0)
        {
            if (roleConflict.Any(item => model.RoleGroup.Contains(item.RoleA.ToString()) && model.RoleGroup.Contains(item.RoleB.ToString())))
            {
                throw new BusinessException("角色存在互斥关系，无法添加！~");
            }
        }
        //查询角色列表
        var roleList = await _roleRepository.GetListAsync(m => model.RoleGroup.Contains(m.Id.ToString()));
        //查询已授权角色信息
        var adminRoleList = await _adminRoleRepository.GetListAsync(m => model.RoleGroup.Contains(m.RoleId.ToString()));
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
        model.LoginPassWord = model.LoginPassWord.AESEncrypt();
        await _thisRepository.UpdateAsync(model.Adapt<SysAdmin>());
        await _adminRoleRepository.DeleteAsync(m => m.AdminId == model.Id);
        var addAdminRoleList = model.RoleGroup.Select(item => new SysAdminRole() { AdminId = model.Id, RoleId = long.Parse(item) }).ToList();
        await _adminRoleRepository.InsertRangeAsync(addAdminRoleList);
    }

    /// <summary>
    /// 删除,支持多个
    /// </summary>
    /// <param name="ids">逗号分隔</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task DeleteAsync(string ids)
    {
        await _thisRepository.DeleteAsync(m=>ids.StrToListLong().Contains(m.Id));
    }
}
