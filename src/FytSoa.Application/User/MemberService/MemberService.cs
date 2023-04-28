using FytSoa.Domain.User;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Result;
using Mapster;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.User;

/// <summary>
/// 会员表服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v3")]
public class MemberService : IApplicationService 
{
    private readonly SugarRepository<Member> _thisRepository;
    public MemberService(SugarRepository<Member> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<MemberDto>> GetPagesAsync(MemberPageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m =>
                m.NickName.Contains(param.Key) || m.WxName.Contains(param.Key) || m.Mobile.Contains(param.Key))
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<MemberDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<MemberDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<MemberDto>();
    }

    /// <summary>
    /// 根据手机号码，查询用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<MemberDto> GetByMobileAsync(string mobile)
    {
        var model = await _thisRepository.GetFirstAsync(m=>m.Mobile==mobile);
        return model.Adapt<MemberDto>();
    }
    
    /// <summary>
    /// 登录方法
    /// </summary>
    /// <returns></returns>
    public async Task<MemberDto> GetSiteLoginAsync(MemberLoginParam param)
    {
        var model = await _thisRepository.GetFirstAsync(m=>m.Email==param.Email);
        if (model==null)
        {
            return new MemberDto();
        }
        return model.LoginPwd!=param.PassWord.AESEncrypt() ? new MemberDto() : model.Adapt<MemberDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(MemberDto model)
    {
        var any = await _thisRepository.IsAnyAsync(m => m.LoginName == model.LoginName || m.Email == model.Email);
        if (any)
        {
            return false;
        }
        return await _thisRepository.InsertAsync(model.Adapt<Member>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(MemberDto model)
    {
        return await _thisRepository.UpdateAsync(model.Adapt<Member>());
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
}
