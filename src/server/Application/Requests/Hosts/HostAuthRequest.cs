namespace Application.Requests.Hosts;

public class HostAuthRequest
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}