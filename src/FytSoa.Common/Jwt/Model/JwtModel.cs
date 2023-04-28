namespace FytSoa.Common.Jwt.Model;

public class JwtModel
{
    public const string Name = "JwtAuth";
    public string Security { get; set; }

    public string Issuer { get; set; }

    public string Audience { get; set; }

    public int WebExp { get; set; }
}