namespace FytSoa.Application;

public class SliderVerifyRequest
{
    public string? Account { get; set; }
}

public class SliderVerifyResponse
{
    public string SliderToken { get; set; } = string.Empty;
}