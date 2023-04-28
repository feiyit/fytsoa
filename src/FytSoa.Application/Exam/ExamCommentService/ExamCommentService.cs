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
/// 评论服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamCommentService : IApplicationService 
{
    private readonly SugarRepository<ExamComment> _thisRepository;
    public ExamCommentService(SugarRepository<ExamComment> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamCommentDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.Id!=0,m=>m.CategoryId==param.Id)
            .WhereIF(!string.IsNullOrEmpty(param.Status),m=>m.Audit==(param.Status=="1"))
            .Includes(m=>m.Course)
            .Includes(m=>m.User)
            .OrderBy(m=>m.CreateTime,OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamCommentDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamCommentDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamCommentDto>();
    }
    
    /// <summary>
    /// 根据分类查询评论总数
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<int> GetCountAsync(long id)
    {
        return await _thisRepository.CountAsync(m=>m.CategoryId==id && m.Audit);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamCommentDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamComment>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamCommentDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamComment>());
    
    /// <summary>
    /// 审核
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAuditAsync(ExamCommentAuditParam param)=> 
        await _thisRepository.UpdateAsync(m => new ExamComment()
        {
            Audit = param.Audit
        },m=>param.Ids.Contains(m.Id));

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
