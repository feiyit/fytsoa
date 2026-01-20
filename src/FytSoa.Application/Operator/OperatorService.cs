using System.Text.Json;
using DotNetCore.CAP;
using FytSoa.Application.Sys;
using FytSoa.Common.Cache;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Common.Jwt;
using FytSoa.Common.Jwt.Model;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Operator;

/// <summary>
/// 操作人服务
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class OperatorService : IApplicationService
{
    private readonly SysAdminService _adminService;
    private readonly SysLogService _logService;
    private readonly SugarRepository<SysAdmin> _thisRepository;
    private readonly ICapPublisher _capBus;
    private readonly ICacheService _cacheService;


    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="adminService"></param>
    /// <param name="logService"></param>
    /// <param name="thisRepository"></param>
    public OperatorService(SysAdminService adminService
        , SysLogService logService
        ,SugarRepository<SysAdmin> thisRepository
        , ICapPublisher capBus
        ,ICacheService cacheService)
    {
        _adminService = adminService;
        _logService = logService;
        _thisRepository = thisRepository;
        _capBus = capBus;
        _cacheService=cacheService;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginParam"></param>
    /// <returns></returns>
    [AllowAnonymous]
    public async Task<LoginTokenDto> LoginAsync(LoginParam loginParam)
    {
        var loginRes = await _adminService.LoginAsync(loginParam);

        var token = JwtAuthService.IssueJwt(new JwtToken()
            { Id = loginRes.Id, FullName = loginRes.FullName, Role = "Admin", RoleArray = loginRes.RoleGroup.Count>0?string.Join(",",loginRes.RoleGroup):"0", TenantId = loginRes.TenantId,EmployeeId = 0, Time = DateTime.Now.AddMinutes(60) });
        
        #region 登录日志收集
        await _capBus.PublishAsync("log.cap", new SysLogDto()
        {
            Level = LogEnum.Info,
            LogType=LogTypeEnum.Login,
            Module="Login",
            Method="Login",
            OperateUser=loginRes.FullName,
            Parameters=JsonSerializer.Serialize(loginParam),
            IP = CommonUtils.GetIp(),
            Address ="/api/login",
            Browser = CommonUtils.GetBrowser(),
        });
        #endregion
        
        return new LoginTokenDto() { accessToken = token,userInfo = new OperatorUser()
        {
            Id = loginRes.Id,
            TenantId = loginRes.TenantId,
            Username = loginRes.FullName,
            Email = loginRes.Email,
            Avatar = loginRes.Avatar,
            OrgName = loginRes.OrganizeObj?.Name
        }};
    }
    
    /// <summary>
    /// 获得用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<OperatorUser> GetUserInfo()
    {
        var id = AppUtils.LoginId;
        Console.WriteLine("用户ID："+id);
        if (id==0)
        {
            return new OperatorUser();
        }
        var model = await _adminService.GetAsync(id);
        return new OperatorUser()
        {
            Id = model.Id,
            TenantId = model.TenantId,
            Username = model.FullName,
            Email = model.Email,
            Avatar = model.Avatar,
            OrgName = model.OrganizeObj?.Name
        };
    }
    
    /// <summary>
    /// 工作台用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<OperatorWorkDto> UserWord()
    {
        var token = AppUtils.Token;
        Console.WriteLine("Token内容："+token.Id);
        if (token.Id==0)
        {
            return new OperatorWorkDto();
        }
        var model = await _adminService.GetAsync(token.Id);
        var roleList = await _thisRepository.Context.Queryable<SysRole>()
            .Where(m => model.RoleGroup.Contains(m.Id.ToString())).ToListAsync();
        var messageCount = await _thisRepository.Context.Queryable<SysMessage>().CountAsync(m => true);
        var agencyCount = await _thisRepository.Context.Queryable<SysMessage>().CountAsync(m => !m.IsRead);
        return new OperatorWorkDto()
        {
            Account = model.LoginAccount,
            fullName = model.FullName,
            sex = model.Sex,
            headPic = model.Avatar,
            summary = model.Summary,
            role = roleList.Select(m => m.Name).ToList(),
            lastTime = model.UpLoginTime,
            loginSum = model.LoginCount,
            messageSum = messageCount,
            agencySum = agencyCount
        };
    }

    /// <summary>
    /// 退出
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task Logout()
    {
        var token = AppUtils.TokenString;
        if (!string.IsNullOrEmpty(token))
        {
            await _cacheService.SetAsync("blacklist:" + token, "invalid", TimeSpan.FromHours(2));
        }
    }
}