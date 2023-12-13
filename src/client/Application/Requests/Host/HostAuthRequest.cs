namespace Application.Requests.Host;

public class HostAuthRequest
{
    public Guid HostId { get; set; }
    public string HostToken { get; set; } = null!;
}