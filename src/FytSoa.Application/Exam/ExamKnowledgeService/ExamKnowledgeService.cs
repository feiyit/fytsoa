using FytSoa.Application.Exam.Param;
using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.User;
using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 知识库服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamKnowledgeService : IApplicationService 
{
    private readonly SugarRepository<ExamKnowledge> _thisRepository;
    readonly IHttpContextAccessor _httpContext;
    public ExamKnowledgeService(SugarRepository<ExamKnowledge> thisRepository
    ,IHttpContextAccessor httpContext)
    {
        _thisRepository = thisRepository;
        _httpContext = httpContext;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamKnowledgeDto>> GetPagesAsync(ExamKnowledgeParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.GradeId!=0,m=>m.GradeId==param.GradeId)
            .WhereIF(param.CategoryId!=0,m=>m.CategoryId==param.CategoryId)
            .Includes(m=>m.GrandCode)
            .Includes(m=>m.Category)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamKnowledgeDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamKnowledgeDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamKnowledgeDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamKnowledgeDto model)
    {
        if (string.IsNullOrEmpty(model.Document))
            return await _thisRepository.InsertAsync(model.Adapt<ExamKnowledge>());
        var host = _httpContext.HttpContext?.Request.Scheme+"://"+_httpContext.HttpContext?.Request.Host;
        model.PageCount = PdfUtils.GetPdFofPageCount(AppUtils.AppRoot+model.Document.Replace(host, ""));
        return await _thisRepository.InsertAsync(model.Adapt<ExamKnowledge>());
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamKnowledgeDto model)
    {
        if (string.IsNullOrEmpty(model.Document))
            return await _thisRepository.UpdateAsync(model.Adapt<ExamKnowledge>());
        var host = _httpContext.HttpContext?.Request.Scheme+"://"+_httpContext.HttpContext?.Request.Host;
        model.PageCount = PdfUtils.GetPdFofPageCount(AppUtils.AppRoot+model.Document.Replace(host, ""));
        return await _thisRepository.UpdateAsync(model.Adapt<ExamKnowledge>());
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
