
namespace Identity.Core.Options;

public class JwtTokenOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpieryInMinutes { get; set; }
    public int RefreshTokenExpieryInDays { get; set; }
}
