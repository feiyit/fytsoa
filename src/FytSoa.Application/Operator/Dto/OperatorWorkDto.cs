namespace FytSoa.Application.Operator;

/// <summary>
/// 返回工作台用户信息
/// </summary>
public class OperatorWorkDto
{
    /// <summary>
    /// 登录账号
    /// </summary>
    public string Account { get; set; }
    public string fullName { get; set; }

    public string headPic { get; set; }
    
    public string sex { get; set; }

    public List<string> post { get; set; }

    public List<string> role { get; set; }

    public string organize { get; set; }

    public DateTime? lastTime { get; set; }

    public int loginSum { get; set; } = 0;

    public int messageSum { get; set; } = 0;

    public int agencySum { get; set; } = 0;
    
    public string summary { get; set; }
}