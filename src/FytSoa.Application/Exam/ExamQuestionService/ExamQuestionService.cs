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
/// 考试题服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamQuestionService : IApplicationService 
{
    private readonly SugarRepository<ExamQuestion> _thisRepository;
    public ExamQuestionService(SugarRepository<ExamQuestion> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamQuestionDto>> GetPagesAsync(ExamQuestionParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.subject!=0,m=>m.SubjectId==param.subject)
            .WhereIF(param.Type!=0,m=>m.Type==param.Type)
            .WhereIF(!string.IsNullOrEmpty(param.grand),m=>param.grand.StrToListLong().Contains(m.GrandId))
            .Includes(m=>m.GrandCode)
            .Includes(m=>m.SubjectCode)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamQuestionDto>>();
    }
    
    /// <summary>
    /// 查询所有
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<ExamQuestionDto>> GetListAsync(ExamQuestionParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.subject!=0,m=>m.SubjectId==param.subject)
            .WhereIF(param.Type!=0,m=>m.Type==param.Type)
            .WhereIF(!string.IsNullOrEmpty(param.grand),m=>param.grand.StrToListLong().Contains(m.GrandId))
            .WhereIF(!string.IsNullOrEmpty(param.IdArr),m=>param.IdArr.StrToListLong().Contains(m.Id))
            .Includes(m=>m.GrandCode)
            .Includes(m=>m.SubjectCode)
            .ToListAsync();
        return query.Adapt<List<ExamQuestionDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamQuestionDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamQuestionDto>();
    }
    
    /// <summary>
    /// 智能组卷
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<List<ExamQuestionDto>> PostSmartAsync(ExamSmartParam param)
    {
        var list = new List<ExamQuestionDto>();
        var exp = new Expressionable<ExamQuestion>();
        if (param.SmartGrand!=0)
        {
            exp.And(m => m.GrandId == param.SmartGrand);
        }
        if (param.SmartSubject!=0)
        {
            exp.And(m => m.SubjectId == param.SmartSubject);
        }
        if (param.Difficulty!=0)
        {
            exp.And(m => m.Difficulty == param.Difficulty);
        }
        if (param.Single>0)
        {
            var query = await _thisRepository.AsQueryable()
                .Where(exp.ToExpression())
                .Where(m=>m.Type==1)
                .OrderBy(m=>m.Id,OrderByType.Desc)
                .Take(param.Single)
                .ToListAsync();
            list.AddRange(query.Adapt<List<ExamQuestionDto>>());
        }
        if (param.Multiple>0)
        {
            var query = await _thisRepository.AsQueryable()
                .Where(exp.ToExpression())
                .Where(m=>m.Type==2)
                .OrderBy(m=>m.Id,OrderByType.Desc)
                .Take(param.Multiple)
                .ToListAsync();
            list.AddRange(query.Adapt<List<ExamQuestionDto>>());
        }
        if (param.Judge>0)
        {
            var query = await _thisRepository.AsQueryable()
                .Where(exp.ToExpression())
                .Where(m=>m.Type==3)
                .OrderBy(m=>m.Id,OrderByType.Desc)
                .Take(param.Judge)
                .ToListAsync();
            list.AddRange(query.Adapt<List<ExamQuestionDto>>());
        }
        if (param.GapFilling>0)
        {
            var query = await _thisRepository.AsQueryable()
                .Where(exp.ToExpression())
                .Where(m=>m.Type==4)
                .OrderBy(m=>m.Id,OrderByType.Desc)
                .Take(param.GapFilling)
                .ToListAsync();
            list.AddRange(query.Adapt<List<ExamQuestionDto>>());
        }
        if (param.Explain>0)
        {
            var query = await _thisRepository.AsQueryable()
                .Where(exp.ToExpression())
                .Where(m=>m.Type==5)
                .OrderBy(m=>m.Id,OrderByType.Desc)
                .Take(param.Explain)
                .ToListAsync();
            list.AddRange(query.Adapt<List<ExamQuestionDto>>());
        }
        return list;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamQuestionDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamQuestion>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamQuestionDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamQuestion>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
