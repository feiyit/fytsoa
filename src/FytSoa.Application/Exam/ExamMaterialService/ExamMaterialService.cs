using AngleSharp.Text;
using FytSoa.Application.Exam.Param;
using FytSoa.Application.Sys;
using FytSoa.Common.Extensions;
using FytSoa.Domain.Exam;
using FytSoa.Sugar;
using FytSoa.Common.Utils;
using FytSoa.Common.Param;
using FytSoa.Common.Result;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace FytSoa.Application.Exam;

/// <summary>
/// 素材服务接口
/// </summary>
[ApiExplorerSettings(GroupName = "v6")]
public class ExamMaterialService : IApplicationService 
{
    private readonly SysSafetyService _safetyService;
    private readonly SugarRepository<ExamMaterial> _thisRepository;
    public ExamMaterialService(SugarRepository<ExamMaterial> thisRepository
    , SysSafetyService safetyService)
    {
        _thisRepository = thisRepository;
        _safetyService= safetyService;
    }
    
    /// <summary>
    /// 查询所有——分页
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<PageResult<ExamMaterialDto>> GetPagesAsync(PageParam param)
    {
        var query = await _thisRepository.AsQueryable()
            .WhereIF(param.Id!=0,m=>m.CategoryId==param.Id)
            .WhereIF(!string.IsNullOrEmpty(param.Key),m=>m.Name.Contains(param.Key) || m.SourceName.Contains(param.Key))
            .Includes(m=>m.Category)
            .ToPageAsync(param.Page, param.Limit);
        return query.Adapt<PageResult<ExamMaterialDto>>();
    }

    /// <summary>
    /// 根据主键查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ExamMaterialDto> GetAsync(long id)
    {
        var model = await _thisRepository.GetByIdAsync(id);
        return model.Adapt<ExamMaterialDto>();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ExamMaterialDto model) =>
        await _thisRepository.InsertAsync(model.Adapt<ExamMaterial>());

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> ModifyAsync(ExamMaterialDto model) =>
        await _thisRepository.UpdateAsync(model.Adapt<ExamMaterial>());

    /// <summary>
    /// 删除,支持批量
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<bool> DeleteAsync([FromBody]List<long> ids) =>
        await _thisRepository.DeleteAsync(m=>ids.Contains(m.Id));
    
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisableRequestSizeLimit]
    public async Task<SysFileDto> Upload([FromForm]IFormFile file,long categoryId)
    {
        var categoryRepository = _thisRepository.ChangeRepository<SugarRepository<ExamMaterialCategory>>();
        var category =await categoryRepository.GetByIdAsync(categoryId);
        var path = "/upload/knowledge/"+category.EnName+"/";
        var safety = _safetyService.Get();
        //原文件名
        var filename = file.FileName;
        //扩展名
        var fileExt = FileUtils.GetFileExt(filename);
        if (!string.IsNullOrEmpty(safety.UploadWhite))
        {
            var arr = safety.UploadWhite.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (!arr.Contains(fileExt))
            {
                throw new BusinessException("文件类型不存在【上传白名单】中，不允许上传~");
            }
        }
        //判断是否包含盘符： 文件名不允许包含冒号，如果存在，则使用新的文件名字
        if (filename.Contains(":"))
        {
            filename = DateTime.Now.GetTimeStamp() + "." + fileExt;
        }
        var basePath =AppUtils.AppRoot+path;
        FileUtils.CreateSuffic(basePath);
        await using (var stream = new FileStream(basePath + filename, FileMode.Create))
        {
            await file.CopyToAsync(stream);
            stream.Flush();
        }

        await AddAsync(new ExamMaterialDto()
        {
            CategoryId=categoryId,
            Name = file.FileName,
            SourceName = file.FileName,
            Ext = fileExt,
            Size = FileUtils.GetFileSize(basePath + filename),
            Urls = path + filename,
            Type="Upload",
            Audit=true
        });
        return new SysFileDto()
        {
            path = path + filename,
            name = filename,
            size = Convert.ToInt64(Math.Round(Convert.ToDecimal(file.Length / 1024), 0))
        };
    }

    /// <summary>
    /// 重命名
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public async Task<bool> ModifyReNameAsync(ExamMaterialRenameParam param)
    {
        var categoryRepository = _thisRepository.ChangeRepository<SugarRepository<ExamMaterialCategory>>();
        var model = await _thisRepository.GetByIdAsync(param.Id);
        var category =await categoryRepository.GetByIdAsync(model.CategoryId);
        var newName = param.Name + "." + model.Ext;
        var isThere = await _thisRepository.IsAnyAsync(m => m.Name == newName);
        if (isThere)
        {
            throw new BusinessException("已存在相同的文件名！~");
        }
        var path = "/upload/knowledge/"+category.EnName+"/";
        FileUtils.FileReName(AppUtils.AppRoot+model.Name,AppUtils.AppRoot+newName);
        model.Name = newName;
        model.Urls = path + newName;
        return await _thisRepository.UpdateAsync(model);
    }
}
