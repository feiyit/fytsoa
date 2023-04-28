using FytSoa.Application.Exam.Param;
using FytSoa.Application.Sys;
using FytSoa.Application.Sys.Param;
using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using FytSoa.Domain.Sys;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 课程服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamCourseService : IApplicationService 
{
    private readonly SugarRepository<ExamCourse> _thisRepository;
    private readonly SysCodeService _codeService;
    public ExamCourseService(SugarRepository<ExamCourse> thisRepository
    ,SysCodeService codeService)
    {
        _thisRepository = thisRepository;
        _codeService = codeService;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamCourseDto>> GetPagesAsync(ExamCourseSearchParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.Type!=0,m=>m.Type==param.Type)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Title.Contains(param.Key))
            .WhereIF(param.Difficulty!=0,m=>m.DifficultyId==param.Difficulty)
            .WhereIF(!string.IsNullOrEmpty(param.Status),m=>m.Status==int.Parse(param.Status))
            .WhereIF(param.Attr!=0,m=>SqlFunc.ToString(m.Attr).Contains(param.Attr.ToString()))
            .WhereIF(!string.IsNullOrEmpty(param.Audit),m=>m.Audit==(param.Audit=="1"))
            .WhereIF(param.TeacherId!=0,m=>m.TeacherId==param.TeacherId)
            .WhereIF(param.GradeId!=0,m=>SqlFunc.ToString(m.GradeId).Contains(param.GradeId.ToString()))
            .WhereIF(param.SubjectId!=0,m=>SqlFunc.ToString(m.SubjectId).Contains(param.SubjectId.ToString()))
            .Includes(m=>m.Teacher,t=>t.ProfessionCode)
            .Includes(m=>m.Difficulty)
            .ToPageAsync(param.Page, param.Limit);
        var result=query.Adapt<PageResult<ExamCourseDto>>();
        var grade = await _codeService.GetListAsync(new CodePageParam(){TypeCode = "grand"});
        var subject= await _codeService.GetListAsync(new CodePageParam(){TypeCode = "subject"});
        foreach (var item in result.Items)
        {
            item.GradeNames = string.Join(",",grade.Where(m => item.GradeId.Select(long.Parse).Contains(m.Id))
                .Select(m => m.Name));
            item.SubjectNames = string.Join(",",subject.Where(m => item.SubjectId.Select(long.Parse).Contains(m.Id))
                .Select(m => m.Name));
        }
        return result;
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamCourseDto> GetAsync(long id)
    {
        var model = await _thisRepository.AsQueryable()
            .Includes(m=>m.Teacher,t=>t.ProfessionCode)
            .FirstAsync(m=>m.Id==id);
        return model.Adapt<ExamCourseDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamCourseDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamCourse>());
    
    /// <summary>
    /// 增加访问量
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAddHitsAsync(long id) => await _thisRepository.UpdateAsync(m => new ExamCourse()
    {
        Hits = m.Hits+1
    },m=>m.Id==id);

    /// <summary>
    /// 增加访问量
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">1=支持 0=反对</param>
    /// <returns></returns>
    public async Task<bool> ModifySupportAsync(long id,int type)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        if (type==0)
        {
            model.Support.Tread += 1;
        }
        else
        {
            model.Support.Praise += 1;
        }
        return await _thisRepository.UpdateAsync(model);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamCourseDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamCourse>());

    /// <summary>
    /// 审核
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAuditAsync(ExamCourseAuditParam param)=> 
        await _thisRepository.UpdateAsync(m => new ExamCourse()
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
