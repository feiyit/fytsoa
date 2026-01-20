namespace FytSoa.Common.Extensions;

/// <summary>
/// 业务抛出异常信息
/// </summary>
public class BusinessException:Exception
{
    private readonly string _message;

    public BusinessException(string message) : base()
    {
        _message = message;
    }

    public string GetMessage()
    {
        return _message;
    }
}