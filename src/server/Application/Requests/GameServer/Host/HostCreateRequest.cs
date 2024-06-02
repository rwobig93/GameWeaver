namespace Application.Requests.GameServer.Host;

public class HostCreateRequest
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = "";
    public List<string> AllowedPorts { get; set; } = [];
}