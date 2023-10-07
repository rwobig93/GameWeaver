namespace Application.Requests.v1.GameServer;

public class HostAuthRequest
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}