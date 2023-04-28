using FytSoa.Application.Exam.Param;
using FytSoa.Application.User;
using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.User;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 考试记录服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamUserlogService : IApplicationService 
{
    private readonly SugarRepository<ExamUserlog> _thisRepository;
    public ExamUserlogService(SugarRepository<ExamUserlog> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamUserlogDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),
                m=>SqlFunc.Subqueryable<Member>().Where(s=>s.Id==m.UserId && s.NickName.Contains(param.Key) || s.LoginName.Contains(param.Key)).Any())
            .WhereIF(param.Id!=0,m=>m.PaperId==param.Id)
            .WhereIF(param.Status=="1",m=>m.Status)
            .WhereIF(param.Status=="0",m=>!m.Status)
            .Includes(m=>m.UserMember)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamUserlogDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamUserlogDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamUserlogDto>();
    }
    
    /// <summary>
    /// 读取用户考试信息，提供管理员批改
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamUserQuestionDto> GetUserCorrectAsync(long id)
    {
        var resModel = new ExamUserQuestionDto();
        var model = await _thisRepository.GetByIdAsync(id);
        resModel.UserLog = model.Adapt<ExamUserlogDto>();
        
        var userRepository = _thisRepository.ChangeRepository<SugarRepository<Member>>();
        var userModel = await userRepository.GetByIdAsync(model.UserId);
        resModel.User =userModel.Adapt<MemberDto>();
        
        var paperRepository = _thisRepository.ChangeRepository<SugarRepository<ExamPaper>>();
        var paperModel = await paperRepository.GetByIdAsync(model.PaperId);
        resModel.Paper =paperModel.Adapt<ExamPaperDto>();
        
        var questionRepository = _thisRepository.ChangeRepository<SugarRepository<ExamQuestion>>();
        var questionIdArr = model.QuestionItem.Select(m => m.Id).ToList();
        var questionList = await questionRepository.GetListAsync(m => questionIdArr.Contains(m.Id));
        resModel.Question= questionList.Adapt<List<ExamQuestionDto>>();
        return resModel;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamUserlogDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamUserlog>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamUserlogDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamUserlog>());

    /// <summary>
    /// 批改
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<bool> ModifyCorrectAsync(ExamUserCorrectParam param)
    {
        var model = await _thisRepository.GetByIdAsync(param.Id);
        model.QuestionItem = param.QuestionItem;
        model.Status = true;
        return await _thisRepository.UpdateAsync(model);
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
