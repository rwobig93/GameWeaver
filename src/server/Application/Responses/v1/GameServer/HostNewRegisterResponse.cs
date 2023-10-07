namespace Application.Responses.v1.GameServer;

public class HostNewRegisterResponse
{
    public Guid HostId { get; set; }
    public string Key { get; set; } = null!;
    public string RegisterUrl { get; set; } = null!;
}