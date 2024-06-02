namespace Application.Requests.GameServer.Host;

public class HostRegistrationCreateRequest
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<string> AllowedPorts { get; set; } = [];
}