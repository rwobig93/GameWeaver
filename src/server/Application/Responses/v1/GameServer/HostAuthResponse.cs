namespace Application.Responses.v1.GameServer;

public class HostAuthResponse
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; }
}