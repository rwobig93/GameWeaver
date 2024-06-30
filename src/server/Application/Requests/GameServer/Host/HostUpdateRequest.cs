namespace Application.Requests.GameServer.Host;

public class HostUpdateRequest
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? AllowedPorts { get; set; }
}