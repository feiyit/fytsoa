using System.Text.Json;
using FytSoa.Application.Sys;
using FytSoa.Common.Enum;
using FytSoa.Common.Utils;
using FytSoa.Domain.Cms;
using FytSoa.Common.Jwt;
using FytSoa.Common.Jwt.Model;
using FytSoa.Common.Result;
using FytSoa.Common.Tenant;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Operator;

/// <summary>
/// 操作人服务
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class OperatorService : IApplicationService
{
    readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SysAdminService _adminService;
    private readonly SysOrganizeService _organizeService;
    private readonly SysLogService _logService;
    private readonly SugarRepository<SysOrganize> _thisRepository;

    public OperatorService(IHttpContextAccessor httpContextAccessor
        , SysAdminService adminService
        , SysOrganizeService organizeService
        , SysLogService logService
        , SugarRepository<SysOrganize> thisRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _adminService = adminService;
        _organizeService = organizeService;
        _logService = logService;
        _thisRepository = thisRepository;
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
            { Id = loginRes.Id, FullName = loginRes.FullName, Role = "Admin", RoleArray = loginRes.RoleGroup.Count>0?string.Join(",",loginRes.RoleGroup):"0", TenantId = loginRes.TenantId, Time = DateTime.Now });
        
        #region 登录日志收集
        await _logService.AddAsync(new SysLogDto()
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
            Avatar = loginRes.Avatar
        }};
    }
    
    /// <summary>
    /// 工作台用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<OperatorWorkDto> UserWord()
    {
        var token = AppUtils.Token;
        if (token.Id==0)
        {
            return new OperatorWorkDto();
        }
        var model = await _adminService.GetAsync(token.Id);
        var organize = await _organizeService.GetAsync(model.OrganizeId);
        var roleList = await _thisRepository.Context.Queryable<SysRole>()
            .Where(m => model.RoleGroup.Contains(m.Id.ToString())).ToListAsync();
        var postList = await _thisRepository.Context.Queryable<SysPost>()
            .Where(m => model.PostGroup.Contains(m.Id.ToString())).ToListAsync();
        var messageCount = await _thisRepository.Context.Queryable<SysMessage>().CountAsync(m => true);
        var agencyCount = await _thisRepository.Context.Queryable<SysMessage>().CountAsync(m => !m.IsRead);
        return new OperatorWorkDto()
        {
            Account = model.LoginAccount,
            fullName = model.FullName,
            sex = model.Sex,
            headPic = model.Avatar,
            organize = organize.Name,
            summary = model.Summary,
            role = roleList.Select(m => m.Name).ToList(),
            post = postList.Select(m => m.Name).ToList(),
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
    public ApiResult<int> Logout()
    {
        return JResult<int>.Success();
    }
}