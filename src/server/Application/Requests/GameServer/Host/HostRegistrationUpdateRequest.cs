namespace Application.Requests.GameServer.Host;

public class HostRegistrationUpdateRequest
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public bool? Active { get; set; }
}