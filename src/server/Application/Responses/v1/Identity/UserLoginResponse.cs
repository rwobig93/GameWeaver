namespace Application.Responses.v1.Identity;

public class UserLoginResponse
{
    public string ClientId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; }
}