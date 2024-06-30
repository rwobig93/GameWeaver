namespace Application.Requests.GameServer.Host;

public class HostRegistrationConfirmRequest
{
    public Guid HostId { get; set; }
    public string Key { get; set; } = null!;
}