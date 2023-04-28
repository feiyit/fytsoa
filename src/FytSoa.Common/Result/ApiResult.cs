using System.Net;

namespace FytSoa.Common.Result;

/// <summary>
/// API 返回JSON格式
/// </summary>
public class ApiResult<TObject>
{
    /// <summary>
    /// PageResult
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; } = (int)HttpStatusCode.InternalServerError;

    /// <summary>
    /// 数据集
    /// </summary>
    public TObject Data { get; set; }
        
}