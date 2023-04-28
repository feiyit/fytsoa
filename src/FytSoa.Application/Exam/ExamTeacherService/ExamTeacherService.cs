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
/// 讲师服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamTeacherService : IApplicationService 
{
    private readonly SugarRepository<ExamTeacher> _thisRepository;
    public ExamTeacherService(SugarRepository<ExamTeacher> thisRepository)
    {
        _thisRepository = thisRepository;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamTeacherDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .Includes(m=>m.ProfessionCode)
            .OrderBy(m=>m.Id,OrderByType.Desc)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamTeacherDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamTeacherDto> GetAsync(long id)
    {
        var model = await _thisRepository.AsQueryable()
            .Includes(m=>m.ProfessionCode)
            .FirstAsync(m=>m.Id==id);
        return model.Adapt<ExamTeacherDto>();
    }

    /// <summary>
    /// 增加访问量
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAddHitsAsync(long id) => await _thisRepository.UpdateAsync(m => new ExamTeacher()
    {
        Hits = m.Hits+1
    },m=>m.Id==id);
    
    /// <summary>
    /// 增加关注数
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAddFocusAsync(long id) => await _thisRepository.UpdateAsync(m => new ExamTeacher()
    {
        Focus = m.Focus+1
    },m=>m.Id==id);

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamTeacherDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamTeacher>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamTeacherDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamTeacher>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
}
