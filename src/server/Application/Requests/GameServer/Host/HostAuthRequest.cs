namespace Application.Requests.GameServer.Host;

public class HostAuthRequest
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}