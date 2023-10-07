namespace Application.Responses.v1.Api;

public class ApiTokenResponse
{
    public string Token { get; set; } = null!;
    public DateTime TokenExpiration { get; set; }
}