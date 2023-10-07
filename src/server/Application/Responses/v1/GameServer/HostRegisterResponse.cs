namespace Application.Responses.v1.GameServer;

public class HostRegisterResponse
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}