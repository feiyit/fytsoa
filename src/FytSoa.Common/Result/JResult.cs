using System.Net;

namespace FytSoa.Common.Result;

public class JResult<TObject>
{
    public static ApiResult<TObject> Error(string errMsg)
    {
        return  new ApiResult<TObject>
        {
            Code = -1,
            Message = errMsg
        };
    }

    public static ApiResult<TObject> Error()
    {
        return new ApiResult<TObject>
        {
            Code = (int)HttpStatusCode.InternalServerError,
            Message = "服务端发生错误"
        };
    }

    public static ApiResult<TObject> Success()
    {
        return new ApiResult<TObject>
        {
            Code = (int)HttpStatusCode.OK
        };
    }

    public static ApiResult<TObject> Success(TObject resultData)
    {
        return new ApiResult<TObject>
        {
            Code = (int)HttpStatusCode.OK,
            Data=resultData
        };
    }
}