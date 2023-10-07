namespace Application.Requests.v1.GameServer;

public class HostRegisterRequest
{
    public Guid HostId { get; set; }
    public string RegisterToken { get; set; } = null!;
}