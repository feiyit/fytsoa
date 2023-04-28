using FytSoa.Application.Operator;
using FytSoa.Application.Sys.Param;
using FytSoa.Common.Extensions;
using FytSoa.Domain.Sys;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Sys;

/// <summary>
/// 通知模块服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v1")]
public class SysNoticeService : IApplicationService 
{
    private readonly SugarRepository<SysNotice> _thisRepository;
    private readonly SugarRepository<SysNoticeRead> _readRepository;
    private readonly SugarRepository<SysAdmin> _adminRepository;
    public SysNoticeService(SugarRepository<SysNotice> thisRepository
        ,SugarRepository<SysNoticeRead> readRepository
        ,SugarRepository<SysAdmin> adminRepository)
    {
        _thisRepository = thisRepository;
        _readRepository = readRepository;
        _adminRepository = adminRepository;
    }
    
    /// <summary>
    /// 查询所有——分页== 默认查询收件箱
    /// status=（1=草稿2=存档3=删除）
    /// type=(1=已发送 2=收件箱  3=带文件)
    /// ReadStatus=(1=未读 2=已读)
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<SysNoticeDto>> GetPagesAsync(NoticeParam param)
    {
        param.Id =  AppUtils.LoginId;
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key) || m.Content.Contains(param.Key))
            // 草稿等
            .WhereIF(param.Status=="1",m=>m.Status==int.Parse(param.Status) && m.SendUserId==param.Id && m.IsSend)
            .WhereIF(param.Status=="2",m=>m.Status==int.Parse(param.Status) && m.SendUserId!=param.Id && !m.IsSend && (SqlFunc.ToString(m.AcceptUserIds).Contains(param.Id.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]"))
            .WhereIF(param.Status=="3",m=>m.Status==int.Parse(param.Status) && !m.IsSend && (SqlFunc.ToString(m.AcceptUserIds).Contains(param.Id.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]"))
            // 查询发件人=已发送
            .WhereIF(param.Type==1,m=>m.SendUserId==param.Id && m.Status==0 && m.IsSend)
            // 查询收件人=收件箱
            .WhereIF(param.Type==2,m=>(SqlFunc.ToString(m.AcceptUserIds).Contains(param.Id.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]") && m.SendUserId!=param.Id && m.Status==0 && !m.IsSend)
            // 查询 带文件的通知
            .WhereIF(param.Type==3,m=> !SqlFunc.IsNullOrEmpty(m.Files) && (SqlFunc.ToString(m.AcceptUserIds).Contains(param.Id.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]")  && m.SendUserId!=param.Id  && m.Status==0 && !m.IsSend)
            // 未读=1 已读=2  全部=0
            .WhereIF(param.ReadStatus==1,m=>SqlFunc.Subqueryable<SysNoticeRead>().Where(s=>s.UserId==param.Id && s.IsRead && s.NoticeId==m.Id).Count()==0)
            .WhereIF(param.ReadStatus==2,m=>SqlFunc.Subqueryable<SysNoticeRead>().Where(s=>s.UserId==param.Id && s.IsRead && s.NoticeId==m.Id).Count()>0)
            .Includes(m=>m.SendUser)
            .OrderByDescending(m=>m.Id)
            .ToPageAsync(param.Page, param.Limit);
        var result=query.Adapt<PageResult<SysNoticeDto>>();
        
        //查询已读
        var noticeIds = result.Items.Select(m => m.Id).ToList();
        var readList =
            await _readRepository.GetListAsync(m => m.IsRead && noticeIds.Contains(m.NoticeId) && m.UserId == param.Id);
        
        foreach (var item in result.Items)
        {
            //是否已读
            item.IsRead = readList.Any(m => m.NoticeId == item.Id);
        }
        return result;
    }

    /// <summary>
    /// 查询未读、草稿、删除、存档统计数据
    /// </summary>
    /// <returns></returns>
    public async Task<SysNoticeTotalDto> GetTotalAsync()
    {
        var userId = AppUtils.LoginId;
        return new SysNoticeTotalDto()
        {
            Unread=await _thisRepository.CountAsync(m=>SqlFunc.Subqueryable<SysNoticeRead>().Where(s=>s.UserId==userId && s.IsRead && m.Id==s.NoticeId).Count()==0 
                                                       && m.SendUserId!=userId && m.Status==0 && !m.IsSend 
                                                       && (SqlFunc.ToString(m.AcceptUserIds).Contains(userId.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]")),
            Draft = await _thisRepository.CountAsync (m => m.SendUserId==userId && m.Status==1 && m.IsSend),
            Archive = await _thisRepository.CountAsync (m => SqlFunc.ToString(m.AcceptUserIds).Contains(userId.ToString()) && m.Status==2),
            Delete = await _thisRepository.CountAsync (m => SqlFunc.ToString(m.AcceptUserIds).Contains(userId.ToString()) && m.Status==3 && !m.IsSend)
        };
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">在收件箱，未读的邮件查询，需要传type=1</param>
    /// <returns></returns>
    [HttpGet("{id}/{type?}")]
    public async Task<SysNoticeDto> GetAsync(long id,int type=0)
    {
        var model = await _thisRepository.AsQueryable()
            .Includes(m=>m.SendUser)
            .FirstAsync(m=>m.Id==id);
        var res = model.Adapt<SysNoticeDto>();
        var adminList = await _adminRepository.GetListAsync(m => model.AcceptUserIds.Contains(m.Id));
        res.AcceptUserList = adminList.Adapt<List<SysAdminDto>>();
        if (type==1)
        {
            //增加已读数据
            await _readRepository.InsertAsync(new SysNoticeRead()
            {
                NoticeId = id,
                UserId = AppUtils.LoginId,
                IsRead = true
            });
        }
        return res;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task AddAsync(SysNoticeDto model)
    {
        model.SendUserId = AppUtils.LoginId;
        if (model.SendUserId==0)
        {
            throw new BusinessException("发件人不能为空");
        }
        
        var list = new List<SysNotice>();
        if (model.Status==0)
        {
            //发送所有，不包含自己
            List<long> adminList;
            if (model.AcceptUserIds.Count==1 && model.AcceptUserIds[0]==0)
            {
                adminList=await _adminRepository
                    .AsQueryable()
                    .Where(m=>m.Id!=model.SendUserId)
                    .Select(m=>m.Id)
                    .ToListAsync();
            }
            else
            {
                adminList = model.AcceptUserIds;
            }
            foreach (var item in adminList)
            {
                list.Add(new SysNotice()
                {
                    SendUserId = model.SendUserId,
                    Title = model.Title,
                    Content = model.Content,
                    Files = model.Files,
                    AcceptUserIds = new List<long>(){item},
                    Status = model.Status,
                });
            }
            await _thisRepository.InsertRangeAsync(list);
        }
        //额外增加一条   属于发件信息的标记
        var _model = model.Adapt<SysNotice>();
        _model.IsSend = true;
        await _thisRepository.InsertAsync(_model);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task ModifyAsync(SysNoticeDto model)
    {
        var list = new List<SysNotice>();
        if (model.Status==0)
        {
            //发送所有，不包含自己
            List<long> adminList;
            if (model.AcceptUserIds.Count==1 && model.AcceptUserIds[0]==0)
            {
                adminList=await _adminRepository
                    .AsQueryable()
                    .Where(m=>m.Id!=model.SendUserId)
                    .Select(m=>m.Id)
                    .ToListAsync();
            }
            else
            {
                adminList = model.AcceptUserIds;
            }
            foreach (var item in adminList)
            {
                list.Add(new SysNotice()
                {
                    SendUserId = model.SendUserId,
                    Title = model.Title,
                    Content = model.Content,
                    Files = model.Files,
                    AcceptUserIds = new List<long>(){item},
                    Status = model.Status,
                });
            }
            await _thisRepository.InsertRangeAsync(list);
        }
        var _model = model.Adapt<SysNotice>();
        _model.IsSend = true;
        await _thisRepository.UpdateAsync(_model);
    }
    
    /// <summary>
    /// 更改状态（1=草稿2=存档3=删除）
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<bool> ModifyStatusAsync([FromBody]NoticeStatusParam param) =>
        await _thisRepository.UpdateAsync(m=>new SysNotice()
        {
            Status = param.Status,
            UpdateTime = DateTime.Now
        },m=>param.Ids.Contains(m.Id));
    
    /// <summary>
    /// 设置已读
    /// 空数组为设置全部已读
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task ReadAsync ([FromBody]List<long> ids)
    {
        List<long> noticeIds;
        var list = new List<SysNoticeRead>();
        if (ids.Count>0)
        {
            var noticeList = await _thisRepository.GetListAsync(m => ids.Contains(m.Id));
            noticeIds = noticeList.Select(m => m.Id).ToList();
        }
        else
        {
            var userId= AppUtils.LoginId;
            var noticeList = await _thisRepository.GetListAsync(m=>SqlFunc.ToString(m.AcceptUserIds).Contains(userId.ToString()) || SqlFunc.ToString(m.AcceptUserIds)=="[0]" 
                && SqlFunc.Subqueryable<SysNoticeRead>().Where(s=>s.UserId==userId && s.IsRead && s.NoticeId==m.Id).Count()==0);
            noticeIds = noticeList.Select(m => m.Id).ToList();
        }

        if (noticeIds.Count>0)
        {
            foreach (var item in noticeIds)
            {
                list.Add(new SysNoticeRead()
                {
                    Id = Unique.Id(),
                    NoticeId = item,
                    UserId = AppUtils.LoginId,
                    IsRead = true
                });
            }

            await _readRepository.InsertRangeAsync(list);
        }
    }

    /// <summary>
    /// 取消已读
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    public async Task ClearReadAsync([FromBody] List<long> ids)
    {
        await _readRepository.DeleteAsync(m => ids.Contains(m.NoticeId) && m.UserId == AppUtils.LoginId);
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
