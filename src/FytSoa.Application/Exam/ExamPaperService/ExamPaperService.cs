using FytSoa.Application.Exam.Param;
using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 试卷服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamPaperService : IApplicationService 
{
    private readonly SugarRepository<ExamPaper> _thisRepository;
    public ExamPaperService(SugarRepository<ExamPaper> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamPaperDto>> GetPagesAsync(ExamPaperPageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.subject!=0,m=>m.SubjectId==param.subject)
            .WhereIF(param.TypeId!=0,m=>m.TypeId==param.TypeId)
            .WhereIF(param.Status=="1",m=>m.Status)
            .WhereIF(param.Status=="0",m=>!m.Status)
            .WhereIF(!string.IsNullOrEmpty(param.grand),m=>param.grand.StrToListLong().Contains(m.GrandId))
            .Includes(m=>m.GrandCode)
            .Includes(m=>m.SubjectCode)
            .Includes(m=>m.TypeCode)
            .ToPageAsync(param.Page, param.Limit);

        if (!param.IsUserLog) return query.Adapt<PageResult<ExamPaperDto>>();
        {
            var userLogRepository = _thisRepository.ChangeRepository<SugarRepository<ExamUserlog>>();
            await _thisRepository.Context.ThenMapperAsync(query.Items, async item =>
            {
                item.UserNumber = await userLogRepository.CountAsync(m => m.PaperId == item.Id);
            });
        }
        return query.Adapt<PageResult<ExamPaperDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamPaperDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamPaperDto>();
    }
    
    /// <summary>
    /// 根据主键查询,包括用户考试次数
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamPaperDto> GetPaperAsync(long id)
    {
        var model = await _thisRepository.AsQueryable()
            .Where(m => m.Id == id)
            .Includes(m => m.GrandCode)
            .Includes(m => m.SubjectCode)
            .Includes(m=>m.TypeCode)
            .FirstAsync();
        var userLogRepository = _thisRepository.ChangeRepository<SugarRepository<ExamUserlog>>();
        model.UserNumber=await userLogRepository.CountAsync(m => m.PaperId == id);
        return model.Adapt<ExamPaperDto>();
    }



    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamPaperDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamPaper>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamPaperDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamPaper>());
    
    /// <summary>
    /// 发布
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<bool> ModifyReleaseAsync(List<long> ids) =>
        await _thisRepository.UpdateAsync(m=>new ExamPaper()
        {
            Status = true
        },m=>ids.Contains(m.Id));
    
    /// <summary>
    /// 撤销
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<bool> ModifyUndoAsync(List<long> ids) =>
        await _thisRepository.UpdateAsync(m=>new ExamPaper()
        {
            Status = false
        },m=>ids.Contains(m.Id));

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
