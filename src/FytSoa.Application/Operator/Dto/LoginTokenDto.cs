namespace FytSoa.Application.Operator;

public class LoginTokenDto
{
    public string accessToken { get; set; }
    
    public OperatorUser userInfo { get; set; }
}