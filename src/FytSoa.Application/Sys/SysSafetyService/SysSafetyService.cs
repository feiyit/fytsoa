using System.Text;
using System.Text.Json;
using FytSoa.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FytSoa.Application.Sys;
[ApiExplorerSettings(GroupName = "v1")]
public class SysSafetyService : IApplicationService
{
    private readonly string _filePath=AppUtils.AppRoot+"/upload/config/security_config.txt";

    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    public SafetySetting Get()
    {
        if (!File.Exists(_filePath))
        {
            return new SafetySetting();
        }
        var jsonContent = File.ReadAllText(_filePath);
        if (string.IsNullOrEmpty(jsonContent)) return new SafetySetting();
        return JsonSerializer.Deserialize<SafetySetting>(jsonContent);
    }
        
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public void Add(SafetySetting config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "配置对象不能为空");
        var jsonContent = JsonSerializer.Serialize(config);
        FileUtils.CreateFile(_filePath,Encoding.UTF8.GetBytes(jsonContent));
    }
}