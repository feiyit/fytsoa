namespace FytSoa.Application.Sys;

public class PassResetParam
{
    public List<long> Id { get; set; }
}

public class SetNewPasswordParam
{
    public long Id { get; set; }
    public string SourcePwd { get; set; }
    
    public string PassWord { get; set; }
}