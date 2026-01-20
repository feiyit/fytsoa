using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;

[ApiExplorerSettings(GroupName = "v1")]
public class SysFileService:IApplicationService
{
    private readonly SysSafetyService _safetyService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public SysFileService(SysSafetyService safetyService
        ,IHttpContextAccessor httpContextAccessor)
    {
        _safetyService= safetyService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// 查询目录-树形结构
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public List<FileUtils.FileDirectory> GetDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = AppUtils.AppRoot+"/upload/";
        }
        return FileUtils.GetFileDirectory(path);
    }

    /// <summary>
    /// 根据目录查询文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public List<FileUtils.ArrayFiles> GetList(string path)
    {
        if (string.IsNullOrEmpty(path) || path=="/")
        {
            path = AppUtils.AppRoot+"/upload/";
        }
        return FileUtils.GetDirs(path);
    }
    
    /// <summary>
    /// 根据目录查询文件
    /// </summary>
    /// <returns></returns>
    public List<FileUtils.FilesInfo> GetFiles(string path,string filetype)
    {
        var basePath = AppUtils.AppRoot;
        if (string.IsNullOrEmpty(path) || path=="/")
        {
            path = basePath + "/upload/";
        }
        else
        {
            path = basePath + path;
        }
        return FileUtils.ResolveFileInfo(path, filetype,basePath);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns></returns>
    public async Task<SysFileDto> Uploading([FromForm]IFormFile file, string path)
    {
        if (file==null)
        {
            throw new BusinessException("请上传文件~");
        }
        var safety = _safetyService.Get();
        //原文件名
        var filename = file.FileName;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        //扩展名
        var fileExt = FileUtils.GetFileExt(filename);
        if (!string.IsNullOrEmpty(safety.UploadWhitelist))
        {
            var arr = safety.UploadWhitelist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (!arr.Contains(fileExt))
            {
                throw new ArgumentException("文件类型不存在【上传白名单】中，不允许上传~");
            }
        }
        filename = fileNameWithoutExtension+"_"+DateTime.Now.GetTimeStamp() + "." + fileExt;
        var basePath =AppUtils.AppRoot+path;
        FileUtils.CreateSuffic(basePath);
        await using (var stream = new FileStream(basePath + filename, FileMode.Create))
        {
            await file.CopyToAsync(stream);
            stream.Flush();
        }
        return new SysFileDto()
        {
            path = path + filename,
            name = filename,
            size = Convert.ToInt64(Math.Round(Convert.ToDecimal(file.Length / 1024), 0))
        };
    }
    
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns></returns>
    public async Task<SysFileDto> Upload(string path)
    {
        var httpFile = _httpContextAccessor.HttpContext?.Request.Form.Files[0];
        if (httpFile==null)
        {
            throw new ArgumentException("上传文件不能为空~");
        }
        var safety = _safetyService.Get();
        //原文件名
        var filename = httpFile.FileName;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        var fileExt = FileUtils.GetFileExt(filename);
        if (!string.IsNullOrEmpty(safety.UploadWhitelist))
        {
            var arr = safety.UploadWhitelist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (!arr.Contains(fileExt))
            {
                throw new ArgumentException("文件类型不存在【上传白名单】中，不允许上传~");
            }
        }
        filename = fileNameWithoutExtension+"_"+DateTime.Now.GetTimeStamp() + "." + fileExt;
        var basePath =AppUtils.AppRoot+path;
        FileUtils.CreateSuffic(basePath);
        await using (var stream = new FileStream(basePath + filename, FileMode.Create))
        {
            await httpFile.CopyToAsync(stream);
            stream.Flush();
        }
        //System.Threading.Thread.Sleep(1000*60);
        return new SysFileDto()
        {
            path = path + filename,
            name = filename,
            size = Convert.ToInt64(Math.Round(Convert.ToDecimal(httpFile.Length / 1024), 0))
        };
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <returns></returns>
    public void DeleteFile(string path)
    {
        var basePath =AppUtils.AppRoot+path;
        FileUtils.DeleteFile(basePath);
    }

    /// <summary>
    /// 删除目录及目录下文件
    /// </summary>
    /// <returns></returns>
    public void DeleteDirectory(string path)
    {
        var basePath =AppUtils.AppRoot+path;
        FileUtils.ClearDirectory(basePath);
        FileUtils.DeleteDirectory(basePath);
    }
}